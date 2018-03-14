﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;
using SixLabors.Primitives;
using Xunit;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Tests.Processing.Processors.Transforms
{
    using SixLabors.ImageSharp.Processing.Transforms;

    public class ResizeTests : FileTestBase
    {
        public static readonly string[] CommonTestImages = { TestImages.Png.CalliphoraPartial };

        public static readonly TheoryData<string, IResampler> AllReSamplers =
            new TheoryData<string, IResampler>
            {
                { "Bicubic", ResampleMode.Bicubic },
                { "Triangle", ResampleMode.Triangle},
                { "NearestNeighbor", ResampleMode.NearestNeighbor },
                { "Box", ResampleMode.Box },
                // { "Lanczos2", KnownResamplers.Lanczos2 }, TODO: Add expected file
                { "Lanczos3", ResampleMode.Lanczos3 },
                { "Lanczos5", ResampleMode.Lanczos5 },
                { "MitchellNetravali", ResampleMode.MitchellNetravali  },
                { "Lanczos8", ResampleMode.Lanczos8  },
                { "Hermite", ResampleMode.Hermite  },
                { "Spline", ResampleMode.Spline  },
                { "Robidoux", ResampleMode.Robidoux  },
                { "RobidouxSharp", ResampleMode.RobidouxSharp },
                { "Welch", ResampleMode.Welch  }
            };

        [Theory]
        [WithTestPatternImages(nameof(AllReSamplers), 100, 100, DefaultPixelType, 0.5f)]
        [WithFileCollection(nameof(CommonTestImages), nameof(AllReSamplers), DefaultPixelType, 0.5f)]
        [WithFileCollection(nameof(CommonTestImages), nameof(AllReSamplers), DefaultPixelType, 0.3f)]
        public void Resize_WorksWithAllResamplers<TPixel>(TestImageProvider<TPixel> provider, string name, IResampler sampler, float ratio)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                SizeF newSize = image.Size() * ratio;
                image.Mutate(x => x.Resize((Size)newSize, sampler, false));
                string details = $"{name}-{ratio.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

                image.DebugSave(provider, details);
                image.CompareToReferenceOutput(provider, details);
            }
        }

        [Theory]
        [WithTestPatternImages(100, 100, DefaultPixelType)]
        public void Resize_Compand<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(image.Size() / 2, true));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithTestPatternImages(50, 50, CommonNonDefaultPixelTypes)]
        public void Resize_IsNotBoundToSinglePixelType<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2, true));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFile(TestImages.Png.Kaboom, DefaultPixelType)]
        public void Resize_DoesNotBleedAlphaPixels<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFile(TestImages.Gif.Giphy, DefaultPixelType)]
        public void Resize_IsAppliedToAllFrames<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2, ResampleMode.NearestNeighbor));

                // Comparer fights decoder with gif-s. Could not use CompareToReferenceOutput here :(
                image.DebugSave(provider, extension: Extensions.Gif);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeFromSourceRectangle<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var sourceRectangle = new Rectangle(image.Width / 8, image.Height / 8, image.Width / 4, image.Height / 4);
                var destRectangle = new Rectangle(image.Width / 4, image.Height / 4, image.Width / 2, image.Height / 2);

                image.Mutate(x => x.Resize(image.Width, image.Height, ResampleMode.Bicubic, sourceRectangle, destRectangle, false));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWidthAndKeepAspect<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(image.Width / 3, 0, false));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeHeightAndKeepAspect<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.Mutate(x => x.Resize(0, image.Height / 3, false));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithCropWidthMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(image.Width / 2, image.Height)
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithCropHeightMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(image.Width, image.Height / 2)
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithPadMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(image.Width + 200, image.Height),
                    Mode = ResizeMode.Pad
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithBoxPadMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(image.Width + 200, image.Height + 200),
                    Mode = ResizeMode.BoxPad
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithMaxMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(300, 300),
                    Mode = ResizeMode.Max
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithMinMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size((int)Math.Round(image.Width * .75F), (int)Math.Round(image.Height * .95F)),
                    Mode = ResizeMode.Min
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [WithFileCollection(nameof(CommonTestImages), DefaultPixelType)]
        public void ResizeWithStretchMode<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var options = new ResizeOptions
                {
                    Size = new Size(image.Width / 2, image.Height),
                    Mode = ResizeMode.Stretch
                };

                image.Mutate(x => x.Resize(options));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(provider);
            }
        }

        [Theory]
        [InlineData(-2, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public static void BicubicWindowOscillatesCorrectly(float x, float expected)
        {
            var sampler = ResampleMode.Bicubic;
            float result = sampler.GetValue(x);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData(-2, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public static void TriangleWindowOscillatesCorrectly(float x, float expected)
        {
            var sampler = ResampleMode.Triangle;
            float result = sampler.GetValue(x);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData(-2, 0)]
        [InlineData(-1, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public static void Lanczos3WindowOscillatesCorrectly(float x, float expected)
        {
            var sampler = ResampleMode.Lanczos3;
            float result = sampler.GetValue(x);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData(-4, 0)]
        [InlineData(-2, 0)]
        [InlineData(0, 1)]
        [InlineData(2, 0)]
        [InlineData(4, 0)]
        public static void Lanczos5WindowOscillatesCorrectly(float x, float expected)
        {
            var sampler = ResampleMode.Lanczos5;
            float result = sampler.GetValue(x);

            Assert.Equal(result, expected);
        }
    }
}