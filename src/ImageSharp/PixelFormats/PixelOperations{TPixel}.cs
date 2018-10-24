﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <summary>
    /// A stateless class implementing Strategy Pattern for batched pixel-data conversion operations
    /// for pixel buffers of type <typeparamref name="TPixel"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    public partial class PixelOperations<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// Gets the global <see cref="PixelOperations{TPixel}"/> instance for the pixel type <typeparamref name="TPixel"/>
        /// </summary>
        public static PixelOperations<TPixel> Instance { get; } = default(TPixel).CreatePixelOperations();

        /// <summary>
        /// Bulk version of <see cref="IPixel.FromVector4"/> converting 'sourceVectors.Length' pixels into 'destinationColors'.
        /// </summary>
        /// <param name="sourceVectors">The <see cref="Span{T}"/> to the source vectors.</param>
        /// <param name="destinationColors">The <see cref="Span{T}"/> to the destination colors.</param>
        internal virtual void FromVector4(ReadOnlySpan<Vector4> sourceVectors, Span<TPixel> destinationColors)
        {
            Guard.DestinationShouldNotBeTooShort(sourceVectors, destinationColors, nameof(destinationColors));

            ref Vector4 sourceRef = ref MemoryMarshal.GetReference(sourceVectors);
            ref TPixel destRef = ref MemoryMarshal.GetReference(destinationColors);

            for (int i = 0; i < sourceVectors.Length; i++)
            {
                ref Vector4 sp = ref Unsafe.Add(ref sourceRef, i);
                ref TPixel dp = ref Unsafe.Add(ref destRef, i);
                dp.FromVector4(sp);
            }
        }

        /// <summary>
        /// Bulk version of <see cref="IPixel.ToVector4()"/> converting 'sourceColors.Length' pixels into 'destinationVectors'.
        /// </summary>
        /// <param name="sourceColors">The <see cref="Span{T}"/> to the source colors.</param>
        /// <param name="destinationVectors">The <see cref="Span{T}"/> to the destination vectors.</param>
        internal virtual void ToVector4(ReadOnlySpan<TPixel> sourceColors, Span<Vector4> destinationVectors)
        {
            Guard.DestinationShouldNotBeTooShort(sourceColors, destinationVectors, nameof(destinationVectors));

            ref TPixel sourceRef = ref MemoryMarshal.GetReference(sourceColors);
            ref Vector4 destRef = ref MemoryMarshal.GetReference(destinationVectors);

            for (int i = 0; i < sourceColors.Length; i++)
            {
                ref TPixel sp = ref Unsafe.Add(ref sourceRef, i);
                ref Vector4 dp = ref Unsafe.Add(ref destRef, i);
                dp = sp.ToVector4();
            }
        }

        /// <summary>
        /// Bulk version of <see cref="IPixel.FromScaledVector4"/> converting 'sourceVectors.Length' pixels into 'destinationColors'.
        /// </summary>
        /// <param name="sourceVectors">The <see cref="Span{T}"/> to the source vectors.</param>
        /// <param name="destinationColors">The <see cref="Span{T}"/> to the destination colors.</param>
        internal virtual void FromScaledVector4(ReadOnlySpan<Vector4> sourceVectors, Span<TPixel> destinationColors)
        {
            Guard.DestinationShouldNotBeTooShort(sourceVectors, destinationColors, nameof(destinationColors));

            ref Vector4 sourceRef = ref MemoryMarshal.GetReference(sourceVectors);
            ref TPixel destRef = ref MemoryMarshal.GetReference(destinationColors);

            for (int i = 0; i < sourceVectors.Length; i++)
            {
                ref Vector4 sp = ref Unsafe.Add(ref sourceRef, i);
                ref TPixel dp = ref Unsafe.Add(ref destRef, i);
                dp.FromScaledVector4(sp);
            }
        }

        /// <summary>
        /// Bulk version of <see cref="IPixel.ToScaledVector4()"/> converting 'sourceColors.Length' pixels into 'destinationVectors'.
        /// </summary>
        /// <param name="sourceColors">The <see cref="Span{T}"/> to the source colors.</param>
        /// <param name="destinationVectors">The <see cref="Span{T}"/> to the destination vectors.</param>
        internal virtual void ToScaledVector4(ReadOnlySpan<TPixel> sourceColors, Span<Vector4> destinationVectors)
        {
            Guard.DestinationShouldNotBeTooShort(sourceColors, destinationVectors, nameof(destinationVectors));

            ref TPixel sourceRef = ref MemoryMarshal.GetReference(sourceColors);
            ref Vector4 destRef = ref MemoryMarshal.GetReference(destinationVectors);

            for (int i = 0; i < sourceColors.Length; i++)
            {
                ref TPixel sp = ref Unsafe.Add(ref sourceRef, i);
                ref Vector4 dp = ref Unsafe.Add(ref destRef, i);
                dp = sp.ToScaledVector4();
            }
        }

        /// <summary>
        /// Converts 'sourceColors.Length' pixels from 'sourceColors' into 'destinationColors'.
        /// </summary>
        /// <typeparam name="TDestinationPixel">The destination pixel type.</typeparam>
        /// <param name="sourceColors">The <see cref="Span{T}"/> to the source colors.</param>
        /// <param name="destinationColors">The <see cref="Span{T}"/> to the destination colors.</param>
        internal virtual void To<TDestinationPixel>(
            ReadOnlySpan<TPixel> sourceColors,
            Span<TDestinationPixel> destinationColors)
            where TDestinationPixel : struct, IPixel<TDestinationPixel>
        {
            Guard.DestinationShouldNotBeTooShort(sourceColors, destinationColors, nameof(destinationColors));

            int count = sourceColors.Length;
            ref TPixel sourceRef = ref MemoryMarshal.GetReference(sourceColors);

            // Gray8 and Gray16 are special implementations of IPixel in that they do not conform to the
            // standard RGBA colorspace format and must be converted from RGBA using the special ITU BT709 alogrithm.
            // One of the requirements of FromScaledVector4/ToScaledVector4 is that it unaware of this and
            // packs/unpacks the pixel without and conversion so we employ custom methods do do this.
            if (typeof(TDestinationPixel) == typeof(Gray16))
            {
                ref Gray16 gray16Ref = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<TDestinationPixel, Gray16>(destinationColors));
                for (int i = 0; i < count; i++)
                {
                    ref TPixel sp = ref Unsafe.Add(ref sourceRef, i);
                    ref Gray16 dp = ref Unsafe.Add(ref gray16Ref, i);
                    dp.ConvertFromRgbaScaledVector4(sp.ToScaledVector4());
                }

                return;
            }

            if (typeof(TDestinationPixel) == typeof(Gray8))
            {
                ref Gray8 gray8Ref = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<TDestinationPixel, Gray8>(destinationColors));
                for (int i = 0; i < count; i++)
                {
                    ref TPixel sp = ref Unsafe.Add(ref sourceRef, i);
                    ref Gray8 dp = ref Unsafe.Add(ref gray8Ref, i);
                    dp.ConvertFromRgbaScaledVector4(sp.ToScaledVector4());
                }

                return;
            }

            // Normal conversion
            ref TDestinationPixel destRef = ref MemoryMarshal.GetReference(destinationColors);
            for (int i = 0; i < count; i++)
            {
                ref TPixel sp = ref Unsafe.Add(ref sourceRef, i);
                ref TDestinationPixel dp = ref Unsafe.Add(ref destRef, i);
                dp.FromScaledVector4(sp.ToScaledVector4());
            }
        }

        /// <summary>
        /// Verifies that the given 'source' and 'destination' spans are at least of 'minLength' size.
        /// Throwing an <see cref="ArgumentException"/> if the condition is not met.
        /// </summary>
        /// <typeparam name="TSource">The source element type</typeparam>
        /// <typeparam name="TDest">The destination element type</typeparam>
        /// <param name="source">The source span</param>
        /// <param name="sourceParamName">The source parameter name</param>
        /// <param name="destination">The destination span</param>
        /// <param name="destinationParamName">The destination parameter name</param>
        /// <param name="minLength">The minimum length</param>
        protected internal static void GuardSpans<TSource, TDest>(
            ReadOnlySpan<TSource> source,
            string sourceParamName,
            Span<TDest> destination,
            string destinationParamName,
            int minLength)
        {
            Guard.MustBeSizedAtLeast(source, minLength, sourceParamName);
            Guard.MustBeSizedAtLeast(destination, minLength, destinationParamName);
        }
    }
}