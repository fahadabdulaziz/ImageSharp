﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Tests.PixelFormats
{
    public class PixelOperationsTests
    {
        public const string SkipProfilingBenchmarks =
#if true
            "Profiling benchmark - enable manually!";
#else
                null;
#endif

        public class Argb32OperationsTests : PixelOperationsTests<Argb32>
        {
            
            public Argb32OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Argb32.PixelOperations>(PixelOperations<Argb32>.Instance);
        }

        public class Bgr24OperationsTests : PixelOperationsTests<Bgr24>
        {
            public Bgr24OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Bgr24.PixelOperations>(PixelOperations<Bgr24>.Instance);
        }

        public class Bgra32OperationsTests : PixelOperationsTests<Bgra32>
        {
            public Bgra32OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Bgra32.PixelOperations>(PixelOperations<Bgra32>.Instance);
        }

        public class Gray8OperationsTests : PixelOperationsTests<Gray8>
        {
            public Gray8OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Gray8.PixelOperations>(PixelOperations<Gray8>.Instance);

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void FromGray8Bytes(int count)
            {
                byte[] source = CreateByteTestData(count);
                var expected = new Gray8[count];

                for (int i = 0; i < count; i++)
                {
                    expected[i].FromGray8(new Gray8(source[i]));
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.FromGray8Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void ToGray8Bytes(int count)
            {
                Gray8[] source = CreatePixelTestData(count);
                byte[] expected = new byte[count];
                var gray = default(Gray8);

                for (int i = 0; i < count; i++)
                {
                    gray.FromScaledVector4(source[i].ToScaledVector4());
                    expected[i] = gray.PackedValue;
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.ToGray8Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void FromGray16Bytes(int count)
            {
                byte[] source = CreateByteTestData(count * 2);
                Span<byte> sourceSpan = source.AsSpan();
                var expected = new Gray8[count];

                for (int i = 0; i < count; i++)
                {
                    int i2 = i * 2;
                    expected[i].FromGray16(MemoryMarshal.Cast<byte, Gray16>(sourceSpan.Slice(i2, 2))[0]);
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.FromGray16Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void ToGray16Bytes(int count)
            {
                Gray8[] source = CreatePixelTestData(count);
                byte[] expected = new byte[count * 2];
                Gray16 gray = default;

                for (int i = 0; i < count; i++)
                {
                    int i2 = i * 2;
                    gray.FromScaledVector4(source[i].ToScaledVector4());
                    OctetBytes bytes = Unsafe.As<Gray16, OctetBytes>(ref gray);
                    expected[i2] = bytes[0];
                    expected[i2 + 1] = bytes[1];
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.ToGray16Bytes(s, d.GetSpan(), count)
                );
            }
        }

        public class Gray16OperationsTests : PixelOperationsTests<Gray16>
        {
            public Gray16OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Gray16.PixelOperations>(PixelOperations<Gray16>.Instance);

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void FromGray8Bytes(int count)
            {
                byte[] source = CreateByteTestData(count);
                var expected = new Gray16[count];

                for (int i = 0; i < count; i++)
                {
                    expected[i].FromGray8(new Gray8(source[i]));
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.FromGray8Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void ToGray8Bytes(int count)
            {
                Gray16[] source = CreatePixelTestData(count);
                byte[] expected = new byte[count];
                var gray = default(Gray8);

                for (int i = 0; i < count; i++)
                {
                    gray.FromScaledVector4(source[i].ToScaledVector4());
                    expected[i] = gray.PackedValue;
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.ToGray8Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void FromGray16Bytes(int count)
            {
                byte[] source = CreateByteTestData(count * 2);
                Span<byte> sourceSpan = source.AsSpan();
                var expected = new Gray16[count];

                for (int i = 0; i < count; i++)
                {
                    int i2 = i * 2;
                    expected[i].FromGray16(MemoryMarshal.Cast<byte, Gray16>(sourceSpan.Slice(i2, 2))[0]);
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.FromGray16Bytes(s, d.GetSpan(), count)
                );
            }

            [Theory]
            [MemberData(nameof(ArraySizesData))]
            public void ToGray16Bytes(int count)
            {
                Gray16[] source = CreatePixelTestData(count);
                byte[] expected = new byte[count * 2];
                Gray16 gray = default;

                for (int i = 0; i < count; i++)
                {
                    int i2 = i * 2;
                    gray.FromScaledVector4(source[i].ToScaledVector4());
                    OctetBytes bytes = Unsafe.As<Gray16, OctetBytes>(ref gray);
                    expected[i2] = bytes[0];
                    expected[i2 + 1] = bytes[1];
                }

                TestOperation(
                    source,
                    expected,
                    (s, d) => Operations.ToGray16Bytes(s, d.GetSpan(), count)
                );
            }
        }

        public class Rgba32OperationsTests : PixelOperationsTests<Rgba32>
        {
            public Rgba32OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Rgba32.PixelOperations>(PixelOperations<Rgba32>.Instance);

            [Fact(Skip = SkipProfilingBenchmarks)]
            public void Benchmark_ToVector4()
            {
                const int times = 200000;
                const int count = 1024;

                using (IMemoryOwner<Rgba32> source = Configuration.Default.MemoryAllocator.Allocate<Rgba32>(count))
                using (IMemoryOwner<Vector4> dest = Configuration.Default.MemoryAllocator.Allocate<Vector4>(count))
                {
                    this.Measure(
                        times,
                        () => PixelOperations<Rgba32>.Instance.ToVector4(source.GetSpan(), dest.GetSpan()));
                }
            }
        }

        public class Rgb48OperationsTests : PixelOperationsTests<Rgb48>
        {
            public Rgb48OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Rgb48.PixelOperations>(PixelOperations<Rgb48>.Instance);
        }

        public class Rgba64OperationsTests : PixelOperationsTests<Rgba64>
        {
            public Rgba64OperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<Rgba64.PixelOperations>(PixelOperations<Rgba64>.Instance);
        }

        public class RgbaVectorOperationsTests : PixelOperationsTests<RgbaVector>
        {
            public RgbaVectorOperationsTests(ITestOutputHelper output)
                : base(output)
            {
            }

            [Fact]
            public void IsSpecialImplementation() => Assert.IsType<RgbaVector.PixelOperations>(PixelOperations<RgbaVector>.Instance);
        }

        [Theory]
        [WithBlankImages(1, 1, PixelTypes.All)]
        public void GetGlobalInstance<TPixel>(TestImageProvider<TPixel> _)
            where TPixel : struct, IPixel<TPixel> => Assert.NotNull(PixelOperations<TPixel>.Instance);

        [Fact]
        public void IsOpaqueColor()
        {
            Assert.True(new GraphicsOptions(true).IsOpaqueColorWithoutBlending(Rgba32.Red));
            Assert.False(new GraphicsOptions(true, 0.5f).IsOpaqueColorWithoutBlending(Rgba32.Red));
            Assert.False(new GraphicsOptions(true).IsOpaqueColorWithoutBlending(Rgba32.Transparent));
            Assert.False(new GraphicsOptions(true, PixelColorBlendingMode.Lighten, 1).IsOpaqueColorWithoutBlending(Rgba32.Red));
            Assert.False(new GraphicsOptions(true, PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.DestOver, 1).IsOpaqueColorWithoutBlending(Rgba32.Red));
        }
    }

    public abstract class PixelOperationsTests<TPixel> : MeasureFixture
        where TPixel : struct, IPixel<TPixel>
    {
        protected PixelOperationsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static TheoryData<int> ArraySizesData => new TheoryData<int> { 0, 1, 2, 7, 16, 512, 513, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 1111 };

        internal static PixelOperations<TPixel> Operations => PixelOperations<TPixel>.Instance;

        internal static TPixel[] CreateExpectedPixelData(Vector4[] source)
        {
            var expected = new TPixel[source.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                expected[i].FromVector4(source[i]);
            }
            return expected;
        }

        internal static TPixel[] CreateScaledExpectedPixelData(Vector4[] source)
        {
            var expected = new TPixel[source.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                expected[i].FromScaledVector4(source[i]);
            }
            return expected;
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromVector4(int count)
        {
            Vector4[] source = CreateVector4TestData(count);
            TPixel[] expected = CreateExpectedPixelData(source);

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromVector4(s, d.GetSpan())
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromScaledVector4(int count)
        {
            Vector4[] source = CreateVector4TestData(count);
            TPixel[] expected = CreateScaledExpectedPixelData(source);

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromScaledVector4(s, d.GetSpan())
            );
        }

        internal static Vector4[] CreateExpectedVector4Data(TPixel[] source)
        {
            var expected = new Vector4[source.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = source[i].ToVector4();
            }
            return expected;
        }

        internal static Vector4[] CreateExpectedScaledVector4Data(TPixel[] source)
        {
            var expected = new Vector4[source.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = source[i].ToScaledVector4();
            }
            return expected;
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToVector4(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            Vector4[] expected = CreateExpectedVector4Data(source);

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToVector4(s, d.GetSpan())
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToScaledVector4(int count)
        {
            TPixel[] source = CreateScaledPixelTestData(count);
            Vector4[] expected = CreateExpectedScaledVector4Data(source);

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToScaledVector4(s, d.GetSpan())
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromArgb32Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 4);
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;

                expected[i].FromArgb32(new Argb32(source[i4 + 1], source[i4 + 2], source[i4 + 3], source[i4 + 0]));
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromArgb32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToArgb32Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 4];
            var argb = default(Argb32);

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;
                argb.FromScaledVector4(source[i].ToScaledVector4());

                expected[i4] = argb.A;
                expected[i4 + 1] = argb.R;
                expected[i4 + 2] = argb.G;
                expected[i4 + 3] = argb.B;
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToArgb32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromBgr24Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 3);
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i3 = i * 3;

                expected[i].FromBgr24(new Bgr24(source[i3 + 2], source[i3 + 1], source[i3]));
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromBgr24Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToBgr24Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 3];
            var bgr = default(Bgr24);

            for (int i = 0; i < count; i++)
            {
                int i3 = i * 3;
                bgr.FromScaledVector4(source[i].ToScaledVector4());
                expected[i3] = bgr.B;
                expected[i3 + 1] = bgr.G;
                expected[i3 + 2] = bgr.R;
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToBgr24Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromBgra32Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 4);
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;

                expected[i].FromBgra32(new Bgra32(source[i4 + 2], source[i4 + 1], source[i4 + 0], source[i4 + 3]));
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromBgra32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToBgra32Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 4];
            var bgra = default(Bgra32);

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;
                bgra.FromScaledVector4(source[i].ToScaledVector4());
                expected[i4] = bgra.B;
                expected[i4 + 1] = bgra.G;
                expected[i4 + 2] = bgra.R;
                expected[i4 + 3] = bgra.A;
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToBgra32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromRgb24Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 3);
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i3 = i * 3;

                expected[i].FromRgb24(new Rgb24(source[i3 + 0], source[i3 + 1], source[i3 + 2]));
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromRgb24Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToRgb24Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 3];
            var rgb = default(Rgb24);

            for (int i = 0; i < count; i++)
            {
                int i3 = i * 3;
                rgb.FromScaledVector4(source[i].ToScaledVector4());
                expected[i3] = rgb.R;
                expected[i3 + 1] = rgb.G;
                expected[i3 + 2] = rgb.B;
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToRgb24Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromRgba32Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 4);
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;

                expected[i].FromRgba32(new Rgba32(source[i4 + 0], source[i4 + 1], source[i4 + 2], source[i4 + 3]));
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromRgba32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToRgba32Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 4];
            var rgba = default(Rgba32);

            for (int i = 0; i < count; i++)
            {
                int i4 = i * 4;
                rgba.FromScaledVector4(source[i].ToScaledVector4());
                expected[i4] = rgba.R;
                expected[i4 + 1] = rgba.G;
                expected[i4 + 2] = rgba.B;
                expected[i4 + 3] = rgba.A;
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToRgba32Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromRgb48Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 6);
            Span<byte> sourceSpan = source.AsSpan();
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i6 = i * 6;
                expected[i].FromRgb48(MemoryMarshal.Cast<byte, Rgb48>(sourceSpan.Slice(i6, 6))[0]);
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromRgb48Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToRgb48Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 6];
            Rgb48 rgb = default;

            for (int i = 0; i < count; i++)
            {
                int i6 = i * 6;
                rgb.FromScaledVector4(source[i].ToScaledVector4());
                OctetBytes rgb48Bytes = Unsafe.As<Rgb48, OctetBytes>(ref rgb);
                expected[i6] = rgb48Bytes[0];
                expected[i6 + 1] = rgb48Bytes[1];
                expected[i6 + 2] = rgb48Bytes[2];
                expected[i6 + 3] = rgb48Bytes[3];
                expected[i6 + 4] = rgb48Bytes[4];
                expected[i6 + 5] = rgb48Bytes[5];
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToRgb48Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void FromRgba64Bytes(int count)
        {
            byte[] source = CreateByteTestData(count * 8);
            Span<byte> sourceSpan = source.AsSpan();
            var expected = new TPixel[count];

            for (int i = 0; i < count; i++)
            {
                int i8 = i * 8;
                expected[i].FromRgba64(MemoryMarshal.Cast<byte, Rgba64>(sourceSpan.Slice(i8, 8))[0]);
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.FromRgba64Bytes(s, d.GetSpan(), count)
            );
        }

        [Theory]
        [MemberData(nameof(ArraySizesData))]
        public void ToRgba64Bytes(int count)
        {
            TPixel[] source = CreatePixelTestData(count);
            byte[] expected = new byte[count * 8];
            Rgba64 rgba = default;

            for (int i = 0; i < count; i++)
            {
                int i8 = i * 8;
                rgba.FromScaledVector4(source[i].ToScaledVector4());
                OctetBytes rgba64Bytes = Unsafe.As<Rgba64, OctetBytes>(ref rgba);
                expected[i8] = rgba64Bytes[0];
                expected[i8 + 1] = rgba64Bytes[1];
                expected[i8 + 2] = rgba64Bytes[2];
                expected[i8 + 3] = rgba64Bytes[3];
                expected[i8 + 4] = rgba64Bytes[4];
                expected[i8 + 5] = rgba64Bytes[5];
                expected[i8 + 6] = rgba64Bytes[6];
                expected[i8 + 7] = rgba64Bytes[7];
            }

            TestOperation(
                source,
                expected,
                (s, d) => Operations.ToRgba64Bytes(s, d.GetSpan(), count)
            );
        }

        internal static void TestOperation<TSource, TDest>(
            TSource[] source,
            TDest[] expected,
            Action<TSource[], IMemoryOwner<TDest>> action)
            where TSource : struct
            where TDest : struct
        {
            using (var buffers = new TestBuffers<TSource, TDest>(source, expected))
            {
                action(buffers.SourceBuffer, buffers.ActualDestBuffer);
                buffers.Verify();
            }
        }

        internal static Vector4[] CreateVector4TestData(int length)
        {
            var result = new Vector4[length];
            var rnd = new Random(42); // Deterministic random values

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetVector(rnd);
            }
            return result;
        }

        internal static TPixel[] CreatePixelTestData(int length)
        {
            var result = new TPixel[length];

            var rnd = new Random(42); // Deterministic random values

            for (int i = 0; i < result.Length; i++)
            {
                Vector4 v = GetVector(rnd);
                result[i].FromVector4(v);
            }

            return result;
        }

        internal static TPixel[] CreateScaledPixelTestData(int length)
        {
            var result = new TPixel[length];

            var rnd = new Random(42); // Deterministic random values

            for (int i = 0; i < result.Length; i++)
            {
                Vector4 v = GetVector(rnd);
                result[i].FromScaledVector4(v);
            }

            return result;
        }

        internal static byte[] CreateByteTestData(int length)
        {
            byte[] result = new byte[length];
            var rnd = new Random(42); // Deterministic random values

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)rnd.Next(255);
            }
            return result;
        }

        internal static Vector4 GetVector(Random rnd)
        {
            return new Vector4(
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble()
            );
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct OctetBytes
        {
            public fixed byte Data[8];

            public byte this[int idx]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref byte self = ref Unsafe.As<OctetBytes, byte>(ref this);
                    return Unsafe.Add(ref self, idx);
                }
            }
        }

        private class TestBuffers<TSource, TDest> : IDisposable
            where TSource : struct
            where TDest : struct
        {
            public TSource[] SourceBuffer { get; }
            public IMemoryOwner<TDest> ActualDestBuffer { get; }
            public TDest[] ExpectedDestBuffer { get; }

            public TestBuffers(TSource[] source, TDest[] expectedDest)
            {
                this.SourceBuffer = source;
                this.ExpectedDestBuffer = expectedDest;
                this.ActualDestBuffer = Configuration.Default.MemoryAllocator.Allocate<TDest>(expectedDest.Length);
            }

            public void Dispose() => this.ActualDestBuffer.Dispose();

            public void Verify()
            {
                int count = this.ExpectedDestBuffer.Length;

                if (typeof(TDest) == typeof(Vector4))
                {
                    Span<Vector4> expected = MemoryMarshal.Cast<TDest, Vector4>(this.ExpectedDestBuffer.AsSpan());
                    Span<Vector4> actual = MemoryMarshal.Cast<TDest, Vector4>(this.ActualDestBuffer.GetSpan());

                    var comparer = new ApproximateFloatComparer(0.001f);
                    for (int i = 0; i < count; i++)
                    {
                        // ReSharper disable PossibleNullReferenceException
                        Assert.Equal(expected[i], actual[i], comparer);
                        // ReSharper restore PossibleNullReferenceException
                    }
                }
                else
                {
                    Span<TDest> expected = this.ExpectedDestBuffer.AsSpan();
                    Span<TDest> actual = this.ActualDestBuffer.GetSpan();
                    for (int i = 0; i < count; i++)
                    {
                        Assert.Equal(expected[i], actual[i]);
                    }
                }
            }
        }
    }
}