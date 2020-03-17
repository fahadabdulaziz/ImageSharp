// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.RemoteExecutor;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Tests.Formats.Jpg.Utils;
using SixLabors.ImageSharp.Tests.TestUtilities;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Tests.Formats.Jpg
{
    // TODO: Scatter test cases into multiple test classes
    public partial class JpegDecoderTests
    {
        public const PixelTypes CommonNonDefaultPixelTypes = PixelTypes.Rgba32 | PixelTypes.Argb32 | PixelTypes.RgbaVector;

        private const float BaselineTolerance = 0.001F / 100;

        private const float ProgressiveTolerance = 0.2F / 100;

        private static ImageComparer GetImageComparer<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            string file = provider.SourceFileOrDescription;

            if (!CustomToleranceValues.TryGetValue(file, out float tolerance))
            {
                bool baseline = file.IndexOf("baseline", StringComparison.OrdinalIgnoreCase) >= 0;
                tolerance = baseline ? BaselineTolerance : ProgressiveTolerance;
            }

            return ImageComparer.Tolerant(tolerance);
        }

        private static bool SkipTest(ITestImageProvider provider)
        {
            string[] largeImagesToSkipOn32Bit =
                {
                    TestImages.Jpeg.Baseline.Jpeg420Exif,
                    TestImages.Jpeg.Issues.MissingFF00ProgressiveBedroom159,
                    TestImages.Jpeg.Issues.BadZigZagProgressive385,
                    TestImages.Jpeg.Issues.NoEoiProgressive517,
                    TestImages.Jpeg.Issues.BadRstProgressive518,
                    TestImages.Jpeg.Issues.InvalidEOI695,
                    TestImages.Jpeg.Issues.ExifResizeOutOfRange696,
                    TestImages.Jpeg.Issues.ExifGetString750Transform
                };

            return !TestEnvironment.Is64BitProcess && largeImagesToSkipOn32Bit.Contains(provider.SourceFileOrDescription);
        }

        public JpegDecoderTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        private ITestOutputHelper Output { get; }

        private static JpegDecoder JpegDecoder => new JpegDecoder();

        [Fact]
        public void ParseStream_BasicPropertiesAreCorrect()
        {
            byte[] bytes = TestFile.Create(TestImages.Jpeg.Progressive.Progress).Bytes;
            using (var ms = new MemoryStream(bytes))
            {
                var decoder = new JpegDecoderCore(Configuration.Default, new JpegDecoder());
                decoder.ParseStream(ms);

                // I don't know why these numbers are different. All I know is that the decoder works
                // and spectral data is exactly correct also.
                // VerifyJpeg.VerifyComponentSizes3(decoder.Frame.Components, 43, 61, 22, 31, 22, 31);
                VerifyJpeg.VerifyComponentSizes3(decoder.Frame.Components, 44, 62, 22, 31, 22, 31);
            }
        }

        public const string DecodeBaselineJpegOutputName = "DecodeBaselineJpeg";

        [Theory]
        [WithFile(TestImages.Jpeg.Baseline.Calliphora, CommonNonDefaultPixelTypes)]
        public void JpegDecoder_IsNotBoundToSinglePixelType<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using Image<TPixel> image = provider.GetImage(JpegDecoder);
            image.DebugSave(provider);

            provider.Utility.TestName = DecodeBaselineJpegOutputName;
            image.CompareToReferenceOutput(
                ImageComparer.Tolerant(BaselineTolerance),
                provider,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithFile(TestImages.Jpeg.Baseline.Floorplan, PixelTypes.Rgba32)]
        [WithFile(TestImages.Jpeg.Progressive.Festzug, PixelTypes.Rgba32)]
        public void DegenerateMemoryRequest_ShouldTranslateTo_ImageFormatException<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            provider.LimitAllocatorBufferCapacity().InBytesSqrt(10);
            ImageFormatException ex = Assert.Throws<ImageFormatException>(() => provider.GetImage(JpegDecoder));
            this.Output.WriteLine(ex.Message);
            Assert.IsType<InvalidMemoryOperationException>(ex.InnerException);
        }

        // DEBUG ONLY!
        // The PDF.js output should be saved by "tests\ImageSharp.Tests\Formats\Jpg\pdfjs\jpeg-converter.htm"
        // into "\tests\Images\ActualOutput\JpegDecoderTests\"
        // [Theory]
        // [WithFile(TestImages.Jpeg.Progressive.Progress, PixelTypes.Rgba32, "PdfJsOriginal_progress.png")]
        public void ValidateProgressivePdfJsOutput<TPixel>(
            TestImageProvider<TPixel> provider,
            string pdfJsOriginalResultImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            // tests\ImageSharp.Tests\Formats\Jpg\pdfjs\jpeg-converter.htm
            string pdfJsOriginalResultPath = Path.Combine(
                provider.Utility.GetTestOutputDir(),
                pdfJsOriginalResultImage);

            byte[] sourceBytes = TestFile.Create(TestImages.Jpeg.Progressive.Progress).Bytes;

            provider.Utility.TestName = nameof(DecodeProgressiveJpegOutputName);

            var comparer = ImageComparer.Tolerant(0, 0);

            using (Image<TPixel> expectedImage = provider.GetReferenceOutputImage<TPixel>(appendPixelTypeToFileName: false))
            using (var pdfJsOriginalResult = Image.Load<Rgba32>(pdfJsOriginalResultPath))
            using (var pdfJsPortResult = Image.Load<Rgba32>(sourceBytes, JpegDecoder))
            {
                ImageSimilarityReport originalReport = comparer.CompareImagesOrFrames(expectedImage, pdfJsOriginalResult);
                ImageSimilarityReport portReport = comparer.CompareImagesOrFrames(expectedImage, pdfJsPortResult);

                this.Output.WriteLine($"Difference for PDF.js ORIGINAL: {originalReport.DifferencePercentageString}");
                this.Output.WriteLine($"Difference for PORT: {portReport.DifferencePercentageString}");
            }
        }
    }
}
