﻿using System;
using System.Numerics;
using System.Reflection;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;
using SixLabors.Primitives;

using Xunit;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Tests.Processing.Transforms
{
    public class AffineTransformTests
    {
        private readonly ITestOutputHelper Output;

        // 1 byte difference on one color component.
        private static readonly ImageComparer ValidatorComparer = ImageComparer.TolerantPercentage(0.0134F, 3);

        /// <summary>
        /// angleDeg, sx, sy, tx, ty
        /// </summary>
        public static readonly TheoryData<float, float, float, float, float> TransformValues
            = new TheoryData<float, float, float, float, float>
                  {
                      { 0, 1, 1, 0, 0 },
                      { 50, 1, 1, 0, 0 },
                      { 0, 1, 1, 20, 10 },
                      { 50, 1, 1, 20, 10 },
                      { 0, 1, 1, -20, -10 },
                      { 50, 1, 1, -20, -10 },
                      { 50, 1.5f, 1.5f, 0, 0 },
                      { 50, 1.1F, 1.3F, 30, -20 },
                      { 0, 2f, 1f, 0, 0 },
                      { 0, 1f, 2f, 0, 0 },
                  };

        public static readonly TheoryData<string> ResamplerNames = new TheoryData<string>
        {
            nameof(KnownResamplers.Bicubic),
            nameof(KnownResamplers.Box),
            nameof(KnownResamplers.CatmullRom),
            nameof(KnownResamplers.Hermite),
            nameof(KnownResamplers.Lanczos2),
            nameof(KnownResamplers.Lanczos3),
            nameof(KnownResamplers.Lanczos5),
            nameof(KnownResamplers.Lanczos8),
            nameof(KnownResamplers.MitchellNetravali),
            nameof(KnownResamplers.NearestNeighbor),
            nameof(KnownResamplers.Robidoux),
            nameof(KnownResamplers.RobidouxSharp),
            nameof(KnownResamplers.Spline),
            nameof(KnownResamplers.Triangle),
            nameof(KnownResamplers.Welch),
        };

        public static readonly TheoryData<string> Transform_DoesNotCreateEdgeArtifacts_ResamplerNames =
            new TheoryData<string>
                {
                    nameof(KnownResamplers.NearestNeighbor),
                    nameof(KnownResamplers.Triangle),
                    nameof(KnownResamplers.Bicubic),
                    nameof(KnownResamplers.Lanczos8),
                };

        public AffineTransformTests(ITestOutputHelper output) => this.Output = output;

        /// <summary>
        /// The output of an "all white" image should be "all white" or transparent, regardless of the transformation and the resampler.
        /// </summary>
        [Theory]
        [WithSolidFilledImages(nameof(Transform_DoesNotCreateEdgeArtifacts_ResamplerNames), 5, 5, 255, 255, 255, 255, PixelTypes.Rgba32)]
        public void Transform_DoesNotCreateEdgeArtifacts<TPixel>(TestImageProvider<TPixel> provider, string resamplerName)
            where TPixel : struct, IPixel<TPixel>
        {
            IResampler resampler = GetResampler(resamplerName);
            using (Image<TPixel> image = provider.GetImage())
            {
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(30);

                image.Mutate(c => c.Transform(builder, resampler));
                image.DebugSave(provider, resamplerName);

                VerifyAllPixelsAreWhiteOrTransparent(image);
            }
        }

        [Theory]
        [WithTestPatternImages(nameof(TransformValues), 100, 50, PixelTypes.Rgba32)]
        public void Transform_RotateScaleTranslate<TPixel>(
            TestImageProvider<TPixel> provider,
            float angleDeg,
            float sx, float sy,
            float tx, float ty)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                image.DebugSave(provider, $"_original");
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(angleDeg)
                    .AppendScale(new SizeF(sx, sy))
                    .AppendTranslation(new PointF(tx, ty));

                this.PrintMatrix(builder.BuildMatrix(image.Size()));

                image.Mutate(i => i.Transform(builder, KnownResamplers.Bicubic));

                FormattableString testOutputDetails = $"R({angleDeg})_S({sx},{sy})_T({tx},{ty})";
                image.DebugSave(provider, testOutputDetails);
                image.CompareToReferenceOutput(ValidatorComparer, provider, testOutputDetails);
            }
        }

        [Theory]
        [WithTestPatternImages(96, 96, PixelTypes.Rgba32, 50, 0.8f)]
        public void Transform_RotateScale_ManuallyCentered<TPixel>(TestImageProvider<TPixel> provider, float angleDeg, float s)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(angleDeg)
                    .AppendScale(new SizeF(s, s));

                image.Mutate(i => i.Transform(builder, KnownResamplers.Bicubic));

                FormattableString testOutputDetails = $"R({angleDeg})_S({s})";
                image.DebugSave(provider, testOutputDetails);
                image.CompareToReferenceOutput(ValidatorComparer, provider, testOutputDetails);
            }
        }

        public static readonly TheoryData<int, int, int, int> Transform_IntoRectangle_Data =
            new TheoryData<int, int, int, int>
                {
                    { 0, 0, 10, 10 },
                    { 0, 0, 5, 10 },
                    { 0, 0, 10, 5 },
                    { 5, 0, 5, 10 },
                    {-5,-5, 20, 20 }
                };

        /// <summary>
        /// Testing transforms using custom source rectangles:
        /// https://github.com/SixLabors/ImageSharp/pull/386#issuecomment-357104963
        /// </summary>
        [Theory]
        [WithTestPatternImages(96, 48, PixelTypes.Rgba32)]
        public void Transform_FromSourceRectangle1<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            var rectangle = new Rectangle(48, 0, 48, 24);

            using (Image<TPixel> image = provider.GetImage())
            {
                image.DebugSave(provider, $"_original");
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendScale(new SizeF(2, 1.5F));

                image.Mutate(i => i.Transform(rectangle, builder, KnownResamplers.Spline));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(ValidatorComparer, provider);
            }
        }

        [Theory]
        [WithTestPatternImages(96, 48, PixelTypes.Rgba32)]
        public void Transform_FromSourceRectangle2<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            var rectangle = new Rectangle(0, 24, 48, 24);

            using (Image<TPixel> image = provider.GetImage())
            {
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendScale(new SizeF(1F, 2F));

                image.Mutate(i => i.Transform(rectangle, builder, KnownResamplers.Spline));

                image.DebugSave(provider);
                image.CompareToReferenceOutput(ValidatorComparer, provider);
            }
        }

        [Theory]
        [WithTestPatternImages(nameof(ResamplerNames), 150, 150, PixelTypes.Rgba32)]
        public void Transform_WithSampler<TPixel>(TestImageProvider<TPixel> provider, string resamplerName)
            where TPixel : struct, IPixel<TPixel>
        {
            IResampler sampler = GetResampler(resamplerName);
            using (Image<TPixel> image = provider.GetImage())
            {
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(50)
                    .AppendScale(new SizeF(.6F, .6F));

                image.Mutate(i => i.Transform(builder, sampler));

                image.DebugSave(provider, resamplerName);
                image.CompareToReferenceOutput(ValidatorComparer, provider, resamplerName);
            }
        }

        private static IResampler GetResampler(string name)
        {
            PropertyInfo property = typeof(KnownResamplers).GetTypeInfo().GetProperty(name);

            if (property is null)
            {
                throw new Exception($"No resampler named {name}");
            }

            return (IResampler)property.GetValue(null);
        }

        private static void VerifyAllPixelsAreWhiteOrTransparent<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            Span<TPixel> data = image.Frames.RootFrame.GetPixelSpan();
            var white = new Rgb24(255, 255, 255);
            foreach (TPixel pixel in data)
            {
                Rgba32 rgba = default;
                pixel.ToRgba32(ref rgba);
                if (rgba.A == 0)
                {
                    continue;
                }

                Assert.Equal(white, rgba.Rgb);
            }
        }

        private void PrintMatrix(Matrix3x2 a)
        {
            string s = $"{a.M11:F10},{a.M12:F10},{a.M21:F10},{a.M22:F10},{a.M31:F10},{a.M32:F10}";
            this.Output.WriteLine(s);
        }
    }
}