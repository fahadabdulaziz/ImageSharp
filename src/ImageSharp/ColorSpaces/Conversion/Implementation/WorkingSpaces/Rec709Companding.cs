﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation
{
    /// <summary>
    /// Implements the Rec. 709 companding function.
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Rec._709
    /// </remarks>
    public static class Rec709Companding
    {
        /// <summary>
        /// Expands a companded channel to its linear equivalent with respect to the energy.
        /// </summary>
        /// <param name="channel">The channel value.</param>
        /// <returns>The <see cref="float"/> representing the linear channel value.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static float Expand(float channel)
            => channel < 0.081F ? channel / 4.5F : MathF.Pow((channel + 0.099F) / 1.099F, 2.222222F);

        /// <summary>
        /// Compresses an uncompanded channel (linear) to its nonlinear equivalent.
        /// </summary>
        /// <param name="channel">The channel value.</param>
        /// <returns>The <see cref="float"/> representing the nonlinear channel value.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static float Compress(float channel)
            => channel < 0.018F ? 4500F * channel : (1.099F * channel) - 0.099F;
    }
}