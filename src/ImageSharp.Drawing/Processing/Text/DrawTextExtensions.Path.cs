﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.Shapes;

namespace SixLabors.ImageSharp.Processing.Text
{
    /// <summary>
    /// Adds extensions that allow the drawing of text along given paths to the <see cref="Image{TPixel}"/> type.
    /// </summary>
    public static partial class DrawTextExtensions
    {
        /// <summary>
        /// Draws the text onto the the image filled via the brush.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="color">The color.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, string text, Font font, TPixel color, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(TextGraphicsOptions.Default, text, font, color, path);

        /// <summary>
        /// Draws the text onto the the image filled via the brush.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="options">The options.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="color">The color.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, TextGraphicsOptions options, string text, Font font, TPixel color, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(options, text, font, Brushes.Solid(color), null, path);

        /// <summary>
        /// Draws the text onto the the image filled via the brush.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="path">The location.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, string text, Font font, IBrush<TPixel> brush, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(TextGraphicsOptions.Default, text, font, brush, path);

        /// <summary>
        /// Draws the text onto the the image filled via the brush.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="options">The options.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, TextGraphicsOptions options, string text, Font font, IBrush<TPixel> brush, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(options, text, font, brush, null, path);

        /// <summary>
        /// Draws the text onto the the image outlined via the pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, string text, Font font, IPen<TPixel> pen, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(TextGraphicsOptions.Default, text, font, pen, path);

        /// <summary>
        /// Draws the text onto the the image outlined via the pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="options">The options.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, TextGraphicsOptions options, string text, Font font, IPen<TPixel> pen, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(options, text, font, null, pen, path);

        /// <summary>
        /// Draws the text onto the the image filled via the brush then outlined via the pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, string text, Font font, IBrush<TPixel> brush, IPen<TPixel> pen, IPath path)
            where TPixel : struct, IPixel<TPixel>
            => source.DrawText(TextGraphicsOptions.Default, text, font, brush, pen, path);

        /// <summary>
        /// Draws the text onto the the image filled via the brush then outlined via the pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="options">The options.</param>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The <see cref="Image{TPixel}" />.
        /// </returns>
        public static IImageProcessingContext<TPixel> DrawText<TPixel>(this IImageProcessingContext<TPixel> source, TextGraphicsOptions options, string text, Font font, IBrush<TPixel> brush, IPen<TPixel> pen, IPath path)
           where TPixel : struct, IPixel<TPixel>
        {
            float dpiX = DefaultTextDpi;
            float dpiY = DefaultTextDpi;

            var style = new RendererOptions(font, dpiX, dpiY)
            {
                ApplyKerning = options.ApplyKerning,
                TabWidth = options.TabWidth,
                WrappingWidth = options.WrapTextWidth,
                HorizontalAlignment = options.HorizontalAlignment,
                VerticalAlignment = options.VerticalAlignment
            };

            IPathCollection glyphs = TextBuilder.GenerateGlyphs(text, path, style);

            var pathOptions = (GraphicsOptions)options;
            if (brush != null)
            {
                source.Fill(pathOptions, brush, glyphs);
            }

            if (pen != null)
            {
                source.Draw(pathOptions, pen, glyphs);
            }

            return source;
        }
    }
}