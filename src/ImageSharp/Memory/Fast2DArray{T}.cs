// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Memory
{
    /// <summary>
    /// Provides fast access to 2D arrays.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    internal struct Fast2DArray<T>
    {
        /// <summary>
        /// The 1D representation of the 2D array.
        /// </summary>
        public T[] Data;

        /// <summary>
        /// Gets the width of the 2D array.
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets the height of the 2D array.
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets the number of items in the 2D array
        /// </summary>
        public int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fast2DArray{T}" /> struct.
        /// </summary>
        /// <param name="length">The length of each dimension.</param>
        public Fast2DArray(int length)
            : this(length, length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fast2DArray{T}" /> struct.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Fast2DArray(int width, int height)
        {
            this.Height = height;
            this.Width = width;

            Guard.MustBeGreaterThan(width, 0, nameof(width));
            Guard.MustBeGreaterThan(height, 0, nameof(height));

            this.Count = width * height;
            this.Data = new T[this.Count];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fast2DArray{T}"/> struct.
        /// </summary>
        /// <param name="data">The 2D array to provide access to.</param>
        public Fast2DArray(T[,] data)
        {
            Guard.NotNull(data, nameof(data));
            this.Height = data.GetLength(0);
            this.Width = data.GetLength(1);

            Guard.MustBeGreaterThan(this.Width, 0, nameof(this.Width));
            Guard.MustBeGreaterThan(this.Height, 0, nameof(this.Height));

            this.Count = this.Width * this.Height;
            this.Data = new T[this.Count];

            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    this.Data[(y * this.Width) + x] = data[y, x];
                }
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified position.
        /// </summary>
        /// <param name="row">The row-coordinate of the item. Must be greater than or equal to zero and less than the height of the array.</param>
        /// <param name="column">The column-coordinate of the item. Must be greater than or equal to zero and less than the width of the array.</param>
        /// <returns>The <see typeparam="T"/> at the specified position.</returns>
        public T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                this.CheckCoordinates(row, column);
                return this.Data[(row * this.Width) + column];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this.CheckCoordinates(row, column);
                this.Data[(row * this.Width) + column] = value;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from a 2D array to a <see cref="Fast2DArray{T}" />.
        /// </summary>
        /// <param name="data">The source array.</param>
        /// <returns>
        /// The <see cref="Fast2DArray{T}"/> representation on the source data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fast2DArray<T>(T[,] data)
        {
            return new Fast2DArray<T>(data);
        }

        /// <summary>
        /// Gets a <see cref="Span{T}"/> representing the row beginning from the the first item on that row.
        /// </summary>
        /// <param name="row">The y-coordinate of the row. Must be greater than or equal to zero and less than the height of the 2D array.</param>
        /// <returns>The <see cref="Span{T}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetRowSpan(int row)
        {
            this.CheckCoordinates(row);
            return new Span<T>(this.Data, row * this.Width, this.Width);
        }

        /// <summary>
        /// Checks the coordinates to ensure they are within bounds.
        /// </summary>
        /// <param name="row">The y-coordinate of the item. Must be greater than zero and smaller than the height of the array.</param>
        /// <param name="column">The x-coordinate of the item. Must be greater than zero and smaller than the width of the array.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the coordinates are not within the bounds of the array.
        /// </exception>
        [Conditional("DEBUG")]
        private void CheckCoordinates(int row, int column)
        {
            if (row < 0 || row >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, $"{row} is outwith the array bounds.");
            }

            if (column < 0 || column >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, $"{column} is outwith the array bounds.");
            }
        }

        /// <summary>
        /// Checks the coordinates to ensure they are within bounds.
        /// </summary>
        /// <param name="row">The y-coordinate of the item. Must be greater than zero and smaller than the height of the array.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the coordinates are not within the bounds of the image.
        /// </exception>
        [Conditional("DEBUG")]
        private void CheckCoordinates(int row)
        {
            if (row < 0 || row >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, $"{row} is outwith the array bounds.");
            }
        }
    }
}