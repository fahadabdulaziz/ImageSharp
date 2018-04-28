﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation.HunterLabColorSapce;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion
{
    /// <content>
    /// Allows conversion to <see cref="HunterLab"/>.
    /// </content>
    internal partial class ColorSpaceConverter
    {
        /// <summary>
        /// Converts a <see cref="CieLab"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieLab color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieLch"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieLch color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieLchuv"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieLchuv color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieLuv"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieLuv color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieXyy"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieXyy color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieXyz"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(CieXyz color)
        {
            // Adaptation
            CieXyz adapted = !this.WhitePoint.Equals(this.TargetHunterLabWhitePoint) && this.IsChromaticAdaptationPerformed
                ? this.ChromaticAdaptation.Transform(color, this.WhitePoint, this.TargetHunterLabWhitePoint)
                : color;

            // Conversion
            return new CieXyzToHunterLabConverter(this.TargetHunterLabWhitePoint).Convert(adapted);
        }

        /// <summary>
        /// Converts a <see cref="Cmyk"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(Cmyk color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Hsl"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(Hsl color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Hsv"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(Hsv color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="LinearRgb"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(LinearRgb color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Lms"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(Lms color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Rgb"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(Rgb color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="YCbCr"/> into a <see cref="HunterLab"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="HunterLab"/></returns>
        public HunterLab ToHunterLab(YCbCr color)
        {
            var xyzColor = this.ToCieXyz(color);
            return this.ToHunterLab(xyzColor);
        }
    }
}