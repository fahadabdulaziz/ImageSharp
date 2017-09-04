namespace ImageSharp.Benchmarks.General.Vectorization
{
    using System;
    using System.Numerics;

    using BenchmarkDotNet.Attributes;

    public class DivFloat : SIMDBenchmarkBase<float>.Divide
    {
        protected override float GetTestValue() => 42;

        [Benchmark(Baseline = true)]
        public void Standard()
        {
            float v = this.testValue;
            for (int i = 0; i < this.input.Length; i++)
            {
                this.result[i] = this.input[i] / v;
            }
        }
    }

    public class Divide : SIMDBenchmarkBase<uint>.Divide
    {
        protected override uint GetTestValue() => 42;

        [Benchmark(Baseline = true)]
        public void Standard()
        {
            uint v = this.testValue;
            for (int i = 0; i < this.input.Length; i++)
            {
                this.result[i] = this.input[i] / v;
            }
        }
    }

    public class DivInt32 : SIMDBenchmarkBase<int>.Divide
    {
        protected override int GetTestValue() => 42;

        [Benchmark(Baseline = true)]
        public void Standard()
        {
            int v = this.testValue;
            for (int i = 0; i < this.input.Length; i++)
            {
                this.result[i] = this.input[i] / v;
            }
        }
    }

    public class DivInt16 : SIMDBenchmarkBase<short>.Divide
    {
        protected override short GetTestValue() => 42;

        protected override Vector<short> GetTestVector() => new Vector<short>(new short[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17});

        [Benchmark(Baseline = true)]
        public void Standard()
        {
            short v = this.testValue;
            for (int i = 0; i < this.input.Length; i++)
            {
                this.result[i] = (short)(this.input[i] / v);
            }
        }
    }
}