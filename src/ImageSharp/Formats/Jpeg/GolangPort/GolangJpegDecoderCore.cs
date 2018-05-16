﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.IO;

using SixLabors.ImageSharp.Formats.Jpeg.Components;
using SixLabors.ImageSharp.Formats.Jpeg.Components.Decoder;
using SixLabors.ImageSharp.Formats.Jpeg.GolangPort.Components.Decoder;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.MetaData.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Formats.Jpeg.GolangPort
{
    /// <inheritdoc />
    /// <summary>
    /// Performs the jpeg decoding operation.
    /// </summary>
    internal sealed unsafe class GolangJpegDecoderCore : IRawJpegData
    {
        /// <summary>
        /// The maximum number of color components
        /// </summary>
        public const int MaxComponents = 4;

        /// <summary>
        /// The maximum number of quantization tables
        /// </summary>
        public const int MaxTq = 3;

        /// <summary>
        /// The only supported precision
        /// </summary>
        public const int SupportedPrecision = 8;

        // Complex value type field + mutable + available to other classes = the field MUST NOT be private :P
#pragma warning disable SA1401 // FieldsMustBePrivate

        /// <summary>
        /// Encapsulates stream reading and processing data and operations for <see cref="GolangJpegDecoderCore"/>.
        /// It's a value type for improved data locality, and reduced number of CALLVIRT-s
        /// </summary>
        public InputProcessor InputProcessor;
#pragma warning restore SA401

        /// <summary>
        /// The global configuration
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// Whether the image has a JFIF header
        /// It's faster to check this than to use the equality operator on the struct
        /// </summary>
        private bool isJFif;

        /// <summary>
        /// Contains information about the JFIF marker
        /// </summary>
        private JFifMarker jFif;

        /// <summary>
        /// Whether the image has a EXIF header
        /// </summary>
        private bool isExif;

        /// <summary>
        /// Whether the image has an Adobe marker.
        /// It's faster to check this than to use the equality operator on the struct
        /// </summary>
        private bool isAdobe;

        /// <summary>
        /// Contains information about the Adobe marker
        /// </summary>
        private AdobeMarker adobe;

        /// <summary>
        /// Initializes a new instance of the <see cref="GolangJpegDecoderCore" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        public GolangJpegDecoderCore(Configuration configuration, IJpegDecoderOptions options)
        {
            this.IgnoreMetadata = options.IgnoreMetadata;
            this.configuration = configuration ?? Configuration.Default;
            this.Temp = new byte[2 * Block8x8F.Size];
        }

        /// <inheritdoc />
        public JpegColorSpace ColorSpace { get; private set; }

        /// <summary>
        /// Gets the component array
        /// </summary>
        public GolangComponent[] Components { get; private set; }

        /// <summary>
        /// Gets the huffman trees
        /// </summary>
        public GolangHuffmanTree[] HuffmanTrees { get; private set; }

        /// <inheritdoc />
        public Block8x8F[] QuantizationTables { get; private set; }

        /// <summary>
        /// Gets the temporary buffer used to store bytes read from the stream.
        /// TODO: Should be stack allocated, fixed sized buffer!
        /// </summary>
        public byte[] Temp { get; }

        /// <inheritdoc />
        public Size ImageSizeInPixels { get; private set; }

        /// <summary>
        /// Gets the number of MCU blocks in the image as <see cref="Size"/>.
        /// </summary>
        public Size ImageSizeInMCU { get; private set; }

        /// <inheritdoc />
        public int ComponentCount { get; private set; }

        IEnumerable<IJpegComponent> IRawJpegData.Components => this.Components;

        /// <summary>
        /// Gets the color depth, in number of bits per pixel.
        /// </summary>
        public int BitsPerPixel => this.ComponentCount * SupportedPrecision;

        /// <summary>
        /// Gets the image height
        /// </summary>
        public int ImageHeight => this.ImageSizeInPixels.Height;

        /// <summary>
        /// Gets the image width
        /// </summary>
        public int ImageWidth => this.ImageSizeInPixels.Width;

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        public Stream InputStream { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the image is interlaced (progressive)
        /// </summary>
        public bool IsProgressive { get; private set; }

        /// <summary>
        /// Gets the restart interval
        /// </summary>
        public int RestartInterval { get; private set; }

        /// <summary>
        /// Gets the number of MCU-s (Minimum Coded Units) in the image along the X axis
        /// </summary>
        public int MCUCountX => this.ImageSizeInMCU.Width;

        /// <summary>
        /// Gets the number of MCU-s (Minimum Coded Units) in the image along the Y axis
        /// </summary>
        public int MCUCountY => this.ImageSizeInMCU.Height;

        /// <summary>
        /// Gets the total number of MCU-s (Minimum Coded Units) in the image.
        /// </summary>
        public int TotalMCUCount => this.MCUCountX * this.MCUCountY;

        /// <summary>
        /// Gets a value indicating whether the metadata should be ignored when the image is being decoded.
        /// </summary>
        public bool IgnoreMetadata { get; }

        /// <summary>
        /// Gets the <see cref="ImageMetaData"/> decoded by this decoder instance.
        /// </summary>
        public ImageMetaData MetaData { get; private set; }

        /// <summary>
        /// Decodes the image from the specified <see cref="Stream"/> and sets
        /// the data to image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="stream">The stream, where the image should be.</param>
        /// <returns>The decoded image.</returns>
        public Image<TPixel> Decode<TPixel>(Stream stream)
            where TPixel : struct, IPixel<TPixel>
        {
            this.ParseStream(stream);
            return this.PostProcessIntoImage<TPixel>();
        }

        /// <summary>
        /// Reads the raw image information from the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        public IImageInfo Identify(Stream stream)
        {
            this.ParseStream(stream, true);
            return new ImageInfo(new PixelTypeInfo(this.BitsPerPixel), this.ImageWidth, this.ImageHeight, this.MetaData);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.Components != null)
            {
                foreach (GolangComponent component in this.Components)
                {
                    component?.Dispose();
                }
            }

            this.InputProcessor.Dispose();
        }

        /// <summary>
        /// Read metadata from stream and read the blocks in the scans into <see cref="GolangComponent.SpectralBlocks"/>.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="metadataOnly">Whether to decode metadata only.</param>
        public void ParseStream(Stream stream, bool metadataOnly = false)
        {
            this.MetaData = new ImageMetaData();
            this.InputStream = stream;
            this.InputProcessor = new InputProcessor(stream, this.Temp);

            if (!metadataOnly)
            {
                this.HuffmanTrees = GolangHuffmanTree.CreateHuffmanTrees();
                this.QuantizationTables = new Block8x8F[MaxTq + 1];
            }

            // Check for the Start Of Image marker.
            this.InputProcessor.ReadFull(this.Temp, 0, 2);

            if (this.Temp[0] != JpegConstants.Markers.XFF || this.Temp[1] != JpegConstants.Markers.SOI)
            {
                throw new ImageFormatException("Missing SOI marker.");
            }

            // Process the remaining segments until the End Of Image marker.
            bool processBytes = true;

            // we can't currently short circute progressive images so don't try.
            while (processBytes)
            {
                this.InputProcessor.ReadFull(this.Temp, 0, 2);

                if (this.InputProcessor.ReachedEOF)
                {
                    // We've reached the end of the stream.
                    processBytes = false;
                }

                while (this.Temp[0] != 0xff)
                {
                    // Strictly speaking, this is a format error. However, libjpeg is
                    // liberal in what it accepts. As of version 9, next_marker in
                    // jdmarker.c treats this as a warning (JWRN_EXTRANEOUS_DATA) and
                    // continues to decode the stream. Even before next_marker sees
                    // extraneous data, jpeg_fill_bit_buffer in jdhuff.c reads as many
                    // bytes as it can, possibly past the end of a scan's data. It
                    // effectively puts back any markers that it overscanned (e.g. an
                    // "\xff\xd9" EOI marker), but it does not put back non-marker data,
                    // and thus it can silently ignore a small number of extraneous
                    // non-marker bytes before next_marker has a chance to see them (and
                    // print a warning).
                    // We are therefore also liberal in what we accept. Extraneous data
                    // is silently ignore
                    // This is similar to, but not exactly the same as, the restart
                    // mechanism within a scan (the RST[0-7] markers).
                    // Note that extraneous 0xff bytes in e.g. SOS data are escaped as
                    // "\xff\x00", and so are detected a little further down below.
                    this.Temp[0] = this.Temp[1];
                    this.Temp[1] = this.InputProcessor.ReadByte();
                }

                byte marker = this.Temp[1];
                if (marker == 0)
                {
                    // Treat "\xff\x00" as extraneous data.
                    continue;
                }

                while (marker == 0xff)
                {
                    // Section B.1.1.2 says, "Any marker may optionally be preceded by any
                    // number of fill bytes, which are bytes assigned code X'FF'".
                    this.InputProcessor.ReadByteUnsafe(out marker);

                    if (this.InputProcessor.ReachedEOF)
                    {
                        // We've reached the end of the stream.
                        processBytes = false;
                        break;
                    }
                }

                // End Of Image.
                if (marker == JpegConstants.Markers.EOI)
                {
                    break;
                }

                if (marker >= JpegConstants.Markers.RST0 && marker <= JpegConstants.Markers.RST7)
                {
                    // Figures B.2 and B.16 of the specification suggest that restart markers should
                    // only occur between Entropy Coded Segments and not after the final ECS.
                    // However, some encoders may generate incorrect JPEGs with a final restart
                    // marker. That restart marker will be seen here instead of inside the ProcessSOS
                    // method, and is ignored as a harmless error. Restart markers have no extra data,
                    // so we check for this before we read the 16-bit length of the segment.
                    continue;
                }

                // Read the 16-bit length of the segment. The value includes the 2 bytes for the
                // length itself, so we subtract 2 to get the number of remaining bytes.
                this.InputProcessor.ReadFullUnsafe(this.Temp, 0, 2);
                int remaining = (this.Temp[0] << 8) + this.Temp[1] - 2;
                if (remaining < 0)
                {
                    throw new ImageFormatException("Short segment length.");
                }

                switch (marker)
                {
                    case JpegConstants.Markers.SOF0:
                    case JpegConstants.Markers.SOF1:
                    case JpegConstants.Markers.SOF2:
                        this.IsProgressive = marker == JpegConstants.Markers.SOF2;
                        this.ProcessStartOfFrameMarker(remaining, metadataOnly);

                        break;
                    case JpegConstants.Markers.DHT:
                        if (metadataOnly)
                        {
                            this.InputProcessor.Skip(remaining);
                        }
                        else
                        {
                            this.ProcessDefineHuffmanTablesMarker(remaining);
                        }

                        break;
                    case JpegConstants.Markers.DQT:
                        if (metadataOnly)
                        {
                            this.InputProcessor.Skip(remaining);
                        }
                        else
                        {
                            this.ProcessDefineQuantizationTablesMarker(remaining);
                        }

                        break;
                    case JpegConstants.Markers.SOS:
                        if (!metadataOnly)
                        {
                            this.ProcessStartOfScanMarker(remaining);
                            if (this.InputProcessor.ReachedEOF)
                            {
                                // If unexpected EOF reached. We can stop processing bytes as we now have the image data.
                                processBytes = false;
                            }
                        }
                        else
                        {
                            // It's highly unlikely that APPn related data will be found after the SOS marker
                            // We should have gathered everything we need by now.
                            processBytes = false;
                        }

                        break;

                    case JpegConstants.Markers.DRI:
                        if (metadataOnly)
                        {
                            this.InputProcessor.Skip(remaining);
                        }
                        else
                        {
                            this.ProcessDefineRestartIntervalMarker(remaining);
                        }

                        break;
                    case JpegConstants.Markers.APP0:
                        this.ProcessApplicationHeaderMarker(remaining);
                        break;
                    case JpegConstants.Markers.APP1:
                        this.ProcessApp1Marker(remaining);
                        break;
                    case JpegConstants.Markers.APP2:
                        this.ProcessApp2Marker(remaining);
                        break;
                    case JpegConstants.Markers.APP14:
                        this.ProcessApp14Marker(remaining);
                        break;
                    default:
                        if ((marker >= JpegConstants.Markers.APP0 && marker <= JpegConstants.Markers.APP15)
                            || marker == JpegConstants.Markers.COM)
                        {
                            this.InputProcessor.Skip(remaining);
                        }

                        break;
                }
            }

            this.InitDerivedMetaDataProperties();
        }

        /// <summary>
        /// Returns true if 'mcuCounter' is at restart interval
        /// </summary>
        public bool IsAtRestartInterval(int mcuCounter)
        {
            return this.RestartInterval > 0 && mcuCounter % this.RestartInterval == 0
                                            && mcuCounter < this.TotalMCUCount;
        }

        /// <summary>
        /// Assigns derived metadata properties to <see cref="MetaData"/>, eg. horizontal and vertical resolution if it has a JFIF header.
        /// </summary>
        private void InitDerivedMetaDataProperties()
        {
            if (this.isJFif)
            {
                this.MetaData.HorizontalResolution = this.jFif.XDensity;
                this.MetaData.VerticalResolution = this.jFif.YDensity;
            }
            else if (this.isExif)
            {
                double horizontalValue = this.MetaData.ExifProfile.TryGetValue(ExifTag.XResolution, out ExifValue horizonalTag)
                    ? ((Rational)horizonalTag.Value).ToDouble()
                    : 0;

                double verticalValue = this.MetaData.ExifProfile.TryGetValue(ExifTag.YResolution, out ExifValue verticalTag)
                    ? ((Rational)verticalTag.Value).ToDouble()
                    : 0;

                if (horizontalValue > 0 && verticalValue > 0)
                {
                    this.MetaData.HorizontalResolution = horizontalValue;
                    this.MetaData.VerticalResolution = verticalValue;
                }
            }
        }

        /// <summary>
        /// Processes the application header containing the JFIF identifier plus extra data.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApplicationHeaderMarker(int remaining)
        {
            if (remaining < 5)
            {
                this.InputProcessor.Skip(remaining);
                return;
            }

            const int MarkerLength = JFifMarker.Length;
            this.InputProcessor.ReadFull(this.Temp, 0, MarkerLength);
            remaining -= MarkerLength;

            this.isJFif = JFifMarker.TryParse(this.Temp, out this.jFif);

            if (remaining > 0)
            {
                this.InputProcessor.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the App1 marker retrieving any stored metadata
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp1Marker(int remaining)
        {
            if (remaining < 6 || this.IgnoreMetadata)
            {
                this.InputProcessor.Skip(remaining);
                return;
            }

            byte[] profile = new byte[remaining];
            this.InputProcessor.ReadFull(profile, 0, remaining);

            if (ProfileResolver.IsProfile(profile, ProfileResolver.ExifMarker))
            {
                this.isExif = true;
                this.MetaData.ExifProfile = new ExifProfile(profile);
            }
        }

        /// <summary>
        /// Processes the App2 marker retrieving any stored ICC profile information
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp2Marker(int remaining)
        {
            // Length is 14 though we only need to check 12.
            const int Icclength = 14;
            if (remaining < Icclength || this.IgnoreMetadata)
            {
                this.InputProcessor.Skip(remaining);
                return;
            }

            byte[] identifier = new byte[Icclength];
            this.InputProcessor.ReadFull(identifier, 0, Icclength);
            remaining -= Icclength; // We have read it by this point

            if (ProfileResolver.IsProfile(identifier, ProfileResolver.IccMarker))
            {
                byte[] profile = new byte[remaining];
                this.InputProcessor.ReadFull(profile, 0, remaining);

                if (this.MetaData.IccProfile == null)
                {
                    this.MetaData.IccProfile = new IccProfile(profile);
                }
                else
                {
                    this.MetaData.IccProfile.Extend(profile);
                }
            }
            else
            {
                // Not an ICC profile we can handle. Skip the remaining bytes so we can carry on and ignore this.
                this.InputProcessor.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the application header containing the Adobe identifier
        /// which stores image encoding information for DCT filters.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessApp14Marker(int remaining)
        {
            const int MarkerLength = AdobeMarker.Length;
            if (remaining < MarkerLength)
            {
                // Skip the application header length
                this.InputProcessor.Skip(remaining);
                return;
            }

            this.InputProcessor.ReadFull(this.Temp, 0, MarkerLength);
            remaining -= MarkerLength;

            this.isAdobe = AdobeMarker.TryParse(this.Temp, out this.adobe);

            if (remaining > 0)
            {
                this.InputProcessor.Skip(remaining);
            }
        }

        /// <summary>
        /// Processes the Define Quantization Marker and tables. Specified in section B.2.4.1.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        /// <exception cref="ImageFormatException">
        /// Thrown if the tables do not match the header
        /// </exception>
        private void ProcessDefineQuantizationTablesMarker(int remaining)
        {
            while (remaining > 0)
            {
                bool done = false;

                remaining--;
                byte x = this.InputProcessor.ReadByte();
                int tq = x & 0x0F;
                if (tq > MaxTq)
                {
                    throw new ImageFormatException("Bad Tq value");
                }

                switch (x >> 4)
                {
                    case 0:
                        if (remaining < Block8x8F.Size)
                        {
                            done = true;
                            break;
                        }

                        remaining -= Block8x8F.Size;
                        this.InputProcessor.ReadFull(this.Temp, 0, Block8x8F.Size);

                        for (int i = 0; i < Block8x8F.Size; i++)
                        {
                            this.QuantizationTables[tq][i] = this.Temp[i];
                        }

                        break;
                    case 1:
                        if (remaining < 2 * Block8x8F.Size)
                        {
                            done = true;
                            break;
                        }

                        remaining -= 2 * Block8x8F.Size;
                        this.InputProcessor.ReadFull(this.Temp, 0, 2 * Block8x8F.Size);

                        for (int i = 0; i < Block8x8F.Size; i++)
                        {
                            this.QuantizationTables[tq][i] = (this.Temp[2 * i] << 8) | this.Temp[(2 * i) + 1];
                        }

                        break;
                    default:
                        throw new ImageFormatException("Bad Pq value");
                }

                if (done)
                {
                    break;
                }
            }

            if (remaining != 0)
            {
                throw new ImageFormatException("DQT has wrong length");
            }
        }

        /// <summary>
        /// Processes the Start of Frame marker.  Specified in section B.2.2.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        /// <param name="metadataOnly">Whether to decode metadata only.</param>
        private void ProcessStartOfFrameMarker(int remaining, bool metadataOnly)
        {
            if (this.ComponentCount != 0)
            {
                throw new ImageFormatException("Multiple SOF markers");
            }

            switch (remaining)
            {
                case 6 + (3 * 1): // grayscale image.
                    this.ComponentCount = 1;
                    break;
                case 6 + (3 * 3): // YCbCr or RGB image.
                    this.ComponentCount = 3;
                    break;
                case 6 + (3 * 4): // YCbCrK or CMYK image.
                    this.ComponentCount = 4;
                    break;
                default:
                    throw new ImageFormatException("Incorrect number of components");
            }

            this.InputProcessor.ReadFull(this.Temp, 0, remaining);

            // We only support 8-bit precision.
            if (this.Temp[0] != SupportedPrecision)
            {
                throw new ImageFormatException("Only 8-Bit precision supported.");
            }

            int height = (this.Temp[1] << 8) + this.Temp[2];
            int width = (this.Temp[3] << 8) + this.Temp[4];

            this.ImageSizeInPixels = new Size(width, height);

            if (this.Temp[5] != this.ComponentCount)
            {
                throw new ImageFormatException("SOF has wrong length");
            }

            if (!metadataOnly)
            {
                this.Components = new GolangComponent[this.ComponentCount];

                for (int i = 0; i < this.ComponentCount; i++)
                {
                    byte componentIdentifier = this.Temp[6 + (3 * i)];
                    var component = new GolangComponent(componentIdentifier, i);
                    component.InitializeCoreData(this);
                    this.Components[i] = component;
                }

                int h0 = this.Components[0].HorizontalSamplingFactor;
                int v0 = this.Components[0].VerticalSamplingFactor;

                this.ImageSizeInMCU = this.ImageSizeInPixels.DivideRoundUp(8 * h0, 8 * v0);

                this.ColorSpace = this.DeduceJpegColorSpace();

                foreach (GolangComponent component in this.Components)
                {
                    component.InitializeDerivedData(this.configuration.MemoryManager, this);
                }
            }
        }

        /// <summary>
        /// Processes a Define Huffman Table marker, and initializes a huffman
        /// struct from its contents. Specified in section B.2.4.2.
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessDefineHuffmanTablesMarker(int remaining)
        {
            while (remaining > 0)
            {
                if (remaining < 17)
                {
                    throw new ImageFormatException("DHT has wrong length");
                }

                this.InputProcessor.ReadFull(this.Temp, 0, 17);

                int tc = this.Temp[0] >> 4;
                if (tc > GolangHuffmanTree.MaxTc)
                {
                    throw new ImageFormatException("Bad Tc value");
                }

                int th = this.Temp[0] & 0x0f;
                if (th > GolangHuffmanTree.MaxTh)
                {
                    throw new ImageFormatException("Bad Th value");
                }

                int huffTreeIndex = (tc * GolangHuffmanTree.ThRowSize) + th;
                this.HuffmanTrees[huffTreeIndex].ProcessDefineHuffmanTablesMarkerLoop(
                    ref this.InputProcessor,
                    this.Temp,
                    ref remaining);
            }
        }

        /// <summary>
        /// Processes the DRI (Define Restart Interval Marker) Which specifies the interval between RSTn markers, in
        /// macroblocks
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        private void ProcessDefineRestartIntervalMarker(int remaining)
        {
            if (remaining != 2)
            {
                throw new ImageFormatException("DRI has wrong length");
            }

            this.InputProcessor.ReadFull(this.Temp, 0, remaining);
            this.RestartInterval = (this.Temp[0] << 8) + this.Temp[1];
        }

        /// <summary>
        /// Processes the SOS (Start of scan marker).
        /// </summary>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        /// <exception cref="ImageFormatException">
        /// Missing SOF Marker
        /// SOS has wrong length
        /// </exception>
        private void ProcessStartOfScanMarker(int remaining)
        {
            var scan = default(GolangJpegScanDecoder);
            GolangJpegScanDecoder.InitStreamReading(&scan, this, remaining);
            this.InputProcessor.Bits = default;
            scan.DecodeBlocks(this);
        }

        private JpegColorSpace DeduceJpegColorSpace()
        {
            switch (this.ComponentCount)
            {
                case 1:
                    return JpegColorSpace.Grayscale;
                case 3:
                    if (!this.isAdobe || this.adobe.ColorTransform == JpegConstants.Adobe.ColorTransformYCbCr)
                    {
                        return JpegColorSpace.YCbCr;
                    }

                    if (this.adobe.ColorTransform == JpegConstants.Adobe.ColorTransformUnknown)
                    {
                        return JpegColorSpace.RGB;
                    }

                    break;
                case 4:
                    if (this.adobe.ColorTransform == JpegConstants.Adobe.ColorTransformYcck)
                    {
                        return JpegColorSpace.Ycck;
                    }

                    return JpegColorSpace.Cmyk;
            }

            throw new ImageFormatException($"Unsupported color mode. Max components 4; found {this.ComponentCount}."
                                           + "JpegDecoder only supports YCbCr, RGB, YccK, CMYK and grayscale color spaces.");
        }

        private Image<TPixel> PostProcessIntoImage<TPixel>()
            where TPixel : struct, IPixel<TPixel>
        {
            using (var postProcessor = new JpegImagePostProcessor(this.configuration.MemoryManager, this))
            {
                var image = new Image<TPixel>(this.configuration, this.ImageWidth, this.ImageHeight, this.MetaData);
                postProcessor.PostProcess(image.Frames.RootFrame);
                return image;
            }
        }
    }
}