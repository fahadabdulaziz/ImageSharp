﻿using System.Numerics;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using SixLabors.ImageSharp.Tuples;

namespace SixLabors.ImageSharp.Benchmarks.General.Vectorization
{
    [Config(typeof(Config.ShortClr))]
    public class WidenBytesToUInt32
    {
        private byte[] source;

        private uint[] dest;

        private const int Count = 64;

        [GlobalSetup]
        public void Setup()
        {
            this.source = new byte[Count];
            this.dest = new uint[Count];
        }

        [Benchmark(Baseline = true)]
        public void Standard()
        {
            const int N = Count / 8;

            ref Octet.OfByte sBase = ref Unsafe.As<byte, Octet.OfByte>(ref this.source[0]);
            ref Octet.OfUInt32 dBase = ref Unsafe.As<uint, Octet.OfUInt32>(ref this.dest[0]);

            for (int i = 0; i < N; i++)
            {
                Unsafe.Add(ref dBase, i).LoadFrom(ref Unsafe.Add(ref sBase, i));
            }
        }

        [Benchmark]
        public void Simd()
        {
            int n = Count / Vector<byte>.Count;

            ref Vector<byte> sBase = ref Unsafe.As<byte, Vector<byte>>(ref this.source[0]);
            ref Vector<uint> dBase = ref Unsafe.As<uint, Vector<uint>>(ref this.dest[0]);

            for (int i = 0; i < n; i++)
            {
                Vector<byte> b = Unsafe.Add(ref sBase, i);

                Vector.Widen(b, out Vector<ushort> s0, out Vector<ushort> s1);
                Vector.Widen(s0, out Vector<uint> w0, out Vector<uint> w1);
                Vector.Widen(s1, out Vector<uint> w2, out Vector<uint> w3);

                ref Vector<uint> d = ref Unsafe.Add(ref dBase, i * 4);
                d = w0;
                Unsafe.Add(ref d, 1) = w1;
                Unsafe.Add(ref d, 2) = w2;
                Unsafe.Add(ref d, 3) = w3;
            }
        }
    }
}