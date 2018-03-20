﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Text;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Quantization;

namespace SixLabors.ImageSharp.Formats.Gif
{
    /// <summary>
    /// Image encoder for writing image data to a stream in gif format.
    /// </summary>
    public sealed class GifEncoder : IImageEncoder, IGifEncoderOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the metadata should be ignored when the image is being encoded.
        /// </summary>
        public bool IgnoreMetadata { get; set; } = false;

        /// <summary>
        /// Gets or sets the encoding that should be used when writing comments.
        /// </summary>
        public Encoding TextEncoding { get; set; } = GifConstants.DefaultEncoding;

        /// <summary>
        /// Gets or sets the quantizer for reducing the color count.
        /// Defaults to the <see cref="OctreeQuantizer"/>
        /// </summary>
        public IQuantizer Quantizer { get; set; } = new OctreeQuantizer();

        /// <inheritdoc/>
        public void Encode<TPixel>(Image<TPixel> image, Stream stream)
            where TPixel : struct, IPixel<TPixel>
        {
            var encoder = new GifEncoderCore(image.GetConfiguration().MemoryManager, this);
            encoder.Encode(image, stream);
        }
    }
}