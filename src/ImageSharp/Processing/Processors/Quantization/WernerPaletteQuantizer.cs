﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace SixLabors.ImageSharp.Processing.Processors.Quantization
{
    /// <summary>
    /// A palette quantizer consisting of colors as defined in the original second edition of Werner’s Nomenclature of Colours 1821.
    /// The hex codes were collected and defined by Nicholas Rougeux <see href="https://www.c82.net/werner"/>
    /// </summary>
    public class WernerPaletteQuantizer : PaletteQuantizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WernerPaletteQuantizer" /> class.
        /// </summary>
        public WernerPaletteQuantizer()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WernerPaletteQuantizer" /> class.
        /// </summary>
        /// <param name="dither">Whether to apply dithering to the output image</param>
        public WernerPaletteQuantizer(bool dither)
            : base(Color.WernerPalette, dither)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WernerPaletteQuantizer" /> class.
        /// </summary>
        /// <param name="diffuser">The error diffusion algorithm, if any, to apply to the output image</param>
        public WernerPaletteQuantizer(IErrorDiffuser diffuser)
            : base(Color.WernerPalette, diffuser)
        {
        }
    }
}