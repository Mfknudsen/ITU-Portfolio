using System;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Runtime.Variables.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public partial struct AgentAStarJob : IJobEntity
    {
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<AreaBufferElement> areaType;
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;

        public AgentAStarJob(DynamicBuffer<VertYBufferElement> vertsY,
            DynamicBuffer<VertXZBufferElement> simpleVerts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<AreaBufferElement> areaType) : this()
        {
            simpleVerts.ToNativeArray(Allocator.TempJob);
            vertsY.ToNativeArray(Allocator.TempJob);
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.areaType = areaType.ToNativeArray(Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(ref UnitAgentComponent agent, ref DynamicBuffer<AgentTrianglePathBufferElement> agentPath,
            in AgentSettingsComponent agentSettings, in LocalTransform transform, in DestinationComponent destination)
        {
            if (agent.CurrentTriangleID == -1 || destination.TriangleID == -1)
                return;

            agentPath.Clear();
            NativePriorityHeap<JobNodeEcs> toCheckNodes =
                new NativePriorityHeap<JobNodeEcs>(256, Allocator.Temp);
            NativeList<JobNodeEcs> checkedNodes = new NativeList<JobNodeEcs>(256, Allocator.Temp);
            NativeHashSet<int> hashSet = new NativeHashSet<int>(256, Allocator.Temp);

            toCheckNodes.Push(new JobNodeEcs(
                this.triangles[agent.CurrentTriangleID],
                destination.Point));

            if (destination.Debug)
            {
                Debug.DrawRay(this.triangles[agent.CurrentTriangleID].Center, Vector3.up, Color.green);
                Debug.DrawRay(this.triangles[destination.TriangleID].Center, Vector3.up, Color.green);
            }

            JobNodeEcs checking = toCheckNodes.Pop();
            bool first = true;
            while (toCheckNodes.Count > 0 || first)
            {
                if (!first)
                    checking = toCheckNodes.Pop();
                first = false;
                checkedNodes.Add(checking);

                NavTriangleBufferElement triangle = this.triangles[checking.TriangleID];

                if (checking.TriangleID == destination.TriangleID)
                    break;

                for (int neighborIndex = 0; neighborIndex < 3; neighborIndex++)
                {
                    int neighborTriangleId = neighborIndex switch
                    {
                        0 => triangle.NeighborOne,
                        1 => triangle.NeighborTwo,
                        2 => triangle.NeighborThree,
                        _ => -1
                    };

                    if (neighborTriangleId == -1)
                        break;

                    if (neighborIndex switch
                        {
                            0 => triangle.NeighborOneWidth,
                            1 => triangle.NeighborTwoWidth,
                            2 => triangle.NeighborThreeWidth,
                            _ => 0
                        } < agentSettings.Radius * 2f)
                        continue;

                    if (hashSet.Contains(neighborTriangleId))
                        continue;

                    JobNodeEcs newJobNode = new JobNodeEcs(
                        this.triangles[neighborTriangleId],
                        destination.Point,
                        checking,
                        triangle.Center,
                        checkedNodes.Length - 1,
                        this.areaType);

                    if (destination.Debug)
                        Debug.DrawLine(triangle.Center, this.triangles[neighborTriangleId].Center, Color.yellow);

                    hashSet.Add(neighborTriangleId);

                    toCheckNodes.Push(newJobNode);
                }
            }

            //Retrace backwards towards current triangle
            NativeList<int> nodePath = new NativeList<int>(64, Allocator.TempJob);

            while (checking.PreviousCheckedID != -1)
            {
                nodePath.Add(checking.TriangleID);

                checking = checkedNodes[checking.PreviousCheckedID];
            }

            nodePath.Add(checking.TriangleID);

            //Reverse the path
            if (agentPath.Length > nodePath.Length)
                agentPath.RemoveRange(nodePath.Length, agentPath.Length - nodePath.Length);
            for (int i = 0; i < agentPath.Length; i++)
            {
                AgentTrianglePathBufferElement t = agentPath[i];
                t.Index = nodePath[nodePath.Length - 1 - i];
                agentPath[i] = t;
            }

            if (agentPath.Length < nodePath.Length)
            {
                for (int i = agentPath.Length; i < nodePath.Length; i++)
                    agentPath.Add(new AgentTrianglePathBufferElement { Index = nodePath[nodePath.Length - 1 - i] });
            }

            nodePath.Dispose();
            toCheckNodes.Dispose();
            checkedNodes.Dispose();
        }
    }

    [BurstCompile]
    public readonly struct JobNodeEcs : IComparable<JobNodeEcs>
    {
        public readonly int TriangleID;
        private readonly float moveCost, distanceToGoal;
        public readonly int PreviousCheckedID;

        public JobNodeEcs(in NavTriangleBufferElement triangle,
            in float3 destination,
            in JobNodeEcs previousJobNode,
            in float3 previousCenter,
            int previousCheckedID,
            in NativeArray<AreaBufferElement> areas)
        {
            this.PreviousCheckedID = previousCheckedID;
            this.TriangleID = triangle.ID;

            this.moveCost = previousJobNode.MoveCost() + triangle.Center.QuickSquareDistance(previousCenter);
            this.distanceToGoal = triangle.Center.QuickSquareDistance(destination);
        }

        public JobNodeEcs(in NavTriangleBufferElement triangle,
            float3 destination)
        {
            this.PreviousCheckedID = -1;
            this.TriangleID = triangle.ID;

            this.moveCost = 0;

            this.distanceToGoal = triangle.Center.QuickSquareDistance(destination);
        }

        [BurstCompile]
        public float Total()
        {
            return this.moveCost + this.distanceToGoal;
        }

        [BurstCompile]
        public float MoveCost()
        {
            return this.moveCost;
        }

        [BurstCompile]
        public int CompareTo(JobNodeEcs other)
        {
            return this.Total().CompareTo(other.Total());
        }
    }
}