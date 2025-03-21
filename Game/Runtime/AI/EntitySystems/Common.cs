using System;
using Runtime.AI.EntityBuffers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    internal static class Common
    {
        public static int GetTriangleIdsByPositionSpiralOutwards(
            ref NativeList<TriangleFlattenIndexBufferElement> reuseList,
            float3 position, int min, int max,
            int increaseForSpiral, int cellXLength, int cellZLength, float minFloorX, float minFloorZ,
            float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenBufferElement> triangleArraySizes)
        {
            if (max <= min) throw new Exception("Max must be greater then min");

            if (increaseForSpiral < 0) throw new Exception("Increase must be zero or greater");

            int reuseSize = 0;

            NativeHashSet<int> addedIds = new NativeHashSet<int>(64, Allocator.Temp);
            while (reuseList.Length == 0 &&
                   (max - (max - min) < cellXLength ||
                    max - (max - min) < cellZLength))
            {
                int2 id = GroupingIDByPosition(position, minFloorX, minFloorZ, cellXLength, cellZLength, groupDivision);

                for (int x = -max; x <= max; x++)
                {
                    if (Mathf.Abs(x) <= min) continue;

                    if (id.x + x < 0 || id.x + x >= cellXLength) continue;

                    for (int y = -max; y <= max; y++)
                    {
                        if (id.y + y < 0 || id.y + y >= cellZLength) continue;

                        reuseSize = GetTriangleIndexByXZ(
                            ref reuseList,
                            id.x + x, id.y + y,
                            cellXLength, triangleIndexes,
                            triangleArraySizes);

                        for (int i = 0; i < reuseSize; i++)
                        {
                            TriangleFlattenIndexBufferElement t = reuseList[i];
                            if (addedIds.Contains(t.Index)) continue;

                            addedIds.Add(t.Index);
                            reuseList.Add(t);
                            reuseSize++;
                        }
                    }
                }

                if (increaseForSpiral == 0)
                    break;

                min += increaseForSpiral;
                max += increaseForSpiral;
            }


            return reuseSize;
        }

        public static int GetTriangleIdsByPosition(
            ref NativeList<TriangleFlattenIndexBufferElement> reuseArray,
            float3 pos,
            int cellXLength, int cellZLength,
            float minFloorX, float minFloorZ, float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenBufferElement> triangleArraySizes)
        {
            int2 id = GroupingIDByPosition(pos, minFloorX, minFloorZ, cellXLength, cellZLength, groupDivision);

            return GetTriangleIndexByXZ(ref reuseArray, id.x, id.y, cellXLength, triangleIndexes,
                triangleArraySizes);
        }

        public static int GetTriangleIdsByPosition(
            ref NativeList<TriangleFlattenIndexBufferElement> reuseArray,
            float x, float z,
            int cellXLength, int cellZLength,
            float minFloorX, float minFloorZ, float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenBufferElement> triangleArraySizes)
        {
            int2 id = GroupingIDByPosition(x, z, minFloorX, minFloorZ, cellXLength, cellZLength, groupDivision);

            return GetTriangleIndexByXZ(ref reuseArray, id.x, id.y, cellXLength, triangleIndexes, triangleArraySizes);
        }

        private static int2 GroupingIDByPosition(float3 position, float minFloorX, float minFloorZ, int cellXLength,
            int cellZLength, float groupDivision)
        {
            return new int2(
                Mathf.FloorToInt(Mathf.Clamp(
                    (position.x - minFloorX) / groupDivision,
                    0,
                    cellXLength - 1)),
                Mathf.FloorToInt(Mathf.Clamp(
                    (position.z - minFloorZ) / groupDivision,
                    0,
                    cellZLength - 1)));
        }

        private static int2 GroupingIDByPosition(float x, float z, float minFloorX, float minFloorZ, int cellXLength,
            int cellZLength, float groupDivision)
        {
            return new int2(
                Mathf.FloorToInt(Mathf.Clamp(
                    (x - minFloorX) / groupDivision,
                    0,
                    cellXLength - 1)),
                Mathf.FloorToInt(Mathf.Clamp(
                    (z - minFloorZ) / groupDivision,
                    0,
                    cellZLength - 1)));
        }

        private static int GetTriangleIndexByXZ(
            ref NativeList<TriangleFlattenIndexBufferElement> reuseArray,
            int x, int z,
            int cellXLength,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenBufferElement> triangleArraySizes)
        {
            int arrayPosition2D = x * cellXLength + z;
            int size = triangleArraySizes[arrayPosition2D].Size;
            int startPositionInArray3D = triangleArraySizes[arrayPosition2D].StartIndex;

            for (int i = 0; i < size; i++)
            {
                if (i < reuseArray.Length)
                    reuseArray[i] = triangleIndexes[startPositionInArray3D + i];
                else
                    reuseArray.Add(triangleIndexes[startPositionInArray3D + i]);
            }

            return size;
        }
    }
}