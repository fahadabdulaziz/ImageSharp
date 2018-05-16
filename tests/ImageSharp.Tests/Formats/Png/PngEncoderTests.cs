﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Linq;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Quantization;

using Xunit;
// ReSharper disable InconsistentNaming

namespace SixLabors.ImageSharp.Tests
{
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

    public class PngEncoderTests
    {
        private const float ToleranceThresholdForPaletteEncoder = 0.2f / 100;

        /// <summary>
        /// All types except Palette
        /// </summary>
        public static readonly TheoryData<PngColorType> PngColorTypes = new TheoryData<PngColorType>
        {
            PngColorType.RgbWithAlpha,
            PngColorType.Rgb,
            PngColorType.Grayscale,
            PngColorType.GrayscaleWithAlpha,
        };

        public static readonly TheoryData<PngFilterMethod> PngFilterMethods = new TheoryData<PngFilterMethod>
        {
            PngFilterMethod.None,
            PngFilterMethod.Sub,
            PngFilterMethod.Up,
            PngFilterMethod.Average,
            PngFilterMethod.Paeth,
            PngFilterMethod.Adaptive
        };

        /// <summary>
        /// All types except Palette
        /// </summary>
        public static readonly TheoryData<int> CompressionLevels = new TheoryData<int>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9
        };

        public static readonly TheoryData<int> PaletteSizes = new TheoryData<int>
        {
            30, 55, 100, 201, 255
        };

        public static readonly TheoryData<int> PaletteLargeOnly = new TheoryData<int>
        {
            80, 100, 120, 230
        };

        [Theory]
        [WithFile(TestImages.Png.Palette8Bpp, nameof(PngColorTypes), PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(PngColorTypes), 48, 24, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(PngColorTypes), 47, 8, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(PngColorTypes), 49, 7, PixelTypes.Rgba32)]
        [WithSolidFilledImages(nameof(PngColorTypes), 1, 1, 255, 100, 50, 255, PixelTypes.Rgba32)]
        [WithTestPatternImages(nameof(PngColorTypes), 7, 5, PixelTypes.Rgba32)]
        public void WorksWithDifferentSizes<TPixel>(TestImageProvider<TPixel> provider, PngColorType pngColorType)
            where TPixel : struct, IPixel<TPixel>
        {
            TestPngEncoderCore(provider, pngColorType, PngFilterMethod.Adaptive, appendPngColorType: true);
        }

        [Theory]
        [WithTestPatternImages(nameof(PngColorTypes), 24, 24, PixelTypes.Rgba32 | PixelTypes.Bgra32 | PixelTypes.Rgb24)]
        public void IsNotBoundToSinglePixelType<TPixel>(TestImageProvider<TPixel> provider, PngColorType pngColorType)
            where TPixel : struct, IPixel<TPixel>
        {
            TestPngEncoderCore(provider, pngColorType, PngFilterMethod.Adaptive, appendPixelType: true, appendPngColorType: true);
        }

        [Theory]
        [WithTestPatternImages(nameof(PngFilterMethods), 24, 24, PixelTypes.Rgba32)]
        public void WorksWithAllFilterMethods<TPixel>(TestImageProvider<TPixel> provider, PngFilterMethod pngFilterMethod)
            where TPixel : struct, IPixel<TPixel>
        {
            TestPngEncoderCore(provider, PngColorType.RgbWithAlpha, pngFilterMethod, appendPngFilterMethod: true);
        }

        [Theory]
        [WithTestPatternImages(nameof(CompressionLevels), 24, 24, PixelTypes.Rgba32)]
        public void WorksWithAllCompressionLevels<TPixel>(TestImageProvider<TPixel> provider, int compressionLevel)
            where TPixel : struct, IPixel<TPixel>
        {
            TestPngEncoderCore(provider, PngColorType.RgbWithAlpha, PngFilterMethod.Adaptive, compressionLevel, appendCompressionLevel: true);
        }

