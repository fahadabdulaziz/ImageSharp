﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

using Xunit;

namespace SixLabors.ImageSharp.Tests
{
    public class QuantizedImageTests
    {
        private Configuration Configuration => Configuration.Default;

        [Fact]
        public void QuantizersDitherByDefault()
        {
            var werner = new WernerPaletteQuantizer();
            var websafe = new WebSafePaletteQuantizer();
            var octree = new OctreeQuantizer();
            var wu = new WuQuantizer();

            Assert.NotNull(werner.Diffuser);
            Assert.NotNull(websafe.Diffuser);
            Assert.NotNull(octree.Diffuser);
            Assert.NotNull(wu.Diffuser);

            Assert.True(werner.CreateFrameQuantizer<Rgba32>(this.Configuration).Dither);
            Assert.True(websafe.CreateFrameQuantizer<Rgba32>(this.Configuration).Dither);
            Assert.True(octree.CreateFrameQuantizer<Rgba32>(this.Configuration).Dither);
            Assert.True(wu.CreateFrameQuantizer<Rgba32>(this.Configuration).Dither);
        }

        [Theory]
        [WithFile(TestImages.Gif.Giphy, PixelTypes.Rgba32, true)]
        [WithFile(TestImages.Gif.Giphy, PixelTypes.Rgba32, false)]
        public void OctreeQuantizerYieldsCorrectTransparentPixel<TPixel>(
            TestImageProvider<TPixel> provider,
            bool dither)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                Assert.True(image[0, 0].Equals(default(TPixel)));

                var quantizer = new OctreeQuantizer(dither);

                foreach (ImageFrame<TPixel> frame in image.Frames)
                {
                    IQuantizedFrame<TPixel> quantized =
                        quantizer.CreateFrameQuantizer<TPixel>(this.Configuration).QuantizeFrame(frame);

                    int index = this.GetTransparentIndex(quantized);
                    Assert.Equal(index, quantized.GetPixelSpan()[0]);
                }
            }
        }

        [Theory]
        [WithFile(TestImages.Gif.Giphy, PixelTypes.Rgba32, true)]
        [WithFile(TestImages.Gif.Giphy, PixelTypes.Rgba32, false)]
        public void WuQuantizerYieldsCorrectTransparentPixel<TPixel>(TestImageProvider<TPixel> provider, bool dither)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                Assert.True(image[0, 0].Equals(default(TPixel)));

                var quantizer = new WuQuantizer(dither);

                foreach (ImageFrame<TPixel> frame in image.Frames)
                {
                    IQuantizedFrame<TPixel> quantized =
                        quantizer.CreateFrameQuantizer<TPixel>(this.Configuration).QuantizeFrame(frame);

                    int index = this.GetTransparentIndex(quantized);
                    Assert.Equal(index, quantized.GetPixelSpan()[0]);
                }
            }
        }

        private int GetTransparentIndex<TPixel>(IQuantizedFrame<TPixel> quantized)
            where TPixel : struct, IPixel<TPixel>
        {
            // Transparent pixels are much more likely to be found at the end of a palette
            int index = -1;
            Rgba32 trans = default;
            ReadOnlySpan<TPixel> paletteSpan = quantized.Palette.Span;
            for (int i = paletteSpan.Length - 1; i >= 0; i--)
            {
                paletteSpan[i].ToRgba32(ref trans);

                if (trans.Equals(default))
                {
                    index = i;
                }
            }

            return index;
        }
    }
}