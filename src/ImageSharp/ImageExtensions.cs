﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// Extension methods over Image{TPixel}
    /// </summary>
    public static partial class ImageExtensions
    {
#if !NETSTANDARD1_1
        /// <summary>
        /// Writes the image to the given stream using the currently loaded image format.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="filePath">The file path to save the image to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the stream is null.</exception>
        public static void Save<TPixel>(this Image<TPixel> source, string filePath)
            where TPixel : struct, IPixel<TPixel>
        {
            Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));

            string ext = Path.GetExtension(filePath);
            IImageFormat format = source.GetConfiguration().ImageFormatsManager.FindFormatByFileExtension(ext);
            if (format == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Can't find a format that is associated with the file extention '{ext}'. Registered formats with there extensions include:");
                foreach (IImageFormat fmt in source.GetConfiguration().ImageFormats)
                {
                    sb.AppendLine($" - {fmt.Name} : {string.Join(", ", fmt.FileExtensions)}");
                }

                throw new NotSupportedException(sb.ToString());
            }

            IImageEncoder encoder = source.GetConfiguration().ImageFormatsManager.FindEncoder(format);

            if (encoder == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Can't find encoder for file extention '{ext}' using image format '{format.Name}'. Registered encoders include:");
                foreach (KeyValuePair<IImageFormat, IImageEncoder> enc in source.GetConfiguration().ImageFormatsManager.ImageEncoders)
                {
                    sb.AppendLine($" - {enc.Key} : {enc.Value.GetType().Name}");
                }

                throw new NotSupportedException(sb.ToString());
            }

            source.Save(filePath, encoder);
        }

        /// <summary>
        /// Writes the image to the given stream using the currently loaded image format.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="filePath">The file path to save the image to.</param>
        /// <param name="encoder">The encoder to save the image with.</param>
        /// <exception cref="ArgumentNullException">Thrown if the encoder is null.</exception>
        public static void Save<TPixel>(this Image<TPixel> source, string filePath, IImageEncoder encoder)
            where TPixel : struct, IPixel<TPixel>
        {
            Guard.NotNull(encoder, nameof(encoder));
            using (Stream fs = source.GetConfiguration().FileSystem.Create(filePath))
            {
                source.Save(fs, encoder);
            }
        }
#endif

        /// <summary>
        /// Writes the image to the given stream using the currently loaded image format.
        /// </summary>
        /// <typeparam name="TPixel">The Pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="stream">The stream to save the image to.</param>
        /// <param name="format">The format to save the image in.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void Save<TPixel>(this Image<TPixel> source, Stream stream, IImageFormat format)
            where TPixel : struct, IPixel<TPixel>
        {
            Guard.NotNull(format, nameof(format));
            IImageEncoder encoder = source.GetConfiguration().ImageFormatsManager.FindEncoder(format);

            if (encoder == null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Can't find encoder for provided mime type. Available encoded:");

                foreach (KeyValuePair<IImageFormat, IImageEncoder> val in source.GetConfiguration().ImageFormatsManager.ImageEncoders)
                {
                    sb.AppendLine($" - {val.Key.Name} : {val.Value.GetType().Name}");
                }

                throw new NotSupportedException(sb.ToString());
            }

            source.Save(stream, encoder);
        }

        /// <summary>
        /// Returns the a copy of the image pixels as a byte array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <returns>A copy of the pixel data as bytes from this frame.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static byte[] SavePixelData<TPixel>(this ImageFrame<TPixel> source)
            where TPixel : struct, IPixel<TPixel>
         => MemoryMarshal.AsBytes(source.GetPixelSpan()).ToArray();

        /// <summary>
        /// Writes the raw image pixels to the given byte array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="buffer">The buffer to save the raw pixel data to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this ImageFrame<TPixel> source, byte[] buffer)
            where TPixel : struct, IPixel<TPixel>
            => SavePixelData(source, MemoryMarshal.Cast<byte, TPixel>(buffer.AsSpan()));

        /// <summary>
        /// Writes the raw image pixels to the given TPixel array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="buffer">The buffer to save the raw pixel data to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this ImageFrame<TPixel> source, TPixel[] buffer)
            where TPixel : struct, IPixel<TPixel>
            => SavePixelData(source, buffer.AsSpan());

        /// <summary>
        /// Returns a copy of the raw image pixels as a byte array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <returns>A copy of the pixel data from the first frame as bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static byte[] SavePixelData<TPixel>(this Image<TPixel> source)
            where TPixel : struct, IPixel<TPixel>
         => source.Frames.RootFrame.SavePixelData();

        /// <summary>
        /// Writes the raw image pixels to the given byte array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image.</param>
        /// <param name="buffer">The buffer to save the raw pixel data to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this Image<TPixel> source, byte[] buffer)
            where TPixel : struct, IPixel<TPixel>
            => source.Frames.RootFrame.SavePixelData(buffer);

        /// <summary>
        /// Writes the raw image pixels to the given TPixel array in row-major order.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="buffer">The buffer to save the raw pixel data to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this Image<TPixel> source, TPixel[] buffer)
            where TPixel : struct, IPixel<TPixel>
            => source.Frames.RootFrame.SavePixelData(buffer);

        /// <summary>
        /// Returns a Base64 encoded string from the given image.
        /// </summary>
        /// <example><see href="data:image/gif;base64,R0lGODlhAQABAIABAEdJRgAAACwAAAAAAQABAAACAkQBAA=="/></example>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="format">The format.</param>
        /// <returns>The <see cref="string"/></returns>
        public static string ToBase64String<TPixel>(this Image<TPixel> source, IImageFormat format)
            where TPixel : struct, IPixel<TPixel>
        {
            using (var stream = new MemoryStream())
            {
                source.Save(stream, format);
                stream.Flush();
                return $"data:{format.DefaultMimeType};base64,{Convert.ToBase64String(stream.ToArray())}";
            }
        }

        /// <summary>
        /// Writes the raw image bytes to the given byte span.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="buffer">The span to save the raw pixel data to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this Image<TPixel> source, Span<byte> buffer)
            where TPixel : struct, IPixel<TPixel>
            => source.Frames.RootFrame.SavePixelData(MemoryMarshal.Cast<byte, TPixel>(buffer));

        /// <summary>
        /// Writes the raw image pixels to the given TPixel span.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The source image</param>
        /// <param name="buffer">The span to save the raw pixel data to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the stream is null.</exception>
        public static void SavePixelData<TPixel>(this ImageFrame<TPixel> source, Span<TPixel> buffer)
            where TPixel : struct, IPixel<TPixel>
        {
            Span<TPixel> sourceBuffer = source.GetPixelSpan();
            Guard.MustBeGreaterThanOrEqualTo(buffer.Length, sourceBuffer.Length, nameof(buffer));

            sourceBuffer.CopyTo(buffer);
        }
    }
}