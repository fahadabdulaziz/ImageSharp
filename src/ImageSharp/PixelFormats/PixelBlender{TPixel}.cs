﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Memory;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <summary>
    /// Abstract base class for calling pixel composition functions
    /// </summary>
    /// <typeparam name="TPixel">The type of the pixel</typeparam>
    internal abstract class PixelBlender<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// Blend 2 pixels together.
        /// </summary>
        /// <param name="background">The background color.</param>
        /// <param name="source">The source color.</param>
        /// <param name="amount">
        /// A value between 0 and 1 indicating the weight of the second source vector.
        /// At amount = 0, "from" is returned, at amount = 1, "to" is returned.
        /// </param>
        /// <returns>The final pixel value after composition</returns>
        public abstract TPixel Blend(TPixel background, TPixel source, float amount);

        /// <summary>
        /// Blend 2 pixels together.
        /// </summary>
        /// <param name="memoryManager">The <see cref="MemoryManager"/></param>
        /// <param name="destination">The destination span.</param>
        /// <param name="background">The background span.</param>
        /// <param name="source">The source span.</param>
        /// <param name="amount">
        /// A value between 0 and 1 indicating the weight of the second source vector.
        /// At amount = 0, "from" is returned, at amount = 1, "to" is returned.
        /// </param>
        public abstract void Blend(MemoryManager memoryManager, Span<TPixel> destination, Span<TPixel> background, Span<TPixel> source, Span<float> amount);
    }
}
