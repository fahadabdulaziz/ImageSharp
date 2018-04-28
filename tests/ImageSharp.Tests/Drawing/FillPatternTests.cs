﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Brushes;
using SixLabors.ImageSharp.Processing.Transforms;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Drawing
{
    public class FillPatternBrushTests : FileTestBase
    {
        private void Test(string name, Rgba32 background, IBrush<Rgba32> brush, Rgba32[,] expectedPattern)
        {
            string path = TestEnvironment.CreateOutputDirectory("Fill", "PatternBrush");
            using (var image = new Image<Rgba32>(20, 20))
            {
                image.Mutate(x => x
                    .Fill(background)
                    .Fill(brush));

                image.Save($"{path}/{name}.png");

                using (PixelAccessor<Rgba32> sourcePixels = image.Lock())
                {
                    // lets pick random spots to start checking
                    var r = new Random();
                    var expectedPatternFast = new DenseMatrix<Rgba32>(expectedPattern);
                    int xStride = expectedPatternFast.Columns;
                    int yStride = expectedPatternFast.Rows;
                    int offsetX = r.Next(image.Width / xStride) * xStride;
                    int offsetY = r.Next(image.Height / yStride) * yStride;
                    for (int x = 0; x < xStride; x++)
                    {
                        for (int y = 0; y < yStride; y++)
                        {
                            int actualX = x + offsetX;
                            int actualY = y + offsetY;
                            Rgba32 expected = expectedPatternFast[y, x]; // inverted pattern
                            Rgba32 actual = sourcePixels[actualX, actualY];
                            if (expected != actual)
                            {
                                Assert.True(false, $"Expected {expected} but found {actual} at ({actualX},{actualY})");
                            }
                        }
                    }
                }
                image.Mutate(x => x.Resize(80, 80));
                image.Save($"{path}/{name}x4.png");
            }
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent10()
        {
            this.Test("Percent10", Rgba32.Blue, Brushes.Percent10(Rgba32.HotPink, Rgba32.LimeGreen),
                new[,]
                {
                { Rgba32.HotPink , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink , Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen}
            });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent10Transparent()
        {
            Test("Percent10_Transparent", Rgba32.Blue, Brushes.Percent10(Rgba32.HotPink),
            new Rgba32[,] {
                { Rgba32.HotPink , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.HotPink , Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.Blue, Rgba32.Blue}
            });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent20()
        {
            Test("Percent20", Rgba32.Blue, Brushes.Percent20(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.HotPink , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink , Rgba32.LimeGreen},
                { Rgba32.HotPink , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink , Rgba32.LimeGreen}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent20_transparent()
        {
            Test("Percent20_Transparent", Rgba32.Blue, Brushes.Percent20(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.HotPink , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.HotPink , Rgba32.Blue},
                { Rgba32.HotPink , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.HotPink , Rgba32.Blue}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithHorizontal()
        {
            Test("Horizontal", Rgba32.Blue, Brushes.Horizontal(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.LimeGreen , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.HotPink, Rgba32.HotPink, Rgba32.HotPink , Rgba32.HotPink},
                { Rgba32.LimeGreen , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen , Rgba32.LimeGreen}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithHorizontal_transparent()
        {
            Test("Horizontal_Transparent", Rgba32.Blue, Brushes.Horizontal(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.Blue , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.HotPink, Rgba32.HotPink, Rgba32.HotPink , Rgba32.HotPink},
                { Rgba32.Blue , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.Blue , Rgba32.Blue}
           });
        }



        [Fact]
        public void ImageShouldBeFloodFilledWithMin()
        {
            Test("Min", Rgba32.Blue, Brushes.Min(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.LimeGreen , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen , Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen , Rgba32.LimeGreen},
                { Rgba32.HotPink, Rgba32.HotPink, Rgba32.HotPink , Rgba32.HotPink}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithMin_transparent()
        {
            Test("Min_Transparent", Rgba32.Blue, Brushes.Min(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.Blue , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue , Rgba32.Blue, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.Blue, Rgba32.Blue , Rgba32.Blue},
                { Rgba32.HotPink, Rgba32.HotPink, Rgba32.HotPink , Rgba32.HotPink},
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithVertical()
        {
            Test("Vertical", Rgba32.Blue, Brushes.Vertical(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithVertical_transparent()
        {
            Test("Vertical_Transparent", Rgba32.Blue, Brushes.Vertical(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.Blue, Rgba32.HotPink, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.HotPink, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.HotPink, Rgba32.Blue, Rgba32.Blue},
                { Rgba32.Blue, Rgba32.HotPink, Rgba32.Blue, Rgba32.Blue}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithForwardDiagonal()
        {
            Test("ForwardDiagonal", Rgba32.Blue, Brushes.ForwardDiagonal(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.HotPink, Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithForwardDiagonal_transparent()
        {
            Test("ForwardDiagonal_Transparent", Rgba32.Blue, Brushes.ForwardDiagonal(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.Blue,    Rgba32.Blue,    Rgba32.Blue,    Rgba32.HotPink},
                { Rgba32.Blue,    Rgba32.Blue,    Rgba32.HotPink, Rgba32.Blue},
                { Rgba32.Blue,    Rgba32.HotPink, Rgba32.Blue,    Rgba32.Blue},
                { Rgba32.HotPink, Rgba32.Blue,    Rgba32.Blue,    Rgba32.Blue}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithBackwardDiagonal()
        {
            Test("BackwardDiagonal", Rgba32.Blue, Brushes.BackwardDiagonal(Rgba32.HotPink, Rgba32.LimeGreen),
           new Rgba32[,] {
                { Rgba32.HotPink,   Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.HotPink,   Rgba32.LimeGreen, Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink,   Rgba32.LimeGreen},
                { Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.LimeGreen, Rgba32.HotPink}
           });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithBackwardDiagonal_transparent()
        {
            Test("BackwardDiagonal_Transparent", Rgba32.Blue, Brushes.BackwardDiagonal(Rgba32.HotPink),
           new Rgba32[,] {
                { Rgba32.HotPink, Rgba32.Blue,    Rgba32.Blue,    Rgba32.Blue},
                { Rgba32.Blue,    Rgba32.HotPink, Rgba32.Blue,    Rgba32.Blue},
                { Rgba32.Blue,    Rgba32.Blue,    Rgba32.HotPink, Rgba32.Blue},
                { Rgba32.Blue,    Rgba32.Blue,    Rgba32.Blue,    Rgba32.HotPink}
           });
        }
    }
}
