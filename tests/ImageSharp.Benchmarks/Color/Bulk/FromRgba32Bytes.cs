// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

// ReSharper disable InconsistentNaming

using System.Buffers;
using System;

using BenchmarkDotNet.Attributes;

using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Benchmarks.ColorSpaces.Bulk
{
    public abstract class FromRgba32Bytes<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        private IMemoryOwner<TPixel> destination;

        private IMemoryOwner<byte> source;

        [Params(16, 128, 1024)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            this.destination = Configuration.Default.MemoryAllocator.Allocate<TPixel>(this.Count);
            this.source = Configuration.Default.MemoryAllocator.Allocate<byte>(this.Count * 4);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            this.destination.Dispose();
            this.source.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void PerElement()
        {
            Span<byte> s = this.source.GetSpan();
            Span<TPixel> d = this.destination.GetSpan();
            
            for (int i = 0; i < this.Count; i++)
            {
                int i4 = i * 4;
                var c = default(TPixel);
                c.FromRgba32(new Rgba32(s[i4], s[i4 + 1], s[i4 + 2], s[i4 + 3]));
                d[i] = c;
            }
        }

        [Benchmark]
        public void CommonBulk()
        {
            new PixelOperations<TPixel>().FromRgba32Bytes(this.source.GetSpan(), this.destination.GetSpan(), this.Count);
        }

        [Benchmark]
        public void OptimizedBulk()
        {
           PixelOperations<TPixel>.Instance.FromRgba32Bytes(this.source.GetSpan(), this.destination.GetSpan(), this.Count);
        }
    }

    public class FromRgba32BytesRgba32 : FromRgba32Bytes<Rgba32>
    {
    }
}