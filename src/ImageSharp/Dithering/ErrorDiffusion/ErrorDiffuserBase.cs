﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Dithering.Base
{
    /// <summary>
    /// The base class for performing error diffusion based dithering.
    /// </summary>
    public abstract class ErrorDiffuserBase : IErrorDiffuser
    {
        /// <summary>
        /// The vector to perform division.
        /// </summary>
        private readonly Vector4 divisorVector;

        /// <summary>
        /// The matrix width
        /// </summary>
        private readonly int matrixHeight;

        /// <summary>
        /// The matrix height
        /// </summary>
        private readonly int matrixWidth;

        /// <summary>
        /// The offset at which to start the dithering operation.
        /// </summary>
        private readonly int startingOffset;

        /// <summary>
        /// The diffusion matrix
        /// </summary>
        private readonly Fast2DArray<float> matrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDiffuserBase"/> class.
        /// </summary>
        /// <param name="matrix">The dithering matrix.</param>
        /// <param name="divisor">The divisor.</param>
        internal ErrorDiffuserBase(Fast2DArray<float> matrix, byte divisor)
        {
            Guard.NotNull(matrix, nameof(matrix));
            Guard.MustBeGreaterThan(divisor, 0, nameof(divisor));

            this.matrix = matrix;
            this.matrixWidth = this.matrix.Width;
            this.matrixHeight = this.matrix.Height;
            this.divisorVector = new Vector4(divisor);

            this.startingOffset = 0;
            for (int i = 0; i < this.matrixWidth; i++)
            {
                // Good to disable here as we are not comparing mathematical output.
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (matrix[0, i] != 0)
                {
                    this.startingOffset = (byte)(i - 1);
                    break;
                }
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dither<TPixel>(ImageFrame<TPixel> image, TPixel source, TPixel transformed, int x, int y, int minX, int minY, int maxX, int maxY)
            where TPixel : struct, IPixel<TPixel>
        {
            image[x, y] = transformed;

            // Calculate the error
            Vector4 error = source.ToVector4() - transformed.ToVector4();

            // Loop through and distribute the error amongst neighboring pixels.
            for (int row = 0; row < this.matrixHeight; row++)
            {
                int matrixY = y + row;
                if (matrixY > minY && matrixY < maxY)
                {
                    Span<TPixel> rowSpan = image.GetPixelRowSpan(matrixY);

                    for (int col = 0; col < this.matrixWidth; col++)
                    {
                        int matrixX = x + (col - this.startingOffset);

                        if (matrixX > minX && matrixX < maxX)
                        {
                            float coefficient = this.matrix[row, col];

                            // Good to disable here as we are not comparing mathematical output.
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (coefficient == 0)
                            {
                                continue;
                            }

                            ref TPixel pixel = ref rowSpan[matrixX];
                            var offsetColor = pixel.ToVector4();

                            Vector4 result = ((error * coefficient) / this.divisorVector) + offsetColor;
                            pixel.PackFromVector4(result);
                        }
                    }
                }
            }
        }
    }
}