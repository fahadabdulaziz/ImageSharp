﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SixLabors.ImageSharp.Formats.Jpeg.Components;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Formats.Jpeg.GolangPort.Components.Decoder
{
    /// <summary>
    /// Encapsulates the impementation of Jpeg SOS Huffman decoding. See JpegScanDecoder.md!
    ///
    /// <see cref="zigStart"/> and <see cref="zigEnd"/> are the spectral selection bounds.
    /// <see cref="ah"/> and <see cref="al"/> are the successive approximation high and low values.
    /// The spec calls these values Ss, Se, Ah and Al.
    /// For progressive JPEGs, these are the two more-or-less independent
    /// aspects of progression. Spectral selection progression is when not
    /// all of a block's 64 DCT coefficients are transmitted in one pass.
    /// For example, three passes could transmit coefficient 0 (the DC
    /// component), coefficients 1-5, and coefficients 6-63, in zig-zag
    /// order. Successive approximation is when not all of the bits of a
    /// band of coefficients are transmitted in one pass. For example,
    /// three passes could transmit the 6 most significant bits, followed
    /// by the second-least significant bit, followed by the least
    /// significant bit.
    /// For baseline JPEGs, these parameters are hard-coded to 0/63/0/0.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe partial struct GolangJpegScanDecoder
    {
        // The JpegScanDecoder members should be ordered in a way that results in optimal memory layout.
#pragma warning disable SA1202 // ElementsMustBeOrderedByAccess

        /// <summary>
        /// The <see cref="ComputationData"/> buffer
        /// </summary>
        private ComputationData data;

        /// <summary>
        /// Pointers to elements of <see cref="data"/>
        /// </summary>
        private DataPointers pointers;

        /// <summary>
        /// The current component index
        /// </summary>
        public int ComponentIndex;

        /// <summary>
        /// X coordinate of the current block, in units of 8x8. (The third block in the first row has (bx, by) = (2, 0))
        /// </summary>
        private int bx;

        /// <summary>
        /// Y coordinate of the current block, in units of 8x8. (The third block in the first row has (bx, by) = (2, 0))
        /// </summary>
        private int by;

        /// <summary>
        /// Start index of the zig-zag selection bound
        /// </summary>
        private int zigStart;

        /// <summary>
        /// End index of the zig-zag selection bound
        /// </summary>
        private int zigEnd;

        /// <summary>
        /// Successive approximation high value
        /// </summary>
        private int ah;

        /// <summary>
        /// Successive approximation low value
        /// </summary>
        private int al;

        /// <summary>
        /// The number of component scans
        /// </summary>
        private int componentScanCount;

        /// <summary>
        /// Horizontal sampling factor at the current component index
        /// </summary>
        private int hi;

        /// <summary>
        /// End-of-Band run, specified in section G.1.2.2.
        /// </summary>
        private int eobRun;

        /// <summary>
        /// The block counter
        /// </summary>
        private int blockCounter;

        /// <summary>
        /// The MCU counter
        /// </summary>
        private int mcuCounter;

        /// <summary>
        /// The expected RST marker value
        /// </summary>
        private byte expectedRst;

        /// <summary>
        /// Initializes a default-constructed <see cref="GolangJpegScanDecoder"/> instance for reading data from <see cref="GolangJpegDecoderCore"/>-s stream.
        /// </summary>
        /// <param name="p">Pointer to <see cref="GolangJpegScanDecoder"/> on the stack</param>
        /// <param name="decoder">The <see cref="GolangJpegDecoderCore"/> instance</param>
        /// <param name="remaining">The remaining bytes in the segment block.</param>
        public static void InitStreamReading(GolangJpegScanDecoder* p, GolangJpegDecoderCore decoder, int remaining)
        {
            p->data = ComputationData.Create();
            p->pointers = new DataPointers(&p->data);
            p->InitStreamReadingImpl(decoder, remaining);
        }

        /// <summary>
        /// Read Huffman data from Jpeg scans in <see cref="GolangJpegDecoderCore.InputStream"/>,
        /// and decode it as <see cref="Block8x8"/> into <see cref="GolangComponent.SpectralBlocks"/>.
        ///
        /// The blocks are traversed one MCU at a time. For 4:2:0 chroma
        /// subsampling, there are four Y 8x8 blocks in every 16x16 MCU.
        /// For a baseline 32x16 pixel image, the Y blocks visiting order is:
        /// 0 1 4 5
        /// 2 3 6 7
        /// For progressive images, the interleaved scans (those with component count &gt; 1)
        /// are traversed as above, but non-interleaved scans are traversed left
        /// to right, top to bottom:
        /// 0 1 2 3
        /// 4 5 6 7
        /// Only DC scans (zigStart == 0) can be interleave AC scans must have
        /// only one component.
        /// To further complicate matters, for non-interleaved scans, there is no
        /// data for any blocks that are inside the image at the MCU level but
        /// outside the image at the pixel level. For example, a 24x16 pixel 4:2:0
        /// progressive image consists of two 16x16 MCUs. The interleaved scans
        /// will process 8 Y blocks:
        /// 0 1 4 5
        /// 2 3 6 7
        /// The non-interleaved scans will process only 6 Y blocks:
        /// 0 1 2
        /// 3 4 5
        /// </summary>
        /// <param name="decoder">The <see cref="GolangJpegDecoderCore"/> instance</param>
        public void DecodeBlocks(GolangJpegDecoderCore decoder)
        {
            decoder.InputProcessor.ResetErrorState();

            this.blockCounter = 0;
            this.mcuCounter = 0;
            this.expectedRst = JpegConstants.Markers.RST0;

            for (int my = 0; my < decoder.MCUCountY; my++)
            {
                for (int mx = 0; mx < decoder.MCUCountX; mx++)
                {
                    this.DecodeBlocksAtMcuIndex(decoder, mx, my);

                    this.mcuCounter++;

                    // Handling restart intervals
                    // Useful info: https://stackoverflow.com/a/8751802
                    if (decoder.IsAtRestartInterval(this.mcuCounter))
                    {
                        this.ProcessRSTMarker(decoder);
                        this.Reset(decoder);
                    }
                }
            }
        }

        private void DecodeBlocksAtMcuIndex(GolangJpegDecoderCore decoder, int mx, int my)
        {
            for (int scanIndex = 0; scanIndex < this.componentScanCount; scanIndex++)
            {
                this.ComponentIndex = this.pointers.ComponentScan[scanIndex].ComponentIndex;
                GolangComponent component = decoder.Components[this.ComponentIndex];

                this.hi = component.HorizontalSamplingFactor;
                int vi = component.VerticalSamplingFactor;

                for (int j = 0; j < this.hi * vi; j++)
                {
                    if (this.componentScanCount != 1)
                    {
                        this.bx = (this.hi * mx) + (j % this.hi);
                        this.by = (vi * my) + (j / this.hi);
                    }
                    else
                    {
                        int q = decoder.MCUCountX * this.hi;
                        this.bx = this.blockCounter % q;
                        this.by = this.blockCounter / q;
                        this.blockCounter++;
                        if (this.bx * 8 >= decoder.ImageWidth || this.by * 8 >= decoder.ImageHeight)
                        {
                            continue;
                        }
                    }

                    // Find the block at (bx,by) in the component's buffer:
                    ref Block8x8 blockRefOnHeap = ref component.GetBlockReference(this.bx, this.by);

                    // Copy block to stack
                    this.data.Block = blockRefOnHeap;

                    if (!decoder.InputProcessor.ReachedEOF)
                    {
                        this.DecodeBlock(decoder, scanIndex);
                    }

                    // Store the result block:
                    blockRefOnHeap = this.data.Block;
                }
            }
        }

        private void ProcessRSTMarker(GolangJpegDecoderCore decoder)
        {
            // Attempt to look for RST[0-7] markers to resynchronize from corrupt input.
            if (!decoder.InputProcessor.ReachedEOF)
            {
                decoder.InputProcessor.ReadFullUnsafe(decoder.Temp, 0, 2);
                if (decoder.InputProcessor.CheckEOFEnsureNoError())
                {
                    if (decoder.Temp[0] != 0xFF || decoder.Temp[1] != this.expectedRst)
                    {
                        bool invalidRst = true;

                        // Most jpeg's containing well-formed input will have a RST[0-7] marker following immediately
                        // but some, see Issue #481, contain padding bytes "0xFF" before the RST[0-7] marker.
                        // If we identify that case we attempt to read until we have bypassed the padded bytes.
                        // We then check again for our RST marker and throw if invalid.
                        // No other methods are attempted to resynchronize from corrupt input.
                        if (decoder.Temp[0] == 0xFF && decoder.Temp[1] == 0xFF)
                        {
                            while (decoder.Temp[0] == 0xFF && decoder.InputProcessor.CheckEOFEnsureNoError())
                            {
                                decoder.InputProcessor.ReadFullUnsafe(decoder.Temp, 0, 1);
                                if (!decoder.InputProcessor.CheckEOFEnsureNoError())
                                {
                                    break;
                                }
                            }

                            // Have we found a valid restart marker?
                            invalidRst = decoder.Temp[0] != this.expectedRst;
                        }

                        if (invalidRst)
                        {
                            throw new ImageFormatException("Bad RST marker");
                        }
                    }

                    this.expectedRst++;
                    if (this.expectedRst == JpegConstants.Markers.RST7 + 1)
                    {
                        this.expectedRst = JpegConstants.Markers.RST0;
                    }
                }
            }
        }

        private void Reset(GolangJpegDecoderCore decoder)
        {
            decoder.InputProcessor.ResetHuffmanDecoder();

            this.ResetDcValues();

            // Reset the progressive decoder state, as per section G.1.2.2.
            this.eobRun = 0;
        }

        /// <summary>
        /// Reset the DC components, as per section F.2.1.3.1.
        /// </summary>
        private void ResetDcValues()
        {
            Unsafe.InitBlock(this.pointers.Dc, default, sizeof(int) * GolangJpegDecoderCore.MaxComponents);
        }

        /// <summary>
        /// The implementation part of <see cref="InitStreamReading"/> as an instance method.
        /// </summary>
        /// <param name="decoder">The <see cref="GolangJpegDecoderCore"/></param>
        /// <param name="remaining">The remaining bytes</param>
        private void InitStreamReadingImpl(GolangJpegDecoderCore decoder, int remaining)
        {
            if (decoder.ComponentCount == 0)
            {
                throw new ImageFormatException("Missing SOF marker");
            }

            if (remaining < 6 || 4 + (2 * decoder.ComponentCount) < remaining || remaining % 2 != 0)
            {
                throw new ImageFormatException("SOS has wrong length");
            }

            decoder.InputProcessor.ReadFull(decoder.Temp, 0, remaining);
            this.componentScanCount = decoder.Temp[0];

            int scanComponentCountX2 = 2 * this.componentScanCount;
            if (remaining != 4 + scanComponentCountX2)
            {
                throw new ImageFormatException("SOS length inconsistent with number of components");
            }

            int totalHv = 0;

            for (int i = 0; i < this.componentScanCount; i++)
            {
                this.InitComponentScan(decoder, i, ref this.pointers.ComponentScan[i], ref totalHv);
            }

            // Section B.2.3 states that if there is more than one component then the
            // total H*V values in a scan must be <= 10.
            if (decoder.ComponentCount > 1 && totalHv > 10)
            {
                throw new ImageFormatException("Total sampling factors too large.");
            }

            this.zigEnd = Block8x8F.Size - 1;

            if (decoder.IsProgressive)
            {
                this.zigStart = decoder.Temp[1 + scanComponentCountX2];
                this.zigEnd = decoder.Temp[2 + scanComponentCountX2];
                this.ah = decoder.Temp[3 + scanComponentCountX2] >> 4;
                this.al = decoder.Temp[3 + scanComponentCountX2] & 0x0f;

                if ((this.zigStart == 0 && this.zigEnd != 0) || this.zigStart > this.zigEnd
                    || this.zigEnd >= Block8x8F.Size)
                {
                    throw new ImageFormatException("Bad spectral selection bounds");
                }

                if (this.zigStart != 0 && this.componentScanCount != 1)
                {
                    throw new ImageFormatException("Progressive AC coefficients for more than one component");
                }

                if (this.ah != 0 && this.ah != this.al + 1)
                {
                    throw new ImageFormatException("Bad successive approximation values");
                }
            }
        }

        /// <summary>
        /// Read the current the current block at (<see cref="bx"/>, <see cref="by"/>) from the decoders stream
        /// </summary>
        /// <param name="decoder">The decoder</param>
        /// <param name="scanIndex">The index of the scan</param>
        private void DecodeBlock(GolangJpegDecoderCore decoder, int scanIndex)
        {
            Block8x8* b = this.pointers.Block;
            int huffmannIdx = (GolangHuffmanTree.AcTableIndex * GolangHuffmanTree.ThRowSize) + this.pointers.ComponentScan[scanIndex].AcTableSelector;
            if (this.ah != 0)
            {
                this.Refine(ref decoder.InputProcessor, ref decoder.HuffmanTrees[huffmannIdx], 1 << this.al);
            }
            else
            {
                int zig = this.zigStart;

                if (zig == 0)
                {
                    zig++;

                    // Decode the DC coefficient, as specified in section F.2.2.1.
                    int huffmanIndex = (GolangHuffmanTree.DcTableIndex * GolangHuffmanTree.ThRowSize) + this.pointers.ComponentScan[scanIndex].DcTableSelector;
                    decoder.InputProcessor.DecodeHuffmanUnsafe(
                            ref decoder.HuffmanTrees[huffmanIndex],
                            out int value);
                    if (!decoder.InputProcessor.CheckEOF())
                    {
                        return;
                    }

                    if (value > 16)
                    {
                        throw new ImageFormatException("Excessive DC component");
                    }

                    decoder.InputProcessor.ReceiveExtendUnsafe(value, out int deltaDC);
                    if (!decoder.InputProcessor.CheckEOFEnsureNoError())
                    {
                        return;
                    }

                    this.pointers.Dc[this.ComponentIndex] += deltaDC;

                    // b[0] = dc[compIndex] << al;
                    value = this.pointers.Dc[this.ComponentIndex] << this.al;
                    Block8x8.SetScalarAt(b, 0, (short)value);
                }

                if (zig <= this.zigEnd && this.eobRun > 0)
                {
                    this.eobRun--;
                }
                else
                {
                    // Decode the AC coefficients, as specified in section F.2.2.2.
                    for (; zig <= this.zigEnd; zig++)
                    {
                        decoder.InputProcessor.DecodeHuffmanUnsafe(ref decoder.HuffmanTrees[huffmannIdx], out int value);
                        if (decoder.InputProcessor.HasError)
                        {
                            return;
                        }

                        int val0 = value >> 4;
                        int val1 = value & 0x0f;
                        if (val1 != 0)
                        {
                            zig += val0;
                            if (zig > this.zigEnd)
                            {
                                break;
                            }

                            decoder.InputProcessor.ReceiveExtendUnsafe(val1, out int ac);
                            if (decoder.InputProcessor.HasError)
                            {
                                return;
                            }

                            // b[Unzig[zig]] = ac << al;
                            value = ac << this.al;
                            Block8x8.SetScalarAt(b, this.pointers.Unzig[zig], (short)value);
                        }
                        else
                        {
                            if (val0 != 0x0f)
                            {
                                this.eobRun = (ushort)(1 << val0);
                                if (val0 != 0)
                                {
                                    this.DecodeEobRun(val0, ref decoder.InputProcessor);
                                    if (!decoder.InputProcessor.CheckEOFEnsureNoError())
                                    {
                                        return;
                                    }
                                }

                                this.eobRun--;
                                break;
                            }

                            zig += 0x0f;
                        }
                    }
                }
            }
        }

        private void DecodeEobRun(int count, ref InputProcessor processor)
        {
            processor.DecodeBitsUnsafe(count, out int bitsResult);
            if (processor.LastErrorCode != GolangDecoderErrorCode.NoError)
            {
                return;
            }

            this.eobRun |= bitsResult;
        }

        private void InitComponentScan(GolangJpegDecoderCore decoder, int i, ref GolangComponentScan currentComponentScan, ref int totalHv)
        {
            // Component selector.
            int cs = decoder.Temp[1 + (2 * i)];
            int compIndex = -1;
            for (int j = 0; j < decoder.ComponentCount; j++)
            {
                // Component compv = ;
                if (cs == decoder.Components[j].Identifier)
                {
                    compIndex = j;
                }
            }

            if (compIndex < 0)
            {
                throw new ImageFormatException("Unknown component selector");
            }

            currentComponentScan.ComponentIndex = (byte)compIndex;

            this.ProcessComponentImpl(decoder, i, ref currentComponentScan, ref totalHv, decoder.Components[compIndex]);
        }

        private void ProcessComponentImpl(
            GolangJpegDecoderCore decoder,
            int i,
            ref GolangComponentScan currentComponentScan,
            ref int totalHv,
            GolangComponent currentComponent)
        {
            // Section B.2.3 states that "the value of Cs_j shall be different from
            // the values of Cs_1 through Cs_(j-1)". Since we have previously
            // verified that a frame's component identifiers (C_i values in section
            // B.2.2) are unique, it suffices to check that the implicit indexes
            // into comp are unique.
            for (int j = 0; j < i; j++)
            {
                if (currentComponentScan.ComponentIndex == this.pointers.ComponentScan[j].ComponentIndex)
                {
                    throw new ImageFormatException("Repeated component selector");
                }
            }

            totalHv += currentComponent.HorizontalSamplingFactor * currentComponent.VerticalSamplingFactor;

            currentComponentScan.DcTableSelector = (byte)(decoder.Temp[2 + (2 * i)] >> 4);
            if (currentComponentScan.DcTableSelector > GolangHuffmanTree.MaxTh)
            {
                throw new ImageFormatException("Bad DC table selector value");
            }

            currentComponentScan.AcTableSelector = (byte)(decoder.Temp[2 + (2 * i)] & 0x0f);
            if (currentComponentScan.AcTableSelector > GolangHuffmanTree.MaxTh)
            {
                throw new ImageFormatException("Bad AC table selector  value");
            }
        }

        /// <summary>
        /// Decodes a successive approximation refinement block, as specified in section G.1.2.
        /// </summary>
        /// <param name="bp">The <see cref="InputProcessor"/> instance</param>
        /// <param name="h">The Huffman tree</param>
        /// <param name="delta">The low transform offset</param>
        private void Refine(ref InputProcessor bp, ref GolangHuffmanTree h, int delta)
        {
            Block8x8* b = this.pointers.Block;

            // Refining a DC component is trivial.
            if (this.zigStart == 0)
            {
                if (this.zigEnd != 0)
                {
                    throw new ImageFormatException("Invalid state for zig DC component");
                }

                bp.DecodeBitUnsafe(out bool bit);
                if (!bp.CheckEOFEnsureNoError())
                {
                    return;
                }

                if (bit)
                {
                    int stuff = Block8x8.GetScalarAt(b, 0);

                    // int stuff = (int)b[0];
                    stuff |= delta;

                    // b[0] = stuff;
                    Block8x8.SetScalarAt(b, 0, (short)stuff);
                }

                return;
            }

            // Refining AC components is more complicated; see sections G.1.2.2 and G.1.2.3.
            int zig = this.zigStart;
            if (this.eobRun == 0)
            {
                for (; zig <= this.zigEnd; zig++)
                {
                    bool done = false;
                    int z = 0;

                    bp.DecodeHuffmanUnsafe(ref h, out int val);
                    if (!bp.CheckEOF())
                    {
                        return;
                    }

                    int val0 = val >> 4;
                    int val1 = val & 0x0f;

                    switch (val1)
                    {
                        case 0:
                            if (val0 != 0x0f)
                            {
                                this.eobRun = 1 << val0;
                                if (val0 != 0)
                                {
                                    this.DecodeEobRun(val0, ref bp);
                                    if (!bp.CheckEOFEnsureNoError())
                                    {
                                        return;
                                    }
                                }

                                done = true;
                            }

                            break;
                        case 1:
                            z = delta;

                            bp.DecodeBitUnsafe(out bool bit);
                            if (!bp.CheckEOFEnsureNoError())
                            {
                                return;
                            }

                            if (!bit)
                            {
                                z = -z;
                            }

                            break;
                        default:
                            throw new ImageFormatException("Unexpected Huffman code");
                    }

                    if (done)
                    {
                        break;
                    }

                    zig = this.RefineNonZeroes(ref bp, zig, val0, delta);

                    if (bp.ReachedEOF || bp.HasError)
                    {
                        return;
                    }

                    if (z != 0 && zig <= this.zigEnd)
                    {
                        // b[Unzig[zig]] = z;
                        Block8x8.SetScalarAt(b, this.pointers.Unzig[zig], (short)z);
                    }
                }
            }

            if (this.eobRun > 0)
            {
                this.eobRun--;
                this.RefineNonZeroes(ref bp, zig, -1, delta);
            }
        }

        /// <summary>
        /// Refines non-zero entries of b in zig-zag order.
        /// If <paramref name="nz" /> >= 0, the first <paramref name="nz" /> zero entries are skipped over.
        /// </summary>
        /// <param name="bp">The <see cref="InputProcessor"/></param>
        /// <param name="zig">The zig-zag start index</param>
        /// <param name="nz">The non-zero entry</param>
        /// <param name="delta">The low transform offset</param>
        /// <returns>The <see cref="int" /></returns>
        private int RefineNonZeroes(ref InputProcessor bp, int zig, int nz, int delta)
        {
            Block8x8* b = this.pointers.Block;
            for (; zig <= this.zigEnd; zig++)
            {
                int u = this.pointers.Unzig[zig];
                int bu = Block8x8.GetScalarAt(b, u);

                // TODO: Are the equality comparsions OK with floating point values? Isn't an epsilon value necessary?
                if (bu == 0)
                {
                    if (nz == 0)
                    {
                        break;
                    }

                    nz--;
                    continue;
                }

                bp.DecodeBitUnsafe(out bool bit);
                if (bp.HasError)
                {
                    return int.MinValue;
                }

                if (!bit)
                {
                    continue;
                }

                int val = bu >= 0 ? bu + delta : bu - delta;

                Block8x8.SetScalarAt(b, u, (short)val);
            }

            return zig;
        }
    }
}