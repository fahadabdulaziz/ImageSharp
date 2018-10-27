﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing a single 8 bit normalized gray values.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct Gray8 : IPixel<Gray8>, IPackedVector<byte>
    {
        private static readonly Vector4 MaxBytes = new Vector4(255F);
        private static readonly Vector4 Half = new Vector4(0.5F);
        private const float Average = 1 / 3F;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gray8"/> struct.
        /// </summary>
        /// <param name="luminance">The luminance component.</param>
        public Gray8(byte luminance) => this.PackedValue = luminance;

        /// <inheritdoc />
        public byte PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Gray8"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Gray8"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Gray8"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static bool operator ==(Gray8 left, Gray8 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Gray8"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Gray8"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Gray8"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public static bool operator !=(Gray8 left, Gray8 right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<Gray8> CreatePixelOperations() => new PixelOperations();

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromVector4(Vector4 vector)
        {
            vector *= MaxBytes;
            vector += Half;
            vector = Vector4.Clamp(vector, Vector4.Zero, MaxBytes) * Average;
            this.PackedValue = (byte)(vector.X + vector.Y + vector.Z);
        }

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public Vector4 ToVector4()
        {
            float rgb = this.PackedValue / 255F;
            return new Vector4(rgb, rgb, rgb, 1F);
        }

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromArgb32(Argb32 source) => this.PackedValue = ImageMaths.Get8BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromBgr24(Bgr24 source) => this.PackedValue = ImageMaths.Get8BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromBgra32(Bgra32 source) => this.PackedValue = ImageMaths.Get8BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromGray8(Gray8 source) => this.PackedValue = source.PackedValue;

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromGray16(Gray16 source) => this.PackedValue = ImageMaths.DownScaleFrom16BitTo8Bit(source.PackedValue);

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromRgb24(Rgb24 source) => this.PackedValue = ImageMaths.Get8BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromRgba32(Rgba32 source) => this.PackedValue = ImageMaths.Get8BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = this.PackedValue;
            dest.G = this.PackedValue;
            dest.B = this.PackedValue;
            dest.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromRgb48(Rgb48 source)
            => this.PackedValue = ImageMaths.Get8BitBT709Luminance(
                ImageMaths.DownScaleFrom16BitTo8Bit(source.R),
                ImageMaths.DownScaleFrom16BitTo8Bit(source.G),
                ImageMaths.DownScaleFrom16BitTo8Bit(source.B));

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public void FromRgba64(Rgba64 source)
            => this.PackedValue = ImageMaths.Get8BitBT709Luminance(
                ImageMaths.DownScaleFrom16BitTo8Bit(source.R),
                ImageMaths.DownScaleFrom16BitTo8Bit(source.G),
                ImageMaths.DownScaleFrom16BitTo8Bit(source.B));

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Gray8 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public bool Equals(Gray8 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString() => $"Gray8({this.PackedValue})";

        /// <inheritdoc />
        [MethodImpl(InliningOptions.ShortMethod)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(InliningOptions.ShortMethod)]
        internal void ConvertFromRgbaScaledVector4(Vector4 vector)
        {
            vector *= MaxBytes;
            vector += Half;
            vector = Vector4.Clamp(vector, Vector4.Zero, MaxBytes);
            this.PackedValue = ImageMaths.Get8BitBT709Luminance((byte)vector.X, (byte)vector.Y, (byte)vector.Z);
        }
    }
}