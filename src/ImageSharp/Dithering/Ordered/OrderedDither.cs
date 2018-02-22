﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Dithering
{
    /// <summary>
    /// An ordered dithering matrix with equal sides of arbitrary length
    /// </summary>
    public class OrderedDither : IOrderedDither
    {
        private readonly Fast2DArray<uint> thresholdMatrix;
        private readonly int modulusX;
        private readonly int modulusY;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDither"/> class.
        /// </summary>
        /// <param name="length">The length of the matrix sides</param>
        public OrderedDither(uint length)
        {
            Fast2DArray<uint> ditherMatrix = OrderedDitherFactory.CreateDitherMatrix(length);
            this.modulusX = ditherMatrix.Width;
            this.modulusY = ditherMatrix.Height;

            // Adjust the matrix range for 0-255
            // It looks like it's actually possible to dither an image using it's own colors. We should investigate for V2
            // https://stackoverflow.com/questions/12422407/monochrome-dithering-in-javascript-bayer-atkinson-floyd-steinberg
            int multiplier = 256 / ditherMatrix.Count;
            for (int y = 0; y < ditherMatrix.Height; y++)
            {
                for (int x = 0; x < ditherMatrix.Width; x++)
                {
                    ditherMatrix[y, x] = (uint)((ditherMatrix[y, x] + 1) * multiplier) - 1;
                }
            }

            this.thresholdMatrix = ditherMatrix;
        }

        /// <inheritdoc />
        public void Dither<TPixel>(ImageFrame<TPixel> image, TPixel source, TPixel upper, TPixel lower, float threshold, int x, int y)
            where TPixel : struct, IPixel<TPixel>
        {
            image[x, y] = this.thresholdMatrix[y % this.modulusY, x % this.modulusX] >= threshold ? lower : upper;
        }
    }
}