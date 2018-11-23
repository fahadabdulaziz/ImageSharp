﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace SixLabors.ImageSharp.Tests.PixelFormats
{
    public class Gray16Tests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(65535)]
        [InlineData(32767)]
        [InlineData(42)]
        public void Gray16_PackedValue_EqualsInput(ushort input)
            => Assert.Equal(input, new Gray16(input).PackedValue);

        [Fact]
        public void Gray16_FromScaledVector4()
        {
            // Arrange
            Gray16 gray = default;
            const ushort expected = 32767;
            Vector4 scaled = new Gray16(expected).ToScaledVector4();

            // Act
            gray.FromScaledVector4(scaled);
            ushort actual = gray.PackedValue;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(65535)]
        [InlineData(32767)]
        public void Gray16_ToScaledVector4(ushort input)
        {
            // Arrange
            var gray = new Gray16(input);

            // Act
            Vector4 actual = gray.ToScaledVector4();

            // Assert
            float vectorInput = input / 65535F;
            Assert.Equal(vectorInput, actual.X);
            Assert.Equal(vectorInput, actual.Y);
            Assert.Equal(vectorInput, actual.Z);
            Assert.Equal(1F, actual.W);
        }

        [Fact]
        public void Gray16_FromVector4()
        {
            // Arrange
            Gray16 gray = default;
            const ushort expected = 32767;
            var vector = new Gray16(expected).ToVector4();

            // Act
            gray.FromVector4(vector);
            ushort actual = gray.PackedValue;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(65535)]
        [InlineData(32767)]
        public void Gray16_ToVector4(ushort input)
        {
            // Arrange
            var gray = new Gray16(input);

            // Act
            var actual = gray.ToVector4();

            // Assert
            float vectorInput = input / 65535F;
            Assert.Equal(vectorInput, actual.X);
            Assert.Equal(vectorInput, actual.Y);
            Assert.Equal(vectorInput, actual.Z);
            Assert.Equal(1F, actual.W);
        }

        [Fact]
        public void Gray16_FromRgba32()
        {
            // Arrange
            Gray16 gray = default;
            const byte rgb = 128;
            ushort scaledRgb = ImageMaths.UpscaleFrom8BitTo16Bit(rgb);
            ushort expected = ImageMaths.Get16BitBT709Luminance(scaledRgb, scaledRgb, scaledRgb);

            // Act
            gray.FromRgba32(new Rgba32(rgb, rgb, rgb));
            ushort actual = gray.PackedValue;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(65535)]
        [InlineData(8100)]
        public void Gray16_ToRgba32(ushort input)
        {
            // Arrange
            ushort expected = ImageMaths.DownScaleFrom16BitTo8Bit(input);
            var gray = new Gray16(input);

            // Act
            Rgba32 actual = default;
            gray.ToRgba32(ref actual);

            // Assert
            Assert.Equal(expected, actual.R);
            Assert.Equal(expected, actual.G);
            Assert.Equal(expected, actual.B);
            Assert.Equal(byte.MaxValue, actual.A);
        }
    }
}
