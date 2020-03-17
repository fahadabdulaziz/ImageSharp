// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

using ImageMagick;

using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;

namespace SixLabors.ImageSharp.Tests.Formats.Tga
{
    public static class TgaTestUtils
    {
        public static void CompareWithReferenceDecoder<TPixel>(
            TestImageProvider<TPixel> provider,
            Image<TPixel> image,
            bool useExactComparer = true,
            float compareTolerance = 0.01f)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            string path = TestImageProvider<TPixel>.GetFilePathOrNull(provider);
            if (path == null)
            {
                throw new InvalidOperationException("CompareToOriginal() works only with file providers!");
            }

            var testFile = TestFile.Create(path);
            Image<Rgba32> magickImage = DecodeWithMagick<Rgba32>(Configuration.Default, new FileInfo(testFile.FullPath));
            if (useExactComparer)
            {
                ImageComparer.Exact.VerifySimilarity(magickImage, image);
            }
            else
            {
                ImageComparer.Tolerant(compareTolerance).VerifySimilarity(magickImage, image);
            }
        }

        public static Image<TPixel> DecodeWithMagick<TPixel>(Configuration configuration, FileInfo fileInfo)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var magickImage = new MagickImage(fileInfo))
            {
                var result = new Image<TPixel>(configuration, magickImage.Width, magickImage.Height);
                Span<TPixel> resultPixels = result.GetPixelSpan();

                using (IPixelCollection pixels = magickImage.GetPixelsUnsafe())
                {
                    byte[] data = pixels.ToByteArray(PixelMapping.RGBA);

                    PixelOperations<TPixel>.Instance.FromRgba32Bytes(
                        configuration,
                        data,
                        resultPixels,
                        resultPixels.Length);
                }

                return result;
            }
        }
    }
}
