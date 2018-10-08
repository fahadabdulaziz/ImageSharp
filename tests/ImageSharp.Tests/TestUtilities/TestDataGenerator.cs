﻿using System;
using System.Numerics;

namespace SixLabors.ImageSharp.Tests
{
    internal static class TestDataGenerator
    {
        public static float[] GenerateRandomFloatArray(this Random rnd, int length, float minVal, float maxVal)
        {
            float[] values = new float[length];

            for (int i = 0; i < length; i++)
            {
                values[i] = GetRandomFloat(rnd, minVal, maxVal);
            }

            return values;
        }

        public static Vector4[] GenerateRandomVectorArray(this Random rnd, int length, float minVal, float maxVal)
        {
            var values = new Vector4[length];

            for (int i = 0; i < length; i++)
            {
                ref Vector4 v = ref values[i];
                v.X = GetRandomFloat(rnd, minVal, maxVal);
                v.Y = GetRandomFloat(rnd, minVal, maxVal);
                v.Z = GetRandomFloat(rnd, minVal, maxVal);
                v.W = GetRandomFloat(rnd, minVal, maxVal);
            }

            return values;
        }

        public static float[] GenerateRandomRoundedFloatArray(this Random rnd, int length, int minVal, int maxValExclusive)
        {
            float[] values = new float[length];

            for (int i = 0; i < length; i++)
            {
                int val = rnd.Next(minVal, maxValExclusive);
                values[i] = (float)val;
            }

            return values;
        }

        private static float GetRandomFloat(Random rnd, float minVal, float maxVal)
        {
            return (float)rnd.NextDouble() * (maxVal - minVal) + minVal;
        }
    }
}