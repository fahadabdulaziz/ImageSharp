﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace SixLabors.ImageSharp.Processing.Drawing
{
    /// <summary>
    /// Adds extensions that allow the drawing of rectangles to the <see cref="Image{TPixel}"/> type.
    /// </summary>
    public static class DrawRectangleExtensions
    {
        /// <summary>
        /// Draws the outline of the rectangle with the provided pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, IPen<TPixel> pen, RectangleF shape, GraphicsOptions options)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(pen, new RectangularePolygon(shape.X, shape.Y, shape.Width, shape.Height), options);

        /// <summary>
        /// Draws the outline of the rectangle with the provided pen.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="shape">The shape.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, IPen<TPixel> pen, RectangleF shape)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(pen, shape, GraphicsOptions.Default);

        /// <summary>
        /// Draws the outline of the rectangle with the provided brush at the provided thickness.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, IBrush<TPixel> brush, float thickness, RectangleF shape, GraphicsOptions options)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(new Pen<TPixel>(brush, thickness), shape, options);

        /// <summary>
        /// Draws the outline of the rectangle with the provided brush at the provided thickness.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="shape">The shape.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, IBrush<TPixel> brush, float thickness, RectangleF shape)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(new Pen<TPixel>(brush, thickness), shape);

        /// <summary>
        /// Draws the outline of the rectangle with the provided brush at the provided thickness.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, TPixel color, float thickness, RectangleF shape, GraphicsOptions options)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(new SolidBrush<TPixel>(color), thickness, shape, options);

        /// <summary>
        /// Draws the outline of the rectangle with the provided brush at the provided thickness.
        /// </summary>
        /// <typeparam name="TPixel">The type of the color.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="shape">The shape.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Draw<TPixel>(this IImageProcessingContext<TPixel> source, TPixel color, float thickness, RectangleF shape)
            where TPixel : struct, IPixel<TPixel>
            => source.Draw(new SolidBrush<TPixel>(color), thickness, shape);
    }
}