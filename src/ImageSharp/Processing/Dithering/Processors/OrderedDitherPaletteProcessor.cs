﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Dithering.Ordered;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Dithering.Processors
{
    /// <summary>
    /// An <see cref="IImageProcessor{TPixel}"/> that dithers an image using error diffusion.
    /// If no palette is given this will default to the web safe colors defined in the CSS Color Module Level 4.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class OrderedDitherPaletteProcessor<TPixel> : PaletteDitherProcessorBase<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDitherPaletteProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="dither">The ordered ditherer.</param>
        public OrderedDitherPaletteProcessor(IOrderedDither dither)
            : this(dither, NamedColors<TPixel>.WebSafePalette)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDitherPaletteProcessor{TPixel}"/> class.
        /// </summary>
        /// <param name="dither">The ordered ditherer.</param>
        /// <param name="palette">The palette to select substitute colors from.</param>
        public OrderedDitherPaletteProcessor(IOrderedDither dither, TPixel[] palette)
            : base(palette)
        {
            Guard.NotNull(dither, nameof(dither));
            this.Dither = dither;
        }

        /// <summary>
        /// Gets the ditherer.
        /// </summary>
        public IOrderedDither Dither { get; }

        /// <inheritdoc/>
        protected override void OnFrameApply(ImageFrame<TPixel> source, Rectangle sourceRectangle, Configuration configuration)
        {
            var rgba = default(Rgba32);
            bool isAlphaOnly = typeof(TPixel) == typeof(Alpha8);

            var interest = Rectangle.Intersect(sourceRectangle, source.Bounds());
            int startY = interest.Y;
            int endY = interest.Bottom;
            int startX = interest.X;
            int endX = interest.Right;

            // Collect the values before looping so we can reduce our calculation count for identical sibling pixels
            TPixel sourcePixel = source[startX, startY];
            TPixel previousPixel = sourcePixel;
            PixelPair<TPixel> pair = this.GetClosestPixelPair(ref sourcePixel, this.Palette);
            sourcePixel.ToRgba32(ref rgba);

            // Convert to grayscale using ITU-R Recommendation BT.709 if required
            float luminance = isAlphaOnly ? rgba.A : (.2126F * rgba.R) + (.7152F * rgba.G) + (.0722F * rgba.B);

            for (int y = startY; y < endY; y++)
            {
                Span<TPixel> row = source.GetPixelRowSpan(y);

                for (int x = startX; x < endX; x++)
                {
                    sourcePixel = row[x];

                    // Check if this is the same as the last pixel. If so use that value
                    // rather than calculating it again. This is an inexpensive optimization.
                    if (!previousPixel.Equals(sourcePixel))
                    {
                        pair = this.GetClosestPixelPair(ref sourcePixel, this.Palette);
                        sourcePixel.ToRgba32(ref rgba);
                        luminance = isAlphaOnly ? rgba.A : (.2126F * rgba.R) + (.7152F * rgba.G) + (.0722F * rgba.B);

                        // Setup the previous pointer
                        previousPixel = sourcePixel;
                    }

                    this.Dither.Dither(source, sourcePixel, pair.Second, pair.First, luminance, x, y);
                }
            }
        }
    }
}