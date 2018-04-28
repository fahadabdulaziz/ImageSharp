﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Filters.Processors;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Processing.Filters
{
    public class SepiaTest : BaseImageOperationsExtensionTest
    {
        [Fact]
        public void Sepia_amount_SepiaProcessorDefaultsSet()
        {
            this.operations.Sepia();
            var processor = this.Verify<SepiaProcessor<Rgba32>>();
        }

        [Fact]
        public void Sepia_amount_rect_SepiaProcessorDefaultsSet()
        {
            this.operations.Sepia(this.rect);
            var processor = this.Verify<SepiaProcessor<Rgba32>>(this.rect);
        }
    }
}