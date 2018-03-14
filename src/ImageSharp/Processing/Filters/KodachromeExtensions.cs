﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Filters.Processors;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Filters
{
    /// <summary>
    /// Adds extensions that allow the recreation of an old Kodachrome camera effect to the <see cref="Image{TPixel}"/> type.
    /// </summary>
    public static class KodachromeExtensions
    {
        /// <summary>
        /// Alters the colors of the image recreating an old Kodachrome camera effect.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Kodachrome<TPixel>(this IImageProcessingContext<TPixel> source)
            where TPixel : struct, IPixel<TPixel>
            => source.ApplyProcessor(new KodachromeProcessor<TPixel>());

        /// <summary>
        /// Alters the colors of the image recreating an old Kodachrome camera effect.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="source">The image this method extends.</param>
        /// <param name="rectangle">
        /// The <see cref="Rectangle"/> structure that specifies the portion of the image object to alter.
        /// </param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        public static IImageProcessingContext<TPixel> Kodachrome<TPixel>(this IImageProcessingContext<TPixel> source, Rectangle rectangle)
            where TPixel : struct, IPixel<TPixel>
            => source.ApplyProcessor(new KodachromeProcessor<TPixel>(), rectangle);
    }
}
