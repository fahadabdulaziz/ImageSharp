﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation.CieLchuvColorSapce;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion
{
    /// <content>
    /// Allows conversion to <see cref="CieLchuv"/>.
    /// </content>
    internal partial class ColorSpaceConverter
    {
        /// <summary>
        /// The converter for converting between CieLab to CieLchuv.
        /// </summary>
        private static readonly CieLuvToCieLchuvConverter CieLuvToCieLchuvConverter = new CieLuvToCieLchuvConverter();

        /// <summary>
        /// Converts a <see cref="CieLab"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(CieLab color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieLch"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(CieLch color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieLuv"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(CieLuv color)
        {
            // Adaptation
            CieLuv adapted = this.IsChromaticAdaptationPerformed ? this.Adapt(color) : color;

            // Conversion
            return CieLuvToCieLchuvConverter.Convert(adapted);
        }

        /// <summary>
        /// Converts a <see cref="CieXyy"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(CieXyy color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="CieXyz"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(CieXyz color)
        {
            CieLab labColor = this.ToCieLab(color);
            return this.ToCieLchuv(labColor);
        }

        /// <summary>
        /// Converts a <see cref="Cmyk"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(Cmyk color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Hsl"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(Hsl color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Hsv"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(Hsv color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="HunterLab"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(HunterLab color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="LinearRgb"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(LinearRgb color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Lms"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(Lms color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="Rgb"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(Rgb color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }

        /// <summary>
        /// Converts a <see cref="YCbCr"/> into a <see cref="CieLchuv"/>
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The <see cref="CieLchuv"/></returns>
        public CieLchuv ToCieLchuv(YCbCr color)
        {
            CieXyz xyzColor = this.ToCieXyz(color);
            return this.ToCieLchuv(xyzColor);
        }
    }
}