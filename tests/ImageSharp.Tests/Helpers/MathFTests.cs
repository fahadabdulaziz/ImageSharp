﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Helpers
{
    public class MathFTests
    {
        [Fact]
        public void MathF_PI_Is_Equal()
        {
            Assert.Equal(MathF.PI, (float)Math.PI);
        }

        [Fact]
        public void MathF_Ceililng_Is_Equal()
        {
            Assert.Equal(MathF.Ceiling(0.3333F), (float)Math.Ceiling(0.3333F));
        }

        [Fact]
        public void MathF_Cos_Is_Equal()
        {
            Assert.Equal(MathF.Cos(0.3333F), (float)Math.Cos(0.3333F));
        }

        [Fact]
        public void MathF_Abs_Is_Equal()
        {
            Assert.Equal(MathF.Abs(-0.3333F), (float)Math.Abs(-0.3333F));
        }

        [Fact]
        public void MathF_Atan2_Is_Equal()
        {
            Assert.Equal(MathF.Atan2(1.2345F, 1.2345F), (float)Math.Atan2(1.2345F, 1.2345F));
        }

        [Fact]
        public void MathF_Exp_Is_Equal()
        {
            Assert.Equal(MathF.Exp(1.2345F), (float)Math.Exp(1.2345F));
        }

        [Fact]
        public void MathF_Floor_Is_Equal()
        {
            Assert.Equal(MathF.Floor(1.2345F), (float)Math.Floor(1.2345F));
        }

        [Fact]
        public void MathF_Min_Is_Equal()
        {
            Assert.Equal(MathF.Min(1.2345F, 5.4321F), (float)Math.Min(1.2345F, 5.4321F));
        }

        [Fact]
        public void MathF_Max_Is_Equal()
        {
            Assert.Equal(MathF.Max(1.2345F, 5.4321F), (float)Math.Max(1.2345F, 5.4321F));
        }

        [Fact]
        public void MathF_Pow_Is_Equal()
        {
            Assert.Equal(MathF.Pow(1.2345F, 5.4321F), (float)Math.Pow(1.2345F, 5.4321F));
        }

        [Fact]
        public void MathF_Round_Is_Equal()
        {
            Assert.Equal(MathF.Round(1.2345F), (float)Math.Round(1.2345F));
        }

        [Fact]
        public void MathF_Round_With_Midpoint_Is_Equal()
        {
            Assert.Equal(MathF.Round(1.2345F, MidpointRounding.AwayFromZero), (float)Math.Round(1.2345F, MidpointRounding.AwayFromZero));
        }

        [Fact]
        public void MathF_Sin_Is_Equal()
        {
            Assert.Equal(MathF.Sin(1.2345F), (float)Math.Sin(1.2345F));
        }

        [Fact]
        public void MathF_SinC_Is_Equal()
        {
            float f = 1.2345F;
            float expected = 1F;
            if (Math.Abs(f) > Constants.Epsilon)
            {
                f *= (float)Math.PI;
                float sinC = (float)Math.Sin(f) / f;

                expected = Math.Abs(sinC) < Constants.Epsilon ? 0F : sinC;
            }
            
            Assert.Equal(MathF.SinC(1.2345F), expected);
        }

        [Fact]
        public void MathF_Sqrt_Is_Equal()
        {
            Assert.Equal(MathF.Sqrt(2F), (float)Math.Sqrt(2F));
        }

        [Fact]
        public void Convert_Degree_To_Radian()
        {
            Assert.Equal((float)(Math.PI / 2D), MathF.DegreeToRadian(90F), new FloatRoundingComparer(6));
        }

        [Fact]
        public void Convert_Radian_To_Degree()
        {
            Assert.Equal(60F, MathF.RadianToDegree((float)(Math.PI / 3D)), new FloatRoundingComparer(5));
        }
    }
}