        [Theory]
        [WithFile(TestImages.Png.Palette8Bpp, nameof(PaletteLargeOnly), PixelTypes.Rgba32)]
        public void PaletteColorType_WuQuantizer<TPixel>(TestImageProvider<TPixel> provider, int paletteSize)
            where TPixel : struct, IPixel<TPixel>
        {
            TestPngEncoderCore(provider, PngColorType.Palette, PngFilterMethod.Adaptive, paletteSize: paletteSize, appendPaletteSize: true);
        }

        private static bool HasAlpha(PngColorType pngColorType) =>
            pngColorType == PngColorType.GrayscaleWithAlpha || pngColorType == PngColorType.RgbWithAlpha;

        private static void TestPngEncoderCore<TPixel>(
            TestImageProvider<TPixel> provider,
            PngColorType pngColorType,
            PngFilterMethod pngFilterMethod,
            int compressionLevel = 6,
            int paletteSize = 255,
            bool appendPngColorType = false,
            bool appendPngFilterMethod = false,
            bool appendPixelType = false,
            bool appendCompressionLevel = false,
            bool appendPaletteSize = false)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                if (!HasAlpha(pngColorType))
                {
                    image.Mutate(c => c.MakeOpaque());
                }

                var encoder = new PngEncoder
                {
                    PngColorType = pngColorType,
                    PngFilterMethod = pngFilterMethod,
                    CompressionLevel = compressionLevel,
                    Quantizer = new WuQuantizer(paletteSize)
                };

                string pngColorTypeInfo = appendPngColorType ? pngColorType.ToString() : string.Empty;
                string pngFilterMethodInfo = appendPngFilterMethod ? pngFilterMethod.ToString() : string.Empty;
                string compressionLevelInfo = appendCompressionLevel ? $"_C{compressionLevel}" : string.Empty;
                string paletteSizeInfo = appendPaletteSize ? $"_PaletteSize-{paletteSize}" : string.Empty;
                string debugInfo = $"{pngColorTypeInfo}{pngFilterMethodInfo}{compressionLevelInfo}{paletteSizeInfo}";
                //string referenceInfo = $"{pngColorTypeInfo}";

                // Does DebugSave & load reference CompareToReferenceInput():
                string actualOutputFile = ((ITestImageProvider)provider).Utility.SaveTestOutputFile(image, "png", encoder, debugInfo, appendPixelType);

                if (TestEnvironment.IsMono)
                {
                    // There are bugs in mono's System.Drawing implementation, reference decoders are not always reliable!
                    return;
                }

                IImageDecoder referenceDecoder = TestEnvironment.GetReferenceDecoder(actualOutputFile);
                string referenceOutputFile = ((ITestImageProvider)provider).Utility.GetReferenceOutputFileName("png", debugInfo, appendPixelType, true);

                using (var actualImage = Image.Load<TPixel>(actualOutputFile, referenceDecoder))
                using (var referenceImage = Image.Load<TPixel>(referenceOutputFile, referenceDecoder))
                {
                    float paletteToleranceHack = 80f / paletteSize;
                    paletteToleranceHack = paletteToleranceHack * paletteToleranceHack;
                    ImageComparer comparer = pngColorType == PngColorType.Palette
                                                 ? ImageComparer.Tolerant(ToleranceThresholdForPaletteEncoder * paletteToleranceHack)
                                                 : ImageComparer.Exact;

                    comparer.VerifySimilarity(referenceImage, actualImage);
                }
            }
        }

        [Theory]
        [WithBlankImages(1, 1, PixelTypes.Rgba32)]
        public void WritesFileMarker<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new PngEncoder());

                byte[] data = ms.ToArray().Take(8).ToArray();
                byte[] expected = {
                    0x89, // Set the high bit.
                    0x50, // P
                    0x4E, // N
                    0x47, // G
                    0x0D, // Line ending CRLF
                    0x0A, // Line ending CRLF
                    0x1A, // EOF
                    0x0A // LF
                };

                Assert.Equal(expected, data);
            }
        }
    }
}