﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Advanced
{
    public class AdvancedImageExtensionsTests
    {
        [Theory]
        [WithTestPatternImages(131, 127, PixelTypes.Rgba32 | PixelTypes.Bgr24)]
        public unsafe void DangerousGetPinnableReference_CopyToBuffer<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                var targetBuffer = new TPixel[image.Width * image.Height];

                ref byte source = ref Unsafe.As<TPixel, byte>(ref targetBuffer[0]);
                ref byte dest = ref Unsafe.As<TPixel, byte>(ref image.DangerousGetPinnableReferenceToPixelBuffer());
                
                fixed (byte* targetPtr = &source)
                fixed (byte* pixelBasePtr = &dest)
                {
                    uint dataSizeInBytes = (uint)(image.Width * image.Height * Unsafe.SizeOf<TPixel>());
                    Unsafe.CopyBlock(targetPtr, pixelBasePtr, dataSizeInBytes);
                }

                image.ComparePixelBufferTo(targetBuffer);
            }
        }
    }
}
