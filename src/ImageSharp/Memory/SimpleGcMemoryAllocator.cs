﻿using System.Buffers;

namespace SixLabors.Memory
{
    /// <summary>
    /// Implements <see cref="MemoryAllocator"/> by newing up arrays by the GC on every allocation requests.
    /// </summary>
    public sealed class SimpleGcMemoryAllocator : MemoryAllocator
    {
        /// <inheritdoc />
        internal override IMemoryOwner<T> Allocate<T>(int length, AllocationOptions options = AllocationOptions.None)
        {
            return new BasicArrayBuffer<T>(new T[length]);
        }

        internal override IManagedByteBuffer AllocateManagedByteBuffer(int length, AllocationOptions options)
        {
            return new BasicByteBuffer(new byte[length]);
        }
    }
}