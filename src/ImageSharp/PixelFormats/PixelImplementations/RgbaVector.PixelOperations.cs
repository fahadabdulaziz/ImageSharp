﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <content>
    /// Provides optimized overrides for bulk operations.
    /// </content>
    public partial struct RgbaVector
    {
        /// <summary>
        /// <see cref="PixelOperations{TPixel}"/> implementation optimized for <see cref="RgbaVector"/>.
        /// </summary>
        internal class PixelOperations : PixelOperations<RgbaVector>
        {
            /// <inheritdoc />
            internal override void FromScaledVector4(
                ReadOnlySpan<Vector4> sourceVectors,
                Span<RgbaVector> destinationColors)
            {
                Guard.DestinationShouldNotBeTooShort(sourceVectors, destinationColors, nameof(destinationColors));

                MemoryMarshal.Cast<Vector4, RgbaVector>(sourceVectors).CopyTo(destinationColors);
            }

            /// <inheritdoc />
            internal override void ToScaledVector4(
                ReadOnlySpan<RgbaVector> sourceColors,
                Span<Vector4> destinationVectors)
                => this.ToVector4(sourceColors, destinationVectors);

            /// <inheritdoc />
            internal override void ToVector4(ReadOnlySpan<RgbaVector> sourceColors, Span<Vector4> destinationVectors)
            {
                Guard.DestinationShouldNotBeTooShort(sourceColors, destinationVectors, nameof(destinationVectors));

                MemoryMarshal.Cast<RgbaVector, Vector4>(sourceColors).CopyTo(destinationVectors);
            }
        }
    }
}