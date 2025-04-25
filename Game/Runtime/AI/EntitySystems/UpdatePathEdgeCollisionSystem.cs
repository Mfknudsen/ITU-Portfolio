using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [UpdateAfter(typeof(TrianglesInCellsSystem))]
    [UpdateBefore(typeof(UpdateMoveDirectionSystem))]
    internal partial struct UpdatePathEdgeCollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            DynamicBuffer<VertBufferElement> verts = SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);
            DynamicBuffer<EdgeCollisionBufferElement> edgeCollisions =
                SystemAPI.GetBuffer<EdgeCollisionBufferElement>(navmeshEntity);

            if (edgeCollisions.Length < triangles.Length * 3)
            {
                edgeCollisions.AddRange(
                    new NativeArray<EdgeCollisionBufferElement>(
                        triangles.Length * 3 - edgeCollisions.Length,
                        Allocator.Temp));
            }

            UpdateEdgeCollisionsJob updateEdgeCollisionsJob = new UpdateEdgeCollisionsJob(
                verts,
                triangles,
                edgeCollisions);
            state.Dependency = updateEdgeCollisionsJob.Schedule(triangles.Length,
                triangles.Length / SystemInfo.processorCount, state.Dependency);
        }
    }

    [BurstCompile]
    internal struct UpdateEdgeCollisionsJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<VertBufferElement> verts;

        [NativeDisableParallelForRestriction] private DynamicBuffer<NavTriangleBufferElement> triangles;

        [NativeDisableParallelForRestriction] private DynamicBuffer<EdgeCollisionBufferElement> edgeCollisions;

        public UpdateEdgeCollisionsJob(DynamicBuffer<VertBufferElement> verts,
            DynamicBuffer<NavTriangleBufferElement> triangles, DynamicBuffer<EdgeCollisionBufferElement> edgeCollisions)
        {
            this.verts = verts;
            this.triangles = triangles;
            this.edgeCollisions = edgeCollisions;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            EdgeCollisionBufferElement edge = this.edgeCollisions[index];
            NavTriangleBufferElement triangle = this.triangles[index];

            float3 a = this.verts[triangle.A].Position,
                b = this.verts[triangle.B].Position,
                c = this.verts[triangle.C].Position;

            float3 ab = triangle.AB,
                bc = triangle.BC,
                ca = -triangle.AC;

            int count = 0;

            if (triangle.ABEdge)
            {
                this.edgeCollisions[index * 3] = new EdgeCollisionBufferElement
                {
                    Start = a,
                    End = b,
                    StartOffset = -ca,
                    EndOffset = bc,
                    StartEnd = ab
                };

                count++;
                Debug.DrawLine(a, b);
            }

            if (triangle.ACEdge)
            {
                this.edgeCollisions[index * 3 + count] = new EdgeCollisionBufferElement
                {
                    Start = a,
                    End = c,
                    StartOffset = -bc,
                    EndOffset = ab,
                    StartEnd = ca
                };
                count++;
                Debug.DrawLine(a, c);
            }

            if (triangle.BCEdge)
            {
                this.edgeCollisions[index * 3 + count] = new EdgeCollisionBufferElement
                {
                    Start = b,
                    End = c,
                    StartOffset = -ab,
                    EndOffset = ca,
                    StartEnd = bc
                };

                Debug.DrawLine(b, c);
            }

            this.edgeCollisions[index] = edge;
        }
    }
}