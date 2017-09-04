﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Colors
{
    /// <summary>
    /// Test implementations of IEquatable
    /// </summary>
    public class ColorEqualityTests
    {
        public static readonly TheoryData<object, object, Type> EqualityData =
           new TheoryData<object, object, Type>()
           {
               { new Alpha8(.5F), new Alpha8(.5F), typeof(Alpha8) },
               { new Argb32(Vector4.One), new Argb32(Vector4.One), typeof(Argb32) },
               { new Bgr565(Vector3.One), new Bgr565(Vector3.One), typeof(Bgr565) },
               { new Bgra4444(Vector4.One), new Bgra4444(Vector4.One), typeof(Bgra4444) },
               { new Bgra5551(Vector4.One), new Bgra5551(Vector4.One), typeof(Bgra5551) },
               { new Byte4(Vector4.One * 255), new Byte4(Vector4.One * 255), typeof(Byte4) },
               { new HalfSingle(-1F), new HalfSingle(-1F), typeof(HalfSingle) },
               { new HalfVector2(0.1f, -0.3f), new HalfVector2(0.1f, -0.3f), typeof(HalfVector2) },
               { new HalfVector4(Vector4.One), new HalfVector4(Vector4.One), typeof(HalfVector4) },
               { new NormalizedByte2(-Vector2.One), new NormalizedByte2(-Vector2.One), typeof(NormalizedByte2) },
               { new NormalizedByte4(Vector4.One), new NormalizedByte4(Vector4.One), typeof(NormalizedByte4) },
               { new NormalizedShort2(Vector2.One), new NormalizedShort2(Vector2.One), typeof(NormalizedShort2) },
               { new NormalizedShort4(Vector4.One), new NormalizedShort4(Vector4.One), typeof(NormalizedShort4) },
               { new Rg32(Vector2.One), new Rg32(Vector2.One), typeof(Rg32) },
               { new Rgba1010102(Vector4.One), new Rgba1010102(Vector4.One), typeof(Rgba1010102) },
               { new Rgba32(Vector4.One), new Rgba32(Vector4.One), typeof(Rgba32) },
               { new Rgba64(Vector4.One), new Rgba64(Vector4.One), typeof(Rgba64) },
               { new Short2(Vector2.One * 0x7FFF), new Short2(Vector2.One * 0x7FFF), typeof(Short2) },
               { new Short4(Vector4.One * 0x7FFF), new Short4(Vector4.One * 0x7FFF), typeof(Short4) },
           };

        public static readonly TheoryData<object, object, Type> NotEqualityDataNulls =
            new TheoryData<object, object, Type>()
            {
                // Valid object against null
                { new Alpha8(.5F), null, typeof(Alpha8) },
                { new Argb32(Vector4.One), null, typeof(Argb32) },
                { new Bgr565(Vector3.One), null, typeof(Bgr565) },
                { new Bgra4444(Vector4.One), null, typeof(Bgra4444) },
                { new Bgra5551(Vector4.One), null, typeof(Bgra5551) },
                { new Byte4(Vector4.One * 255), null, typeof(Byte4) },
                { new HalfSingle(-1F), null, typeof(HalfSingle) },
                { new HalfVector2(0.1f, -0.3f), null, typeof(HalfVector2) },
                { new HalfVector4(Vector4.One), null, typeof(HalfVector4) },
                { new NormalizedByte2(-Vector2.One), null, typeof(NormalizedByte2) },
                { new NormalizedByte4(Vector4.One), null, typeof(NormalizedByte4) },
                { new NormalizedShort2(Vector2.One), null, typeof(NormalizedShort2) },
                { new NormalizedShort4(Vector4.One), null, typeof(NormalizedShort4) },
                { new Rg32(Vector2.One), null, typeof(Rg32) },
                { new Rgba1010102(Vector4.One), null, typeof(Rgba1010102) },
                { new Rgba64(Vector4.One), null, typeof(Rgba64) },
                { new Short2(Vector2.One * 0x7FFF), null, typeof(Short2) },
                { new Short4(Vector4.One * 0x7FFF), null, typeof(Short4) },
            };

        public static readonly TheoryData<object, object, Type> NotEqualityDataDifferentObjects =
           new TheoryData<object, object, Type>()
           {
                // Valid objects of different types but not equal
                { new Alpha8(.5F), new Argb32(Vector4.Zero), null },
                { new HalfSingle(-1F), new NormalizedShort2(Vector2.Zero), null },
                { new Rgba1010102(Vector4.One), new Bgra5551(Vector4.Zero), null },
           };

        public static readonly TheoryData<object, object, Type> NotEqualityData =
           new TheoryData<object, object, Type>()
           {
                // Valid objects of the same type but not equal
                { new Alpha8(.5F), new Alpha8(.8F), typeof(Alpha8) },
                { new Argb32(Vector4.One), new Argb32(Vector4.Zero), typeof(Argb32) },
                { new Bgr565(Vector3.One), new Bgr565(Vector3.Zero), typeof(Bgr565) },
                { new Bgra4444(Vector4.One), new Bgra4444(Vector4.Zero), typeof(Bgra4444) },
                { new Bgra5551(Vector4.One), new Bgra5551(Vector4.Zero), typeof(Bgra5551) },
                { new Byte4(Vector4.One * 255), new Byte4(Vector4.Zero), typeof(Byte4) },
                { new HalfSingle(-1F), new HalfSingle(1F), typeof(HalfSingle) },
                { new HalfVector2(0.1f, -0.3f), new HalfVector2(0.1f, 0.3f), typeof(HalfVector2) },
                { new HalfVector4(Vector4.One), new HalfVector4(Vector4.Zero), typeof(HalfVector4) },
                { new NormalizedByte2(-Vector2.One), new NormalizedByte2(-Vector2.Zero), typeof(NormalizedByte2) },
                { new NormalizedByte4(Vector4.One), new NormalizedByte4(Vector4.Zero), typeof(NormalizedByte4) },
                { new NormalizedShort2(Vector2.One), new NormalizedShort2(Vector2.Zero), typeof(NormalizedShort2) },
                { new NormalizedShort4(Vector4.One), new NormalizedShort4(Vector4.Zero), typeof(NormalizedShort4) },
                { new Rg32(Vector2.One), new Rg32(Vector2.Zero), typeof(Rg32) },
                { new Rgba1010102(Vector4.One), new Rgba1010102(Vector4.Zero), typeof(Rgba1010102) },
                { new Rgba32(Vector4.One), new Rgba32(Vector4.Zero), typeof(Rgba32) },
                { new Rgba64(Vector4.One), new Rgba64(Vector4.Zero), typeof(Rgba64) },
                { new Short2(Vector2.One * 0x7FFF), new Short2(Vector2.Zero), typeof(Short2) },
                { new Short4(Vector4.One * 0x7FFF), new Short4(Vector4.Zero), typeof(Short4) },
           };

        [Theory]
        [MemberData(nameof(EqualityData))]
        public void Equality(object first, object second, Type type)
        {
            // Act
            bool equal = first.Equals(second);

            // Assert
            Assert.True(equal);
        }

        [Theory]
        [MemberData(nameof(NotEqualityDataNulls))]
        [MemberData(nameof(NotEqualityDataDifferentObjects))]
        [MemberData(nameof(NotEqualityData))]
        public void NotEquality(object first, object second, Type type)
        {
            // Act
            bool equal = first.Equals(second);

            // Assert
            Assert.False(equal);
        }

        [Theory]
        [MemberData(nameof(EqualityData))]
        public void HashCodeEqual(object first, object second, Type type)
        {
            // Act
            bool equal = first.GetHashCode() == second.GetHashCode();

            // Assert
            Assert.True(equal);
        }

        [Theory]
        [MemberData(nameof(NotEqualityDataDifferentObjects))]
        public void HashCodeNotEqual(object first, object second, Type type)
        {
            // Act
            bool equal = first.GetHashCode() == second.GetHashCode();

            // Assert
            Assert.False(equal);
        }

        [Theory]
        [MemberData(nameof(EqualityData))]
        public void EqualityObject(object first, object second, Type type)
        {
            // Arrange
            // Cast to the known object types, this is so that we can hit the
            // equality operator on the concrete type, otherwise it goes to the
            // default "object" one :)
            dynamic firstObject = Convert.ChangeType(first, type);
            dynamic secondObject = Convert.ChangeType(second, type);

            // Act
            dynamic equal = firstObject.Equals(secondObject);

            // Assert
            Assert.True(equal);
        }

        [Theory]
        [MemberData(nameof(NotEqualityData))]
        public void NotEqualityObject(object first, object second, Type type)
        {
            // Arrange
            // Cast to the known object types, this is so that we can hit the
            // equality operator on the concrete type, otherwise it goes to the
            // default "object" one :)
            dynamic firstObject = Convert.ChangeType(first, type);
            dynamic secondObject = Convert.ChangeType(second, type);

            // Act
            dynamic equal = firstObject.Equals(secondObject);

            // Assert
            Assert.False(equal);
        }

        [Theory]
        [MemberData(nameof(EqualityData))]
        public void EqualityOperator(object first, object second, Type type)
        {
            // Arrange
            // Cast to the known object types, this is so that we can hit the
            // equality operator on the concrete type, otherwise it goes to the
            // default "object" one :)
            dynamic firstObject = Convert.ChangeType(first, type);
            dynamic secondObject = Convert.ChangeType(second, type);

            // Act
            dynamic equal = firstObject == secondObject;

            // Assert
            Assert.True(equal);
        }

        [Theory]
        [MemberData(nameof(NotEqualityData))]
        public void NotEqualityOperator(object first, object second, Type type)
        {
            // Arrange
            // Cast to the known object types, this is so that we can hit the
            // equality operator on the concrete type, otherwise it goes to the
            // default "object" one :)
            dynamic firstObject = Convert.ChangeType(first, type);
            dynamic secondObject = Convert.ChangeType(second, type);

            // Act
            dynamic notEqual = firstObject != secondObject;

            // Assert
            Assert.True(notEqual);
        }
    }
}
