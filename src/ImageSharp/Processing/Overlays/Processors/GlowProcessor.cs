﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Overlays.Processors
{
    /// <summary>
    /// An <see cref="IImageProcessor{TPixel}"/> that applies a radial glow effect an <see cref="Image{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal class GlowProcessor<TPixel> : ImageProcessor<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        private readonly PixelBlender<TPixel> blender;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlowProcessor{TPixel}" /> class.
        /// </summary>
        /// <param name="color">The color or the glow.</param>
        public GlowProcessor(TPixel color)
            : this(color, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlowProcessor{TPixel}" /> class.
        /// </summary>
        /// <param name="color">The color or the glow.</param>
        /// <param name="radius">The radius of the glow.</param>
        public GlowProcessor(TPixel color, ValueSize radius)
            : this(color, radius, GraphicsOptions.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlowProcessor{TPixel}" /> class.
        /// </summary>
        /// <param name="color">The color or the glow.</param>
        /// <param name="options">The options effecting blending and composition.</param>
        public GlowProcessor(TPixel color, GraphicsOptions options)
            : this(color, 0, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlowProcessor{TPixel}" /> class.
        /// </summary>
        /// <param name="color">The color or the glow.</param>
        /// <param name="radius">The radius of the glow.</param>
        /// <param name="options">The options effecting blending and composition.</param>
        public GlowProcessor(TPixel color, ValueSize radius, GraphicsOptions options)
        {
            this.GlowColor = color;
            this.Radius = radius;
            this.blender = PixelOperations<TPixel>.Instance.GetPixelBlender(options.BlenderMode);
            this.GraphicsOptions = options;
        }

        /// <summary>
        /// Gets the options effecting blending and composition
        /// </summary>
        public GraphicsOptions GraphicsOptions { get; }

        /// <summary>
        /// Gets or sets the glow color to apply.
        /// </summary>
        public TPixel GlowColor { get; set; }

        /// <summary>
        /// Gets or sets the the radius.
        /// </summary>
        public ValueSize Radius { get; set; }

        /// <inheritdoc/>
        protected override void OnFrameApply(ImageFrame<TPixel> source, Rectangle sourceRectangle, Configuration configuration)
        {
            int startY = sourceRectangle.Y;
            int endY = sourceRectangle.Bottom;
            int startX = sourceRectangle.X;
            int endX = sourceRectangle.Right;
            TPixel glowColor = this.GlowColor;
            Vector2 center = Rectangle.Center(sourceRectangle);

            float finalRadius = this.Radius.Calculate(source.Size());

            float maxDistance = finalRadius > 0 ? MathF.Min(finalRadius, sourceRectangle.Width * .5F) : sourceRectangle.Width * .5F;

            // Align start/end positions.
            int minX = Math.Max(0, startX);
            int maxX = Math.Min(source.Width, endX);
            int minY = Math.Max(0, startY);
            int maxY = Math.Min(source.Height, endY);

            // Reset offset if necessary.
            if (minX > 0)
            {
                startX = 0;
            }

            if (minY > 0)
            {
                startY = 0;
            }

            int width = maxX - minX;
            using (IBuffer<TPixel> rowColors = source.MemoryManager.Allocate<TPixel>(width))
            {
                // Be careful! Do not capture rowColorsSpan in the lambda below!
                Span<TPixel> rowColorsSpan = rowColors.Span;

                for (int i = 0; i < width; i++)
                {
                    rowColorsSpan[i] = glowColor;
                }

                Parallel.For(
                    minY,
                    maxY,
                    configuration.ParallelOptions,
                    y =>
                    {
                        using (IBuffer<float> amounts = source.MemoryManager.Allocate<float>(width))
                        {
                            Span<float> amountsSpan = amounts.Span;
                            int offsetY = y - startY;
                            int offsetX = minX - startX;
                            for (int i = 0; i < width; i++)
                            {
                                float distance = Vector2.Distance(center, new Vector2(i + offsetX, offsetY));
                                amountsSpan[i] = (this.GraphicsOptions.BlendPercentage * (1 - (.95F * (distance / maxDistance)))).Clamp(0, 1);
                            }

                            Span<TPixel> destination = source.GetPixelRowSpan(offsetY).Slice(offsetX, width);

                            this.blender.Blend(source.MemoryManager, destination, destination, rowColors.Span, amountsSpan);
                        }
                    });
            }
        }
    }
}