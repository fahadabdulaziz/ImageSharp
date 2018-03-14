﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Processing.Convolution
{
    using SixLabors.ImageSharp.Processing.Convolution;
    using SixLabors.ImageSharp.Processing.Convolution.Processors;

    public class BoxBlurTest : BaseImageOperationsExtensionTest
    {
        [Fact]
        public void BoxBlur_BoxBlurProcessorDefaultsSet()
        {
            this.operations.BoxBlur();
            var processor = this.Verify<BoxBlurProcessor<Rgba32>>();

            Assert.Equal(7, processor.Radius);
        }

        [Fact]
        public void BoxBlur_amount_BoxBlurProcessorDefaultsSet()
        {
            this.operations.BoxBlur(34);
            var processor = this.Verify<BoxBlurProcessor<Rgba32>>();

            Assert.Equal(34, processor.Radius);
        }

        [Fact]
        public void BoxBlur_amount_rect_BoxBlurProcessorDefaultsSet()
        {
            this.operations.BoxBlur(5, this.rect);
            var processor = this.Verify<BoxBlurProcessor<Rgba32>>(this.rect);

            Assert.Equal(5, processor.Radius);
        }
    }
}