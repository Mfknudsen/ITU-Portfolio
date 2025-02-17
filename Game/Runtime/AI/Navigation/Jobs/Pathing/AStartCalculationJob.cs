#region Libraries

using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    [BurstCompile]
    public struct AStartCalculationJob : IJobParallelFor
    {
        #region Values

        [WriteOnly] private NativeArray<JobPath> paths;
        [ReadOnly] private NativeArray<JobAgent> agents;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [ReadOnly] private NativeArray<float2> simpleVerts;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [ReadOnly] private NativeArray<int> areas;

        [ReadOnly] private NativeArray<JobTriangle> triangles;

        #endregion

        #region Build In States

        public AStartCalculationJob(NativeArray<JobPath> paths, NativeArray<JobAgent> agents,
            NativeArray<float2> simpleVerts,
            NativeArray<int> areas, NativeArray<JobTriangle> triangles)
        {
            this.paths = paths;
            this.agents = agents;
            this.simpleVerts = simpleVerts;
            this.areas = areas;
            this.triangles = triangles;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            UnsafeList<JobNode> toCheckNodes = new UnsafeList<JobNode>(20, Allocator.TempJob),
                checkedNodes = new UnsafeList<JobNode>(20, Allocator.TempJob);

            float3 destination = this.agents[index].endPoint;
            toCheckNodes.Add(new JobNode(this.triangles[this.agents[index].currentTriangleID],
                this.simpleVerts, new float2(destination.x, destination.z), 0));

            JobNode checking = toCheckNodes[0];

            while (toCheckNodes.Length > 0)
            {
                checking = toCheckNodes[0];
                toCheckNodes.RemoveAt(0);
                checkedNodes.Add(checking);

                JobTriangle triangle = this.triangles[checking.triangleID];

                if (checking.triangleID == this.agents[index].destinationTriangleID)
                    break;

                for (int neighborIndex = 0; neighborIndex < triangle.neighbors.Length; neighborIndex++)
                {
                    int neighborTriangleId = triangle.neighbors[neighborIndex];
                    if (triangle.widths[neighborIndex] < this.agents[index].radius * 2f)
                        continue;

                    if (ContainsID(toCheckNodes, neighborTriangleId) ||
                        ContainsID(checkedNodes, neighborTriangleId))
                        continue;

                    JobNode newJobNode = new JobNode(
                        this.triangles[neighborTriangleId],
                        this.simpleVerts,
                        this.agents[index].endPoint.XZFloat(),
                        checking, this.areas, this.triangles, 0);

                    if (toCheckNodes.Length > 0)
                    {
                        bool added = false;
                        for (int j = 0; j < toCheckNodes.Length; j++)
                            if (newJobNode.Total() < toCheckNodes[j].Total())
                            {
                                added = true;
                                toCheckNodes.Add(toCheckNodes[^1]);

                                for (int x = toCheckNodes.Length - 2; x > j && x > 0; x--)
                                    toCheckNodes[x] = toCheckNodes[x - 1];

                                toCheckNodes[j] = newJobNode;

                                break;
                            }

                        if (!added)
                            toCheckNodes.Add(newJobNode);
                    }
                    else
                    {
                        toCheckNodes.Add(newJobNode);
                    }
                }
            }

            this.paths[index] = new JobPath(checking, checkedNodes, this.triangles,
                this.agents[index].currentTriangleID, this.agents[index].destinationTriangleID);

            toCheckNodes.Dispose();
            checkedNodes.Dispose();
        }

        #endregion

        #region Internal

        private static bool ContainsID(UnsafeList<JobNode> list, int target)
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].triangleID == target)
                    return true;

            return false;
        }

        #endregion
    }
}