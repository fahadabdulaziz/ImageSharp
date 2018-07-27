﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using SixLabors.Shapes;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Drawing.Paths
{
    public class FillPathCollection : BaseImageOperationsExtensionTest
    {
        GraphicsOptions noneDefault = new GraphicsOptions();
        Rgba32 color = Rgba32.HotPink;
        SolidBrush<Rgba32> brush = Brushes.Solid(Rgba32.HotPink);
        IPath path1 = new Path(new LinearLineSegment(new SixLabors.Primitives.PointF[] {
                    new Vector2(10,10),
                    new Vector2(20,10),
                    new Vector2(20,10),
                    new Vector2(30,10),
                }));
        IPath path2 = new Path(new LinearLineSegment(new SixLabors.Primitives.PointF[] {
                    new Vector2(10,10),
                    new Vector2(20,10),
                    new Vector2(20,10),
                    new Vector2(30,10),
                }));

        IPathCollection pathCollection;

        public FillPathCollection()
        {
            this.pathCollection = new PathCollection(this.path1, this.path2);
        }

        [Fact]
        public void CorrectlySetsBrushAndPath()
        {
            this.operations.Fill(this.brush, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor<Rgba32> processor = this.Verify<FillRegionProcessor<Rgba32>>(i);

                Assert.Equal(GraphicsOptions.Default, processor.Options);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);

                // path is converted to a polygon before filling
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                LinearLineSegment segments = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                Assert.Equal(this.brush, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsBrushPathOptions()
        {
            this.operations.Fill(this.noneDefault, this.brush, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor<Rgba32> processor = this.Verify<FillRegionProcessor<Rgba32>>(i);

                Assert.Equal(this.noneDefault, processor.Options);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                LinearLineSegment segments = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                Assert.Equal(this.brush, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsColorAndPath()
        {
            this.operations.Fill(this.color, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor<Rgba32> processor = this.Verify<FillRegionProcessor<Rgba32>>(i);

                Assert.Equal(GraphicsOptions.Default, processor.Options);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                LinearLineSegment segments = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                SolidBrush<Rgba32> brush = Assert.IsType<SolidBrush<Rgba32>>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }

        [Fact]
        public void CorrectlySetsColorPathAndOptions()
        {
            this.operations.Fill(this.noneDefault, this.color, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor<Rgba32> processor = this.Verify<FillRegionProcessor<Rgba32>>(i);

                Assert.Equal(this.noneDefault, processor.Options);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                LinearLineSegment segments = Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                SolidBrush<Rgba32> brush = Assert.IsType<SolidBrush<Rgba32>>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }
    }
}
