// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;
using Xunit;
// ReSharper disable InconsistentNaming

namespace SixLabors.ImageSharp.Tests.Memory
{
    

    /// <summary>
    /// Inherit this class to test an <see cref="IBuffer{T}"/> implementation (provided by <see cref="MemoryManager"/>).
    /// </summary>
    public abstract class BufferTestSuite
    {
        protected BufferTestSuite(MemoryManager memoryManager)
        {
            this.MemoryManager = memoryManager;
        }

        protected MemoryManager MemoryManager { get; }

        public struct CustomStruct : IEquatable<CustomStruct>
        {
            public long A;

            public byte B;

            public float C;

            public CustomStruct(long a, byte b, float c)
            {
                this.A = a;
                this.B = b;
                this.C = c;
            }

            public bool Equals(CustomStruct other)
            {
                return this.A == other.A && this.B == other.B && this.C.Equals(other.C);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is CustomStruct && this.Equals((CustomStruct)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = this.A.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.B.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.C.GetHashCode();
                    return hashCode;
                }
            }
        }

        public static readonly TheoryData<int> LenthValues = new TheoryData<int> { 0, 1, 7, 1023, 1024 };

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void HasCorrectLength_byte(int desiredLength)
        {
            this.TestHasCorrectLength<byte>(desiredLength);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void HasCorrectLength_float(int desiredLength)
        {
            this.TestHasCorrectLength<float>(desiredLength);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void HasCorrectLength_CustomStruct(int desiredLength)
        {
            this.TestHasCorrectLength<CustomStruct>(desiredLength);
        }

        private void TestHasCorrectLength<T>(int desiredLength)
            where T : struct
        {
            using (IBuffer<T> buffer = this.MemoryManager.Allocate<T>(desiredLength))
            {
                Assert.Equal(desiredLength, buffer.Span.Length);
            }
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void CanAllocateCleanBuffer_byte(int desiredLength)
        {
            this.TestCanAllocateCleanBuffer<byte>(desiredLength, false);
            this.TestCanAllocateCleanBuffer<byte>(desiredLength, true);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void CanAllocateCleanBuffer_double(int desiredLength)
        {
            this.TestCanAllocateCleanBuffer<double>(desiredLength);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void CanAllocateCleanBuffer_CustomStruct(int desiredLength)
        {
            this.TestCanAllocateCleanBuffer<CustomStruct>(desiredLength);
        }

        private IBuffer<T> Allocate<T>(int desiredLength, bool clean, bool managedByteBuffer)
            where T : struct
        {
            if (managedByteBuffer)
            {
                if (!(this.MemoryManager.AllocateManagedByteBuffer(desiredLength, clean) is IBuffer<T> buffer))
                {
                    throw new InvalidOperationException("typeof(T) != typeof(byte)");
                }

                return buffer;
            }

            return this.MemoryManager.Allocate<T>(desiredLength, clean);
        }

        private void TestCanAllocateCleanBuffer<T>(int desiredLength, bool testManagedByteBuffer = false)
            where T : struct, IEquatable<T>
        {
            ReadOnlySpan<T> expected = new T[desiredLength];

            for (int i = 0; i < 10; i++)
            {
                using (IBuffer<T> buffer = this.Allocate<T>(desiredLength, true, testManagedByteBuffer))
                {
                    Assert.True(buffer.Span.SequenceEqual(expected));
                }
            }
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void SpanPropertyIsAlwaysTheSame_int(int desiredLength)
        {
            this.TestSpanPropertyIsAlwaysTheSame<int>(desiredLength);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void SpanPropertyIsAlwaysTheSame_byte(int desiredLength)
        {
            this.TestSpanPropertyIsAlwaysTheSame<byte>(desiredLength, false);
            this.TestSpanPropertyIsAlwaysTheSame<byte>(desiredLength, true);
        }

        private void TestSpanPropertyIsAlwaysTheSame<T>(int desiredLength, bool testManagedByteBuffer = false)
            where T : struct
        {
            using (IBuffer<T> buffer = this.Allocate<T>(desiredLength, false, testManagedByteBuffer))
            {
                ref T a = ref MemoryMarshal.GetReference(buffer.Span);
                ref T b = ref MemoryMarshal.GetReference(buffer.Span);
                ref T c = ref MemoryMarshal.GetReference(buffer.Span);

                Assert.True(Unsafe.AreSame(ref a, ref b));
                Assert.True(Unsafe.AreSame(ref b, ref c));
            }
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void WriteAndReadElements_float(int desiredLength)
        {
            this.TestWriteAndReadElements<float>(desiredLength, x => x * 1.2f);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void WriteAndReadElements_byte(int desiredLength)
        {
            this.TestWriteAndReadElements<byte>(desiredLength, x => (byte)(x+1), false);
            this.TestWriteAndReadElements<byte>(desiredLength, x => (byte)(x + 1), true);
        }

        private void TestWriteAndReadElements<T>(int desiredLength, Func<int, T> getExpectedValue, bool testManagedByteBuffer = false)
            where T : struct
        {
            using (IBuffer<T> buffer = this.Allocate<T>(desiredLength, false, testManagedByteBuffer))
            {
                T[] expectedVals = new T[buffer.Length()];

                for (int i = 0; i < buffer.Length(); i++)
                {
                    Span<T> span = buffer.Span;
                    expectedVals[i] = getExpectedValue(i);
                    span[i] = expectedVals[i];
                }

                for (int i = 0; i < buffer.Length(); i++)
                {
                    Span<T> span = buffer.Span;
                    Assert.Equal(expectedVals[i], span[i]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void IndexingSpan_WhenOutOfRange_Throws_byte(int desiredLength)
        {
            this.TestIndexOutOfRangeShouldThrow<byte>(desiredLength, false);
            this.TestIndexOutOfRangeShouldThrow<byte>(desiredLength, true);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void IndexingSpan_WhenOutOfRange_Throws_long(int desiredLength)
        {
            this.TestIndexOutOfRangeShouldThrow<long>(desiredLength);
        }

        [Theory]
        [MemberData(nameof(LenthValues))]
        public void IndexingSpan_WhenOutOfRange_Throws_CustomStruct(int desiredLength)
        {
            this.TestIndexOutOfRangeShouldThrow<CustomStruct>(desiredLength);
        }

        private T TestIndexOutOfRangeShouldThrow<T>(int desiredLength, bool testManagedByteBuffer = false)
            where T : struct, IEquatable<T>
        {
            var dummy = default(T);

            using (IBuffer<T> buffer = this.Allocate<T>(desiredLength, false, testManagedByteBuffer))
            {
                Assert.ThrowsAny<Exception>(
                    () =>
                        {
                            Span<T> span = buffer.Span;
                            dummy = span[desiredLength];
                        });

                Assert.ThrowsAny<Exception>(
                    () =>
                        {
                            Span<T> span = buffer.Span;
                            dummy = span[desiredLength + 1];
                        });

                Assert.ThrowsAny<Exception>(
                    () =>
                        {
                            Span<T> span = buffer.Span;
                            dummy = span[desiredLength + 42];
                        });
            }

            return dummy;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(1024)]
        [InlineData(6666)]
        public void ManagedByteBuffer_ArrayIsCorrect(int desiredLength)
        {
            using (IManagedByteBuffer buffer = this.MemoryManager.AllocateManagedByteBuffer(desiredLength))
            {
                ref byte array0 = ref buffer.Array[0];
                ref byte span0 = ref buffer.DangerousGetPinnableReference();

                Assert.True(Unsafe.AreSame(ref span0, ref array0));
                Assert.True(buffer.Array.Length >= buffer.Span.Length);
            }
        }
    }
}