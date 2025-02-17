// This code originates from https://github.cds.internal.unity3d.com/andy-bastable/SpatialTree
// Check that repo and ask for permission before using it in other projects

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Runtime.Variables.Jobs
{
    public enum Comparison
    {
        Min,
        Max
    };


    [NativeContainerSupportsDeallocateOnJobCompletion]
    [NativeContainer]
    public unsafe struct NativePriorityHeap<T> : IDisposable where T : unmanaged, IComparable<T>
    {
        [NativeDisableUnsafePtrRestriction] private T* mBuffer;
        private readonly Allocator mAllocatorLabel;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle mSafety;
        [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel mDisposeSentinel;
#endif

        private readonly int mCompareMultiplier;

        public NativePriorityHeap(int capacity, Allocator allocator,
            Comparison comparison = Comparison.Min)
        {
            long totalSize = UnsafeUtility.SizeOf<T>() * capacity;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be >= 0");
#endif

            this.mBuffer = (T*)UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), allocator);

            this.Capacity = capacity;
            this.mAllocatorLabel = allocator;
            this.Count = 0;

            this.mCompareMultiplier = comparison == Comparison.Min ? 1 : -1;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out this.mSafety, out this.mDisposeSentinel, 0, allocator);
#endif
        }

        private NativePriorityHeap(in NativeArray<T> array, int count,
            Comparison comparison = Comparison.Min)
        {
            this.mBuffer = (T*)array.GetUnsafePtr();

            this.Capacity = array.Length;
            this.mAllocatorLabel = Allocator.None;
            this.Count = count;

            this.mCompareMultiplier = comparison == Comparison.Min ? 1 : -1;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out this.mSafety, out this.mDisposeSentinel, 0, Allocator.Temp);
#endif
        }

        public static NativePriorityHeap<T> FromArray(in NativeArray<T> array, int count,
            Comparison comparison = Comparison.Min)
        {
            return new NativePriorityHeap<T>(array, count, comparison);
        }

        public bool IsCreated => this.Capacity > 0;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref this.mSafety, ref this.mDisposeSentinel);
#endif

            UnsafeUtility.Free(this.mBuffer, this.mAllocatorLabel);
            this.mBuffer = null;
            this.Capacity = 0;
        }

        public void Clear()
        {
            this.Count = 0;
        }

        public int Count { get; private set; }

        public int Capacity { get; private set; }

        public void Push(T item)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(this.mSafety);
#endif

            if (this.Count >= this.Capacity)
                throw new InvalidOperationException(
                    $"Not enough capacity {this.Capacity} for NativePriorityHeap of size {this.Count}");

            // add new entry to bottom
            *(this.mBuffer + this.Count) = item;

            this.BubbleUp(this.Count);
            this.Count++;
        }

        public NativeArray<T> AsArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(this.mSafety);
            AtomicSafetyHandle arraySafety = this.mSafety;
            AtomicSafetyHandle.UseSecondaryVersion(ref arraySafety);
#endif
            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(this.mBuffer,
                this.Capacity,
                Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, arraySafety);
#endif
            return array;
        }

        public T Peek()
        {
            if (this.Count == 0)
                throw new InvalidOperationException("NativePriorityHeap is empty");

            T root = *this.mBuffer;

            return root;
        }

        public T Pop()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(this.mSafety);
#endif

            if (this.Count == 0)
                throw new InvalidOperationException("NativePriorityHeap is empty");

            T root = *this.mBuffer;

            // reduce count and swap last entry to top
            *this.mBuffer = *(this.mBuffer + this.Count - 1);
            this.Count--;

            // bubble down to ensure heap is ordered
            this.BubbleDown(0);

            return root;
        }

        private void BubbleUp(int i)
        {
            T* entryPtr = this.mBuffer + i;
            int parentIndex = GetParentIndex(i);

            while (entryPtr > this.mBuffer
                   && (this.mBuffer + parentIndex)->CompareTo(*entryPtr) * this.mCompareMultiplier > 0)
            {
                // swap
                T parentEntry = *(this.mBuffer + parentIndex);
                *(this.mBuffer + parentIndex) = *entryPtr;
                *entryPtr = parentEntry;

                entryPtr = this.mBuffer + parentIndex;
                i = parentIndex;
                parentIndex = GetParentIndex(i);
            }
        }

        private void BubbleDown(int initialIndex)
        {
            int smallestIndex = initialIndex;
            T* initialEntryPtr = this.mBuffer + initialIndex;
            T* smallestEntryPtr = this.mBuffer + smallestIndex;

            int leftIndex = GetLeftChildIndex(initialIndex);
            if (leftIndex < this.Count)
            {
                T* leftEntryPtr = this.mBuffer + leftIndex;
                if (initialEntryPtr->CompareTo(*leftEntryPtr) * this.mCompareMultiplier > 0)
                {
                    smallestIndex = leftIndex;
                    smallestEntryPtr = leftEntryPtr;
                }
            }

            int rightIndex = GetRightChildIndex(initialIndex);
            if (rightIndex < this.Count)
            {
                T* rightEntryPtr = this.mBuffer + rightIndex;
                if (smallestEntryPtr->CompareTo(*rightEntryPtr) * this.mCompareMultiplier > 0)
                {
                    smallestIndex = rightIndex;
                    smallestEntryPtr = rightEntryPtr;
                }
            }

            if (smallestIndex == initialIndex) return;

            T temp = *initialEntryPtr;
            *initialEntryPtr = *smallestEntryPtr;
            *smallestEntryPtr = temp;

            this.BubbleDown(smallestIndex);
        }

        private static int GetParentIndex(int i)
        {
            return (i - 1) / 2;
        }

        private static int GetLeftChildIndex(int i)
        {
            return 2 * i + 1;
        }

        private static int GetRightChildIndex(int i)
        {
            return 2 * i + 2;
        }
    }
}