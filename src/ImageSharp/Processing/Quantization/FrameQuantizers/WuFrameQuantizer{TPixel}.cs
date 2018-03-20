﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Processing.Quantization.FrameQuantizers
{
    /// <summary>
    /// An implementation of Wu's color quantizer with alpha channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Based on C Implementation of Xiaolin Wu's Color Quantizer (v. 2)
    /// (see Graphics Gems volume II, pages 126-133)
    /// (<see href="http://www.ece.mcmaster.ca/~xwu/cq.c"/>).
    /// </para>
    /// <para>
    /// This adaptation is based on the excellent JeremyAnsel.ColorQuant by Jérémy Ansel
    /// <see href="https://github.com/JeremyAnsel/JeremyAnsel.ColorQuant"/>
    /// </para>
    /// <para>
    /// Algorithm: Greedy orthogonal bipartition of RGB space for variance minimization aided by inclusion-exclusion tricks.
    /// For speed no nearest neighbor search is done. Slightly better performance can be expected by more sophisticated
    /// but more expensive versions.
    /// </para>
    /// </remarks>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal sealed class WuFrameQuantizer<TPixel> : FrameQuantizerBase<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
        // TODO: The WuFrameQuantizer<TPixel> code is rising several questions:
        // - Do we really need to ALWAYS allocate the whole table of size TableLength? (~ 2471625 * sizeof(long) * 5 bytes )
        // - Isn't an AOS ("array of structures") layout more efficient & more readable than SOA ("structure of arrays") for this particular use case?
        //   (T, R, G, B, A, M2) could be grouped together!
        // - There are per-pixel virtual calls in InitialQuantizePixel, why not do it on a per-row basis?
        // - It's a frequently used class, we need tests! (So we can optimize safely.) There are tests in the original!!! We should just adopt them!
        //   https://github.com/JeremyAnsel/JeremyAnsel.ColorQuant/blob/master/JeremyAnsel.ColorQuant/JeremyAnsel.ColorQuant.Tests/WuColorQuantizerTests.cs

        /// <summary>
        /// The index bits.
        /// </summary>
        private const int IndexBits = 6;

        /// <summary>
        /// The index alpha bits.
        /// </summary>
        private const int IndexAlphaBits = 3;

        /// <summary>
        /// The index count.
        /// </summary>
        private const int IndexCount = (1 << IndexBits) + 1;

        /// <summary>
        /// The index alpha count.
        /// </summary>
        private const int IndexAlphaCount = (1 << IndexAlphaBits) + 1;

        /// <summary>
        /// The table length.
        /// </summary>
        private const int TableLength = IndexCount * IndexCount * IndexCount * IndexAlphaCount;

        /// <summary>
        /// A lookup table for colors
        /// </summary>
        private readonly Dictionary<TPixel, byte> colorMap = new Dictionary<TPixel, byte>();

        /// <summary>
        /// Moment of <c>P(c)</c>.
        /// </summary>
        private IBuffer<long> vwt;

        /// <summary>
        /// Moment of <c>r*P(c)</c>.
        /// </summary>
        private IBuffer<long> vmr;

        /// <summary>
        /// Moment of <c>g*P(c)</c>.
        /// </summary>
        private IBuffer<long> vmg;

        /// <summary>
        /// Moment of <c>b*P(c)</c>.
        /// </summary>
        private IBuffer<long> vmb;

        /// <summary>
        /// Moment of <c>a*P(c)</c>.
        /// </summary>
        private IBuffer<long> vma;

        /// <summary>
        /// Moment of <c>c^2*P(c)</c>.
        /// </summary>
        private IBuffer<float> m2;

        /// <summary>
        /// Color space tag.
        /// </summary>
        private IBuffer<byte> tag;

        /// <summary>
        /// Maximum allowed color depth
        /// </summary>
        private int colors;

        /// <summary>
        /// The reduced image palette
        /// </summary>
        private TPixel[] palette;

        /// <summary>
        /// The color cube representing the image palette
        /// </summary>
        private Box[] colorCube;

        /// <summary>
        /// Initializes a new instance of the <see cref="WuFrameQuantizer{TPixel}"/> class.
        /// </summary>
        /// <param name="quantizer">The wu quantizer</param>
        /// <remarks>
        /// The Wu quantizer is a two pass algorithm. The initial pass sets up the 3-D color histogram,
        /// the second pass quantizes a color based on the position in the histogram.
        /// </remarks>
        public WuFrameQuantizer(WuQuantizer quantizer)
            : base(quantizer, false)
        {
            this.colors = quantizer.MaxColors;
        }

        /// <inheritdoc/>
        public override QuantizedFrame<TPixel> QuantizeFrame(ImageFrame<TPixel> image)
        {
            Guard.NotNull(image, nameof(image));
            MemoryManager memoryManager = image.MemoryManager;

            try
            {
                this.vwt = memoryManager.AllocateClean<long>(TableLength);
                this.vmr = memoryManager.AllocateClean<long>(TableLength);
                this.vmg = memoryManager.AllocateClean<long>(TableLength);
                this.vmb = memoryManager.AllocateClean<long>(TableLength);
                this.vma = memoryManager.AllocateClean<long>(TableLength);
                this.m2 = memoryManager.AllocateClean<float>(TableLength);
                this.tag = memoryManager.AllocateClean<byte>(TableLength);

                return base.QuantizeFrame(image);
            }
            finally
            {
                this.vwt.Dispose();
                this.vmr.Dispose();
                this.vmg.Dispose();
                this.vmb.Dispose();
                this.vma.Dispose();
                this.m2.Dispose();
                this.tag.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override TPixel[] GetPalette()
        {
            if (this.palette == null)
            {
                this.palette = new TPixel[this.colors];
                for (int k = 0; k < this.colors; k++)
                {
                    this.Mark(ref this.colorCube[k], (byte)k);

                    float weight = Volume(ref this.colorCube[k], this.vwt.Span);

                    if (MathF.Abs(weight) > Constants.Epsilon)
                    {
                        float r = Volume(ref this.colorCube[k], this.vmr.Span);
                        float g = Volume(ref this.colorCube[k], this.vmg.Span);
                        float b = Volume(ref this.colorCube[k], this.vmb.Span);
                        float a = Volume(ref this.colorCube[k], this.vma.Span);

                        ref TPixel color = ref this.palette[k];
                        color.PackFromVector4(new Vector4(r, g, b, a) / weight / 255F);
                    }
                }
            }

            return this.palette;
        }

        /// <summary>
        /// Quantizes the pixel
        /// </summary>
        /// <param name="rgba">The rgba used to quantize the pixel input</param>
        private void QuantizePixel(ref Rgba32 rgba)
        {
            // Add the color to a 3-D color histogram.
            int r = rgba.R >> (8 - IndexBits);
            int g = rgba.G >> (8 - IndexBits);
            int b = rgba.B >> (8 - IndexBits);
            int a = rgba.A >> (8 - IndexAlphaBits);

            int index = GetPaletteIndex(r + 1, g + 1, b + 1, a + 1);

            Span<long> vwtSpan = this.vwt.Span;
            Span<long> vmrSpan = this.vmr.Span;
            Span<long> vmgSpan = this.vmg.Span;
            Span<long> vmbSpan = this.vmb.Span;
            Span<long> vmaSpan = this.vma.Span;
            Span<float> m2Span = this.m2.Span;

            vwtSpan[index]++;
            vmrSpan[index] += rgba.R;
            vmgSpan[index] += rgba.G;
            vmbSpan[index] += rgba.B;
            vmaSpan[index] += rgba.A;

            var vector = new Vector4(rgba.R, rgba.G, rgba.B, rgba.A);
            m2Span[index] += Vector4.Dot(vector, vector);
        }

        /// <inheritdoc/>
        protected override void FirstPass(ImageFrame<TPixel> source, int width, int height)
        {
            // Build up the 3-D color histogram
            // Loop through each row
            for (int y = 0; y < height; y++)
            {
                Span<TPixel> row = source.GetPixelRowSpan(y);
                ref TPixel scanBaseRef = ref MemoryMarshal.GetReference(row);

                // And loop through each column
                var rgba = default(Rgba32);
                for (int x = 0; x < width; x++)
                {
                    ref TPixel pixel = ref Unsafe.Add(ref scanBaseRef, x);
                    pixel.ToRgba32(ref rgba);
                    this.QuantizePixel(ref rgba);
                }
            }

            this.Get3DMoments(source.MemoryManager);
            this.BuildCube();
        }

        /// <inheritdoc/>
        protected override void SecondPass(ImageFrame<TPixel> source, byte[] output, int width, int height)
        {
            // Load up the values for the first pixel. We can use these to speed up the second
            // pass of the algorithm by avoiding transforming rows of identical color.
            TPixel sourcePixel = source[0, 0];
            TPixel previousPixel = sourcePixel;
            byte pixelValue = this.QuantizePixel(sourcePixel);
            TPixel[] colorPalette = this.GetPalette();
            TPixel transformedPixel = colorPalette[pixelValue];

            for (int y = 0; y < height; y++)
            {
                Span<TPixel> row = source.GetPixelRowSpan(y);

                // And loop through each column
                for (int x = 0; x < width; x++)
                {
                    // Get the pixel.
                    sourcePixel = row[x];

                    // Check if this is the same as the last pixel. If so use that value
                    // rather than calculating it again. This is an inexpensive optimization.
                    if (!previousPixel.Equals(sourcePixel))
                    {
                        // Quantize the pixel
                        pixelValue = this.QuantizePixel(sourcePixel);

                        // And setup the previous pointer
                        previousPixel = sourcePixel;

                        if (this.Dither)
                        {
                            transformedPixel = colorPalette[pixelValue];
                        }
                    }

                    if (this.Dither)
                    {
                        // Apply the dithering matrix. We have to reapply the value now as the original has changed.
                        this.Diffuser.Dither(source, sourcePixel, transformedPixel, x, y, 0, 0, width, height);
                    }

                    output[(y * source.Width) + x] = pixelValue;
                }
            }
        }

        /// <summary>
        /// Gets the index of the given color in the palette.
        /// </summary>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <param name="a">The alpha value.</param>
        /// <returns>The index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPaletteIndex(int r, int g, int b, int a)
        {
            return (r << ((IndexBits * 2) + IndexAlphaBits)) + (r << (IndexBits + IndexAlphaBits + 1))
                   + (g << (IndexBits + IndexAlphaBits)) + (r << (IndexBits * 2)) + (r << (IndexBits + 1))
                   + (g << IndexBits) + ((r + g + b) << IndexAlphaBits) + r + g + b + a;
        }

        /// <summary>
        /// Computes sum over a box of any given statistic.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static float Volume(ref Box cube, Span<long> moment)
        {
            return moment[GetPaletteIndex(cube.R1, cube.G1, cube.B1, cube.A1)]
                   - moment[GetPaletteIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                   - moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                   + moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                   - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                   + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                   + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                   - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                   - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                   + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                   + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                   - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                   + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                   - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                   - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                   + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];
        }

        /// <summary>
        /// Computes part of Volume(cube, moment) that doesn't depend on r1, g1, or b1 (depending on direction).
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static long Bottom(ref Box cube, int direction, Span<long> moment)
        {
            switch (direction)
            {
                // Red
                case 3:
                    return -moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Green
                case 2:
                    return -moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Blue
                case 1:
                    return -moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Alpha
                case 0:
                    return -moment[GetPaletteIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>
        /// Computes remainder of Volume(cube, moment), substituting position for r1, g1, or b1 (depending on direction).
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="position">The position.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static long Top(ref Box cube, int direction, int position, Span<long> moment)
        {
            switch (direction)
            {
                // Red
                case 3:
                    return moment[GetPaletteIndex(position, cube.G1, cube.B1, cube.A1)]
                           - moment[GetPaletteIndex(position, cube.G1, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(position, cube.G1, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(position, cube.G1, cube.B0, cube.A0)]
                           - moment[GetPaletteIndex(position, cube.G0, cube.B1, cube.A1)]
                           + moment[GetPaletteIndex(position, cube.G0, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(position, cube.G0, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(position, cube.G0, cube.B0, cube.A0)];

                // Green
                case 2:
                    return moment[GetPaletteIndex(cube.R1, position, cube.B1, cube.A1)]
                           - moment[GetPaletteIndex(cube.R1, position, cube.B1, cube.A0)]
                           - moment[GetPaletteIndex(cube.R1, position, cube.B0, cube.A1)]
                           + moment[GetPaletteIndex(cube.R1, position, cube.B0, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, position, cube.B1, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, position, cube.B1, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, position, cube.B0, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, position, cube.B0, cube.A0)];

                // Blue
                case 1:
                    return moment[GetPaletteIndex(cube.R1, cube.G1, position, cube.A1)]
                           - moment[GetPaletteIndex(cube.R1, cube.G1, position, cube.A0)]
                           - moment[GetPaletteIndex(cube.R1, cube.G0, position, cube.A1)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, position, cube.A0)]
                           - moment[GetPaletteIndex(cube.R0, cube.G1, position, cube.A1)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, position, cube.A0)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, position, cube.A1)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, position, cube.A0)];

                // Alpha
                case 0:
                    return moment[GetPaletteIndex(cube.R1, cube.G1, cube.B1, position)]
                           - moment[GetPaletteIndex(cube.R1, cube.G1, cube.B0, position)]
                           - moment[GetPaletteIndex(cube.R1, cube.G0, cube.B1, position)]
                           + moment[GetPaletteIndex(cube.R1, cube.G0, cube.B0, position)]
                           - moment[GetPaletteIndex(cube.R0, cube.G1, cube.B1, position)]
                           + moment[GetPaletteIndex(cube.R0, cube.G1, cube.B0, position)]
                           + moment[GetPaletteIndex(cube.R0, cube.G0, cube.B1, position)]
                           - moment[GetPaletteIndex(cube.R0, cube.G0, cube.B0, position)];

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>
        /// Converts the histogram into moments so that we can rapidly calculate the sums of the above quantities over any desired box.
        /// </summary>
        private void Get3DMoments(MemoryManager memoryManager)
        {
            Span<long> vwtSpan = this.vwt.Span;
            Span<long> vmrSpan = this.vmr.Span;
            Span<long> vmgSpan = this.vmg.Span;
            Span<long> vmbSpan = this.vmb.Span;
            Span<long> vmaSpan = this.vma.Span;
            Span<float> m2Span = this.m2.Span;

            using (IBuffer<long> volume = memoryManager.Allocate<long>(IndexCount * IndexAlphaCount))
            using (IBuffer<long> volumeR = memoryManager.Allocate<long>(IndexCount * IndexAlphaCount))
            using (IBuffer<long> volumeG = memoryManager.Allocate<long>(IndexCount * IndexAlphaCount))
            using (IBuffer<long> volumeB = memoryManager.Allocate<long>(IndexCount * IndexAlphaCount))
            using (IBuffer<long> volumeA = memoryManager.Allocate<long>(IndexCount * IndexAlphaCount))
            using (IBuffer<float> volume2 = memoryManager.Allocate<float>(IndexCount * IndexAlphaCount))

            using (IBuffer<long> area = memoryManager.Allocate<long>(IndexAlphaCount))
            using (IBuffer<long> areaR = memoryManager.Allocate<long>(IndexAlphaCount))
            using (IBuffer<long> areaG = memoryManager.Allocate<long>(IndexAlphaCount))
            using (IBuffer<long> areaB = memoryManager.Allocate<long>(IndexAlphaCount))
            using (IBuffer<long> areaA = memoryManager.Allocate<long>(IndexAlphaCount))
            using (IBuffer<float> area2 = memoryManager.Allocate<float>(IndexAlphaCount))
            {
                Span<long> volumeSpan = volume.Span;
                Span<long> volumeRSpan = volumeR.Span;
                Span<long> volumeGSpan = volumeG.Span;
                Span<long> volumeBSpan = volumeB.Span;
                Span<long> volumeASpan = volumeA.Span;
                Span<float> volume2Span = volume2.Span;

                Span<long> areaSpan = area.Span;
                Span<long> areaRSpan = areaR.Span;
                Span<long> areaGSpan = areaG.Span;
                Span<long> areaBSpan = areaB.Span;
                Span<long> areaASpan = areaA.Span;
                Span<float> area2Span = area2.Span;

                for (int r = 1; r < IndexCount; r++)
                {
                    volume.Clear();
                    volumeR.Clear();
                    volumeG.Clear();
                    volumeB.Clear();
                    volumeA.Clear();
                    volume2.Clear();

                    for (int g = 1; g < IndexCount; g++)
                    {
                        area.Clear();
                        areaR.Clear();
                        areaG.Clear();
                        areaB.Clear();
                        areaA.Clear();
                        area2.Clear();

                        for (int b = 1; b < IndexCount; b++)
                        {
                            long line = 0;
                            long lineR = 0;
                            long lineG = 0;
                            long lineB = 0;
                            long lineA = 0;
                            float line2 = 0;

                            for (int a = 1; a < IndexAlphaCount; a++)
                            {
                                int ind1 = GetPaletteIndex(r, g, b, a);

                                line += vwtSpan[ind1];
                                lineR += vmrSpan[ind1];
                                lineG += vmgSpan[ind1];
                                lineB += vmbSpan[ind1];
                                lineA += vmaSpan[ind1];
                                line2 += m2Span[ind1];

                                areaSpan[a] += line;
                                areaRSpan[a] += lineR;
                                areaGSpan[a] += lineG;
                                areaBSpan[a] += lineB;
                                areaASpan[a] += lineA;
                                area2Span[a] += line2;

                                int inv = (b * IndexAlphaCount) + a;

                                volumeSpan[inv] += areaSpan[a];
                                volumeRSpan[inv] += areaRSpan[a];
                                volumeGSpan[inv] += areaGSpan[a];
                                volumeBSpan[inv] += areaBSpan[a];
                                volumeASpan[inv] += areaASpan[a];
                                volume2Span[inv] += area2Span[a];

                                int ind2 = ind1 - GetPaletteIndex(1, 0, 0, 0);

                                vwtSpan[ind1] = vwtSpan[ind2] + volumeSpan[inv];
                                vmrSpan[ind1] = vmrSpan[ind2] + volumeRSpan[inv];
                                vmgSpan[ind1] = vmgSpan[ind2] + volumeGSpan[inv];
                                vmbSpan[ind1] = vmbSpan[ind2] + volumeBSpan[inv];
                                vmaSpan[ind1] = vmaSpan[ind2] + volumeASpan[inv];
                                m2Span[ind1] = m2Span[ind2] + volume2Span[inv];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the weighted variance of a box cube.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <returns>The <see cref="float"/>.</returns>
        private float Variance(ref Box cube)
        {
            float dr = Volume(ref cube, this.vmr.Span);
            float dg = Volume(ref cube, this.vmg.Span);
            float db = Volume(ref cube, this.vmb.Span);
            float da = Volume(ref cube, this.vma.Span);

            Span<float> m2Span = this.m2.Span;

            float xx =
                m2Span[GetPaletteIndex(cube.R1, cube.G1, cube.B1, cube.A1)]
                - m2Span[GetPaletteIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                - m2Span[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                + m2Span[GetPaletteIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                - m2Span[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                + m2Span[GetPaletteIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                + m2Span[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                - m2Span[GetPaletteIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                - m2Span[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                + m2Span[GetPaletteIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                + m2Span[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                - m2Span[GetPaletteIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                + m2Span[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                - m2Span[GetPaletteIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                - m2Span[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                + m2Span[GetPaletteIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

            var vector = new Vector4(dr, dg, db, da);
            return xx - (Vector4.Dot(vector, vector) / Volume(ref cube, this.vwt.Span));
        }

        /// <summary>
        /// We want to minimize the sum of the variances of two sub-boxes.
        /// The sum(c^2) terms can be ignored since their sum over both sub-boxes
        /// is the same (the sum for the whole box) no matter where we split.
        /// The remaining terms have a minus sign in the variance formula,
        /// so we drop the minus sign and maximize the sum of the two terms.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="first">The first position.</param>
        /// <param name="last">The last position.</param>
        /// <param name="cut">The cutting point.</param>
        /// <param name="wholeR">The whole red.</param>
        /// <param name="wholeG">The whole green.</param>
        /// <param name="wholeB">The whole blue.</param>
        /// <param name="wholeA">The whole alpha.</param>
        /// <param name="wholeW">The whole weight.</param>
        /// <returns>The <see cref="float"/>.</returns>
        private float Maximize(ref Box cube, int direction, int first, int last, out int cut, float wholeR, float wholeG, float wholeB, float wholeA, float wholeW)
        {
            long baseR = Bottom(ref cube, direction, this.vmr.Span);
            long baseG = Bottom(ref cube, direction, this.vmg.Span);
            long baseB = Bottom(ref cube, direction, this.vmb.Span);
            long baseA = Bottom(ref cube, direction, this.vma.Span);
            long baseW = Bottom(ref cube, direction, this.vwt.Span);

            float max = 0F;
            cut = -1;

            for (int i = first; i < last; i++)
            {
                float halfR = baseR + Top(ref cube, direction, i, this.vmr.Span);
                float halfG = baseG + Top(ref cube, direction, i, this.vmg.Span);
                float halfB = baseB + Top(ref cube, direction, i, this.vmb.Span);
                float halfA = baseA + Top(ref cube, direction, i, this.vma.Span);
                float halfW = baseW + Top(ref cube, direction, i, this.vwt.Span);

                if (MathF.Abs(halfW) < Constants.Epsilon)
                {
                    continue;
                }

                var vector = new Vector4(halfR, halfG, halfB, halfA);
                float temp = Vector4.Dot(vector, vector) / halfW;

                halfW = wholeW - halfW;

                if (MathF.Abs(halfW) < Constants.Epsilon)
                {
                    continue;
                }

                halfR = wholeR - halfR;
                halfG = wholeG - halfG;
                halfB = wholeB - halfB;
                halfA = wholeA - halfA;

                vector = new Vector4(halfR, halfG, halfB, halfA);

                temp += Vector4.Dot(vector, vector) / halfW;

                if (temp > max)
                {
                    max = temp;
                    cut = i;
                }
            }

            return max;
        }

        /// <summary>
        /// Cuts a box.
        /// </summary>
        /// <param name="set1">The first set.</param>
        /// <param name="set2">The second set.</param>
        /// <returns>Returns a value indicating whether the box has been split.</returns>
        private bool Cut(ref Box set1, ref Box set2)
        {
            float wholeR = Volume(ref set1, this.vmr.Span);
            float wholeG = Volume(ref set1, this.vmg.Span);
            float wholeB = Volume(ref set1, this.vmb.Span);
            float wholeA = Volume(ref set1, this.vma.Span);
            float wholeW = Volume(ref set1, this.vwt.Span);

            float maxr = this.Maximize(ref set1, 3, set1.R0 + 1, set1.R1, out int cutr, wholeR, wholeG, wholeB, wholeA, wholeW);
            float maxg = this.Maximize(ref set1, 2, set1.G0 + 1, set1.G1, out int cutg, wholeR, wholeG, wholeB, wholeA, wholeW);
            float maxb = this.Maximize(ref set1, 1, set1.B0 + 1, set1.B1, out int cutb, wholeR, wholeG, wholeB, wholeA, wholeW);
            float maxa = this.Maximize(ref set1, 0, set1.A0 + 1, set1.A1, out int cuta, wholeR, wholeG, wholeB, wholeA, wholeW);

            int dir;

            if ((maxr >= maxg) && (maxr >= maxb) && (maxr >= maxa))
            {
                dir = 3;

                if (cutr < 0)
                {
                    return false;
                }
            }
            else if ((maxg >= maxr) && (maxg >= maxb) && (maxg >= maxa))
            {
                dir = 2;
            }
            else if ((maxb >= maxr) && (maxb >= maxg) && (maxb >= maxa))
            {
                dir = 1;
            }
            else
            {
                dir = 0;
            }

            set2.R1 = set1.R1;
            set2.G1 = set1.G1;
            set2.B1 = set1.B1;
            set2.A1 = set1.A1;

            switch (dir)
            {
                // Red
                case 3:
                    set2.R0 = set1.R1 = cutr;
                    set2.G0 = set1.G0;
                    set2.B0 = set1.B0;
                    set2.A0 = set1.A0;
                    break;

                // Green
                case 2:
                    set2.G0 = set1.G1 = cutg;
                    set2.R0 = set1.R0;
                    set2.B0 = set1.B0;
                    set2.A0 = set1.A0;
                    break;

                // Blue
                case 1:
                    set2.B0 = set1.B1 = cutb;
                    set2.R0 = set1.R0;
                    set2.G0 = set1.G0;
                    set2.A0 = set1.A0;
                    break;

                // Alpha
                case 0:
                    set2.A0 = set1.A1 = cuta;
                    set2.R0 = set1.R0;
                    set2.G0 = set1.G0;
                    set2.B0 = set1.B0;
                    break;
            }

            set1.Volume = (set1.R1 - set1.R0) * (set1.G1 - set1.G0) * (set1.B1 - set1.B0) * (set1.A1 - set1.A0);
            set2.Volume = (set2.R1 - set2.R0) * (set2.G1 - set2.G0) * (set2.B1 - set2.B0) * (set2.A1 - set2.A0);

            return true;
        }

        /// <summary>
        /// Marks a color space tag.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="label">A label.</param>
        private void Mark(ref Box cube, byte label)
        {
            Span<byte> tagSpan = this.tag.Span;

            for (int r = cube.R0 + 1; r <= cube.R1; r++)
            {
                for (int g = cube.G0 + 1; g <= cube.G1; g++)
                {
                    for (int b = cube.B0 + 1; b <= cube.B1; b++)
                    {
                        for (int a = cube.A0 + 1; a <= cube.A1; a++)
                        {
                            tagSpan[GetPaletteIndex(r, g, b, a)] = label;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the cube.
        /// </summary>
        private void BuildCube()
        {
            this.colorCube = new Box[this.colors];
            float[] vv = new float[this.colors];

            ref var cube = ref this.colorCube[0];
            cube.R0 = cube.G0 = cube.B0 = cube.A0 = 0;
            cube.R1 = cube.G1 = cube.B1 = IndexCount - 1;
            cube.A1 = IndexAlphaCount - 1;

            int next = 0;

            for (int i = 1; i < this.colors; i++)
            {
                ref var nextCube = ref this.colorCube[next];
                ref var currentCube = ref this.colorCube[i];
                if (this.Cut(ref nextCube, ref currentCube))
                {
                    vv[next] = nextCube.Volume > 1 ? this.Variance(ref nextCube) : 0F;
                    vv[i] = currentCube.Volume > 1 ? this.Variance(ref currentCube) : 0F;
                }
                else
                {
                    vv[next] = 0F;
                    i--;
                }

                next = 0;

                float temp = vv[0];
                for (int k = 1; k <= i; k++)
                {
                    if (vv[k] > temp)
                    {
                        temp = vv[k];
                        next = k;
                    }
                }

                if (temp <= 0F)
                {
                    this.colors = i + 1;
                    break;
                }
            }
        }

        /// <summary>
        /// Process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>
        /// The quantized value
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte QuantizePixel(TPixel pixel)
        {
            if (this.Dither)
            {
                // The colors have changed so we need to use Euclidean distance calculation to find the closest value.
                // This palette can never be null here.
                return this.GetClosestPixel(pixel, this.palette, this.colorMap);
            }

            // Expected order r->g->b->a
            var rgba = default(Rgba32);
            pixel.ToRgba32(ref rgba);

            int r = rgba.R >> (8 - IndexBits);
            int g = rgba.G >> (8 - IndexBits);
            int b = rgba.B >> (8 - IndexBits);
            int a = rgba.A >> (8 - IndexAlphaBits);

            Span<byte> tagSpan = this.tag.Span;

            return tagSpan[GetPaletteIndex(r + 1, g + 1, b + 1, a + 1)];
        }

        /// <summary>
        /// Represents a box color cube.
        /// </summary>
        private struct Box
        {
            /// <summary>
            /// Gets or sets the min red value, exclusive.
            /// </summary>
            public int R0;

            /// <summary>
            /// Gets or sets the max red value, inclusive.
            /// </summary>
            public int R1;

            /// <summary>
            /// Gets or sets the min green value, exclusive.
            /// </summary>
            public int G0;

            /// <summary>
            /// Gets or sets the max green value, inclusive.
            /// </summary>
            public int G1;

            /// <summary>
            /// Gets or sets the min blue value, exclusive.
            /// </summary>
            public int B0;

            /// <summary>
            /// Gets or sets the max blue value, inclusive.
            /// </summary>
            public int B1;

            /// <summary>
            /// Gets or sets the min alpha value, exclusive.
            /// </summary>
            public int A0;

            /// <summary>
            /// Gets or sets the max alpha value, inclusive.
            /// </summary>
            public int A1;

            /// <summary>
            /// Gets or sets the volume.
            /// </summary>
            public int Volume;
        }
    }
}