﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace SixLabors.ImageSharp.Advanced
{
    /// <summary>
    /// Unlike traditional Mono/.NET, code on the iPhone is statically compiled ahead of time instead of being
    /// compiled on demand by a JIT compiler. This means there are a few limitations with respect to generics,
    /// these are caused because not every possible generic instantiation can be determined up front at compile time.
    /// The Aot Compiler is designed to overcome the limitations of this compiler.
    /// </summary>
    public static class AotCompilerTools
    {
        /// <summary>
        /// Seeds the compiler using the given pixel format.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        public static void Seed<TPixel>()
             where TPixel : struct, IPixel<TPixel>
        {
            // This is we actually call all the individual methods you need to seed.
            AotCompileOctreeQuantizer<TPixel>();
            AotCompileWuQuantizer<TPixel>();
            AotCompileDithering<TPixel>();

            // TODO: Do the discovery work to figure out what works and what doesn't.
        }

        /// <summary>
        /// Seeds the compiler using the given pixel formats.
        /// </summary>
        /// <typeparam name="TPixel">The first pixel format.</typeparam>
        /// <typeparam name="TPixel2">The second pixel format.</typeparam>
        public static void Seed<TPixel, TPixel2>()
             where TPixel : struct, IPixel<TPixel>
             where TPixel2 : struct, IPixel<TPixel2>
        {
            Seed<TPixel>();
            Seed<TPixel2>();
        }

        /// <summary>
        /// Seeds the compiler using the given pixel formats.
        /// </summary>
        /// <typeparam name="TPixel">The first pixel format.</typeparam>
        /// <typeparam name="TPixel2">The second pixel format.</typeparam>
        /// <typeparam name="TPixel3">The third pixel format.</typeparam>
        public static void Seed<TPixel, TPixel2, TPixel3>()
             where TPixel : struct, IPixel<TPixel>
             where TPixel2 : struct, IPixel<TPixel2>
             where TPixel3 : struct, IPixel<TPixel3>
        {
            Seed<TPixel, TPixel2>();
            Seed<TPixel3>();
        }

        /// <summary>
        /// This method doesn't actually do anything but serves an important purpose...
        /// If you are running ImageSharp on iOS and try to call SaveAsGif, it will throw an excepion:
        /// "Attempting to JIT compile method... OctreeFrameQuantizer.ConstructPalette... while running in aot-only mode."
        /// The reason this happens is the SaveAsGif method makes haevy use of generics, which are too confusing for the AoT
        /// compiler used on Xamarin.iOS. It spins up the JIT compiler to try and figure it out, but that is an illegal op on
        /// iOS so it bombs out.
        /// If you are getting the above error, you need to call this method, which will pre-seed the AoT compiler with the
        /// necessary methods to complete the SaveAsGif call. That's it, otherwise you should NEVER need this method!!!
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        private static void AotCompileOctreeQuantizer<TPixel>()
            where TPixel : struct, IPixel<TPixel>
        {
            var test = new OctreeFrameQuantizer<TPixel>(new OctreeQuantizer(false));
            test.AotGetPalette();
        }

        /// <summary>
        /// This method pre-seeds the WuQuantizer in the AoT compiler for iOS.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        private static void AotCompileWuQuantizer<TPixel>()
            where TPixel : struct, IPixel<TPixel>
        {
            var test = new WuFrameQuantizer<TPixel>(new WuQuantizer(false));
            test.QuantizeFrame(new ImageFrame<TPixel>(Configuration.Default, 1, 1));
            test.AotGetPalette();
        }

        /// <summary>
        /// This method pre-seeds the default dithering engine (FloydSteinbergDiffuser) in the AoT compiler for iOS.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        private static void AotCompileDithering<TPixel>()
            where TPixel : struct, IPixel<TPixel>
        {
            var test = new FloydSteinbergDiffuser();
            TPixel pixel = default;
            test.Dither<TPixel>(new ImageFrame<TPixel>(Configuration.Default, 1, 1), pixel, pixel, 0, 0, 0, 0, 0, 0);
        }
    }
}