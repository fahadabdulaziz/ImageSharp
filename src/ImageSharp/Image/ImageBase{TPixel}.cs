﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// The base class of all images. Encapsulates the basic properties and methods required to manipulate
    /// images in different pixel formats.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    public abstract class ImageBase<TPixel> : IImageBase, IDisposable, IPixelSource<TPixel>
        where TPixel : struct, IPixel<TPixel>
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// The image pixels. Not private as Buffer2D requires an array in its constructor.
        /// </summary>
        internal TPixel[] PixelBuffer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose() method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBase{TPixel}"/> class.
        /// </summary>
        /// <param name="configuration">
        /// The configuration providing initialization code which allows extending the library.
        /// </param>
        protected ImageBase(Configuration configuration)
        {
            this.Configuration = configuration ?? Configuration.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBase{TPixel}"/> class.
        /// </summary>
        /// <param name="configuration">
        /// The configuration providing initialization code which allows extending the library.
        /// </param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if either <paramref name="width"/> or <paramref name="height"/> are less than or equal to 0.
        /// </exception>
        protected ImageBase(Configuration configuration, int width, int height)
            : this(configuration)
        {
            Guard.MustBeGreaterThan(width, 0, nameof(width));
            Guard.MustBeGreaterThan(height, 0, nameof(height));

            this.Width = width;
            this.Height = height;
            this.RentPixels();
            this.ClearPixels();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBase{TPixel}"/> class.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="ImageBase{TPixel}"/> to create this instance from.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the given <see cref="ImageBase{TPixel}"/> is null.
        /// </exception>
        protected ImageBase(ImageBase<TPixel> other)
            : this(other.Configuration)
        {
            Guard.NotNull(other, nameof(other), "Other image cannot be null.");

            this.Width = other.Width;
            this.Height = other.Height;
            this.CopyProperties(other);

            // Rent then copy the pixels. Unsafe.CopyBlock gives us a nice speed boost here.
            this.RentPixels();

            other.GetPixelSpan().CopyTo(this.GetPixelSpan());
        }

        /// <inheritdoc/>
        Span<TPixel> IPixelSource<TPixel>.Span => new Span<TPixel>(this.PixelBuffer, 0, this.Width * this.Height);

        /// <inheritdoc/>
        public int Width { get; private set; }

        /// <inheritdoc/>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the configuration providing initialization code which allows extending the library.
        /// </summary>
        public Configuration Configuration { get; private set; }

        /// <summary>
        /// Gets or sets the pixel at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel. Must be greater than or equal to zero and less than the width of the image.</param>
        /// <param name="y">The y-coordinate of the pixel. Must be greater than or equal to zero and less than the height of the image.</param>
        /// <returns>The <see typeparam="TPixel"/> at the specified position.</returns>
        public TPixel this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                this.CheckCoordinates(x, y);
                return this.PixelBuffer[(y * this.Width) + x];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this.CheckCoordinates(x, y);
                this.PixelBuffer[(y * this.Width) + x] = value;
            }
        }

        /// <summary>
        /// Gets a reference to the pixel at the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel. Must be greater than or equal to zero and less than the width of the image.</param>
        /// <param name="y">The y-coordinate of the pixel. Must be greater than or equal to zero and less than the height of the image.</param>
        /// <returns>The <see typeparam="TPixel"/> at the specified position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TPixel GetPixelReference(int x, int y)
        {
            this.CheckCoordinates(x, y);
            return ref this.PixelBuffer[(y * this.Width) + x];
        }

        /// <summary>
        /// Clones the image
        /// </summary>
        /// <returns>A new items which is a clone of the original.</returns>
        public ImageBase<TPixel> Clone()
        {
            return this.CloneImageBase();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns image as byte array
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] GetBytes()
        {
            return this.Pixels.AsBytes().ToArray();
        }

        /// <summary>
        /// Locks the image providing access to the pixels.
        /// <remarks>
        /// It is imperative that the accessor is correctly disposed off after use.
        /// </remarks>
        /// </summary>
        /// <returns>The <see cref="PixelAccessor{TPixel}"/></returns>
        internal PixelAccessor<TPixel> Lock()
        {
            return new PixelAccessor<TPixel>(this);
        }

        /// <summary>
        /// Copies the pixels to another <see cref="PixelAccessor{TPixel}"/> of the same size.
        /// </summary>
        /// <param name="target">The target pixel buffer accessor.</param>
        internal void CopyTo(PixelAccessor<TPixel> target)
        {
            SpanHelper.Copy(this.GetPixelSpan(), target.PixelBuffer.Span);
        }

        /// <summary>
        /// Switches the buffers used by the image and the PixelAccessor meaning that the Image will "own" the buffer from the PixelAccessor and the PixelAccessor will now own the Images buffer.
        /// </summary>
        /// <param name="pixelSource">The pixel source.</param>
        internal void SwapPixelsBuffers(PixelAccessor<TPixel> pixelSource)
        {
            Guard.NotNull(pixelSource, nameof(pixelSource));

            int newWidth = pixelSource.Width;
            int newHeight = pixelSource.Height;

            // Push my memory into the accessor (which in turn unpins the old buffer ready for the images use)
            TPixel[] newPixels = pixelSource.ReturnCurrentColorsAndReplaceThemInternally(this.Width, this.Height, this.PixelBuffer);
            this.Width = newWidth;
            this.Height = newHeight;
            this.PixelBuffer = newPixels;
        }

        /// <summary>
        /// Switches the buffers used by the image and the pixelSource meaning that the Image will "own" the buffer from the pixelSource and the pixelSource will now own the Images buffer.
        /// </summary>
        /// <param name="pixelSource">The pixel source.</param>
        internal void SwapPixelsData(ImageBase<TPixel> pixelSource)
        {
            Guard.NotNull(pixelSource, nameof(pixelSource));

            int newWidth = pixelSource.Width;
            int newHeight = pixelSource.Height;
            TPixel[] newPixels = pixelSource.PixelBuffer;

            pixelSource.PixelBuffer = this.PixelBuffer;
            pixelSource.Width = this.Width;
            pixelSource.Height = this.Height;

            this.Width = newWidth;
            this.Height = newHeight;
            this.PixelBuffer = newPixels;
        }

        /// <summary>
        /// Clones the image
        /// </summary>
        /// <returns>A new items which is a clone of the original.</returns>
        protected abstract ImageBase<TPixel> CloneImageBase();

        /// <summary>
        /// Copies the properties from the other <see cref="IImageBase"/>.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="IImageBase"/> to copy the properties from.
        /// </param>
        protected void CopyProperties(IImageBase other)
        {
            DebugGuard.NotNull(other, nameof(other));

            this.Configuration = other.Configuration;
        }

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.ReturnPixels();
            }

            // Note disposing is done.
            this.isDisposed = true;
        }

        /// <summary>
        /// Rents the pixel array from the pool.
        /// </summary>
        private void RentPixels()
        {
            this.PixelBuffer = PixelDataPool<TPixel>.Rent(this.Width * this.Height);
        }

        /// <summary>
        /// Returns the rented pixel array back to the pool.
        /// </summary>
        private void ReturnPixels()
        {
            PixelDataPool<TPixel>.Return(this.PixelBuffer);
            this.PixelBuffer = null;
        }

        /// <summary>
        /// Clears the pixel array.
        /// </summary>
        private void ClearPixels()
        {
            Array.Clear(this.PixelBuffer, 0, this.Width * this.Height);
        }

        /// <summary>
        /// Checks the coordinates to ensure they are within bounds.
        /// </summary>
        /// <param name="y">The y-coordinate of the pixel. Must be greater than zero and less than the height of the image.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the coordinates are not within the bounds of the image.
        /// </exception>
        [Conditional("DEBUG")]
        private void CheckCoordinates(int y)
        {
            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, $"{y} is outwith the image bounds.");
            }
        }

        /// <summary>
        /// Checks the coordinates to ensure they are within bounds.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel. Must be greater than zero and less than the width of the image.</param>
        /// <param name="y">The y-coordinate of the pixel. Must be greater than zero and less than the height of the image.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the coordinates are not within the bounds of the image.
        /// </exception>
        [Conditional("DEBUG")]
        private void CheckCoordinates(int x, int y)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, $"{x} is outwith the image bounds.");
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, $"{y} is outwith the image bounds.");
            }
        }
    }
}