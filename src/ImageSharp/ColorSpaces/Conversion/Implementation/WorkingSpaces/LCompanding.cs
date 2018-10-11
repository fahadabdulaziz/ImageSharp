﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation
{
    /// <summary>
    /// Implements L* companding
    /// </summary>
    /// <remarks>
    /// For more info see:
    /// <see href="http://www.brucelindbloom.com/index.html?Eqn_RGB_to_XYZ.html"/>
    /// <see href="http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_RGB.html"/>
    /// </remarks>
    public static class LCompanding
    {
        /// <summary>
        /// Expands a companded channel to its linear equivalent with respect to the energy.
        /// </summary>
        /// <param name="channel">The channel value.</param>
        /// <returns>The <see cref="float"/> representing the linear channel value.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static float Expand(float channel)
            => channel <= 0.08 ? 100 * channel / CieConstants.Kappa : ImageMaths.Pow3((channel + 0.16F) / 1.16F);

        /// <summary>
        /// Compresses an uncompanded channel (linear) to its nonlinear equivalent.
        /// </summary>
        /// <param name="channel">The channel value</param>
        /// <returns>The <see cref="float"/> representing the nonlinear channel value.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static float Compress(float channel)
            => channel <= CieConstants.Epsilon ? channel * CieConstants.Kappa / 100F : MathF.Pow(1.16F * channel, 0.3333333F) - 0.16F;
    }
}