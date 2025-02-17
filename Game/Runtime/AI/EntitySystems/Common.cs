using System;
using Runtime.AI.EntityBuffers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    internal static class Common
    {
        public static NativeList<int> GetTriangleIdsByPositionSpiralOutwards(float3 position, int min, int max,
            int increaseForSpiral, int cellXLength, int cellZLength, float minFloorX, float minFloorZ,
            float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray,
            NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes)
        {
            if (max <= min) throw new Exception("Max must be greater then min");

            if (increaseForSpiral < 0) throw new Exception("Increase must be zero or greater");

            NativeList<int> result = new NativeList<int>(Allocator.Temp);
            while (result.Length == 0 &&
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

                        foreach (int i in GetTriangleIndexByXZ(id.x + x, id.y + y, cellXLength, triangleIndexes,
                                     triangleStartIndexArray, triangleArraySizes))
                            if (!result.Contains(i))
                                result.Add(i);
                    }
                }

                if (increaseForSpiral == 0)
                    break;

                min += increaseForSpiral;
                max += increaseForSpiral;
            }

            return result;
        }

        public static NativeArray<int> GetTriangleIdsByPosition(float3 pos, int cellXLength, int cellZLength,
            float minFloorX, float minFloorZ, float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray,
            NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes)
        {
            int2 id = GroupingIDByPosition(pos, minFloorX, minFloorZ, cellXLength, cellZLength, groupDivision);

            return GetTriangleIndexByXZ(id.x, id.y, cellXLength, triangleIndexes,
                triangleStartIndexArray, triangleArraySizes);
        }

        public static NativeArray<int> GetTriangleIdsByPosition(float x, float z, int cellXLength, int cellZLength,
            float minFloorX, float minFloorZ, float groupDivision,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray,
            NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes)
        {
            int2 id = GroupingIDByPosition(x, z, minFloorX, minFloorZ, cellXLength, cellZLength, groupDivision);

            return GetTriangleIndexByXZ(id.x, id.y, cellXLength, triangleIndexes,
                triangleStartIndexArray, triangleArraySizes);
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

        private static NativeArray<int> GetTriangleIndexByXZ(int x, int z, int cellXLength,
            NativeArray<TriangleFlattenIndexBufferElement> triangleIndexes,
            NativeArray<TriangleFlattenStartIndexBufferElement> triangleStartIndexArray,
            NativeArray<TriangleFlattenSizeBufferElement> triangleArraySizes)
        {
            int arrayPosition2D = x * cellXLength + z;
            int size = triangleArraySizes[arrayPosition2D].Size;
            int startPositionInArray3D = triangleStartIndexArray[arrayPosition2D].Index;

            NativeArray<int> result = new NativeArray<int>(size, Allocator.Temp);
            for (int i = 0; i < size; i++)
                result[i] = triangleIndexes[startPositionInArray3D + i].Index;

            return result;
        }

        public static float3 VertByIndex(int index, NativeArray<VertXZBufferElement> simpleVerts,
            NativeArray<VertYBufferElement> vertsY)
        {
            return new float3(simpleVerts[index].X, vertsY[index].Y, simpleVerts[index].Z);
        }
    }
}