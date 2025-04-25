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
    internal partial struct FlattenPathCollisionSystem : ISystem
    {
        private EntityQuery entityQuery;

        private ComponentLookup<NavMeshCellComponent> cellLookup;
        private BufferLookup<CellEdgeCollisionBufferElement> edgeCollisionBufferLookup;
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> triangleBufferLookup;
        private DynamicBuffer<NavTriangleBufferElement> triangles;
        private DynamicBuffer<VertBufferElement> verts;

        private NavigationMeshSingletonComponent navmesh;

        private bool singletonReady;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<NavMeshCellComponent>());

            this.cellLookup = state.GetComponentLookup<NavMeshCellComponent>();
            this.edgeCollisionBufferLookup = state.GetBufferLookup<CellEdgeCollisionBufferElement>();
            this.triangleBufferLookup = state.GetBufferLookup<NavMeshCellTriangleIndexBufferElement>();

            this.singletonReady = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.singletonReady)
            {
                if (SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                {
                    this.triangles = SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);
                    this.verts = SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
                    this.navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();
                    this.singletonReady = true;
                }
                else
                    return;
            }

            this.cellLookup.Update(ref state);
            this.edgeCollisionBufferLookup.Update(ref state);
            this.triangleBufferLookup.Update(ref state);

            NativeList<Entity> entities = this.entityQuery
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();

            UpdateSizeJob updateSizeJob = new UpdateSizeJob(
                entities,
                this.cellLookup,
                this.edgeCollisionBufferLookup,
                this.triangleBufferLookup,
                this.triangles,
                this.verts);
            state.Dependency =
                updateSizeJob.Schedule(entities, entities.Length / SystemInfo.processorCount, state.Dependency);
            state.CompleteDependency();
            entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct UpdateSizeJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> verts;

        [NativeDisableParallelForRestriction] private ComponentLookup<NavMeshCellComponent> cellLookup;

        [NativeDisableParallelForRestriction]
        private BufferLookup<CellEdgeCollisionBufferElement> edgeCollisionBufferLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> triangleBufferLookup;


        public UpdateSizeJob(NativeList<Entity> entities,
            ComponentLookup<NavMeshCellComponent> cellLookup,
            BufferLookup<CellEdgeCollisionBufferElement> edgeCollisionBufferLookup,
            BufferLookup<NavMeshCellTriangleIndexBufferElement> triangleBufferLookup,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertBufferElement> verts)
        {
            this.entities = entities;
            this.cellLookup = cellLookup;
            this.edgeCollisionBufferLookup = edgeCollisionBufferLookup;
            this.triangleBufferLookup = triangleBufferLookup;
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.verts = verts.ToNativeArray(Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            NavMeshCellComponent cellComponent = this.cellLookup[entity];
            DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles = this.triangleBufferLookup[entity];
            DynamicBuffer<CellEdgeCollisionBufferElement> edgeCollisionBuffer = this.edgeCollisionBufferLookup[entity];

            int count = 0;

            for (int i = 0; i < cellComponent.TriangleSize; i++)
            {
                NavMeshCellTriangleIndexBufferElement cellTriangle = cellTriangles[i];
                NavTriangleBufferElement triangle = this.triangles[cellTriangle.Index];

                VertBufferElement a = this.verts[triangle.A], b = this.verts[triangle.B], c = this.verts[triangle.C];
                float3 ab = triangle.AB,
                    bc = triangle.BC,
                    ca = -triangle.AC;
                if (triangle.ABEdge)
                {
                    if (count < edgeCollisionBuffer.Length - 1)
                    {
                        edgeCollisionBuffer[count] = new CellEdgeCollisionBufferElement()
                        {
                            Start = a.Position,
                            End = b.Position,
                            StartOffset = -ca,
                            EndOffset = bc,
                            StartEnd = ab
                        };
                    }
                    else
                    {
                        edgeCollisionBuffer.Add(new CellEdgeCollisionBufferElement()
                        {
                            Start = a.Position,
                            End = b.Position,
                            StartOffset = -ca,
                            EndOffset = bc,
                            StartEnd = ab
                        });
                    }

                    count++;
                }

                if (triangle.ACEdge)
                {
                    if (count < edgeCollisionBuffer.Length - 1)
                    {
                        edgeCollisionBuffer[count] = new CellEdgeCollisionBufferElement()
                        {
                            Start = a.Position,
                            End = c.Position,
                            StartOffset = -bc,
                            EndOffset = ab,
                            StartEnd = ca
                        };
                    }
                    else
                    {
                        edgeCollisionBuffer.Add(new CellEdgeCollisionBufferElement()
                        {
                            Start = a.Position,
                            End = c.Position,
                            StartOffset = -bc,
                            EndOffset = ab,
                            StartEnd = ca
                        });
                    }

                    count++;
                }

                if (!triangle.BCEdge) continue;

                if (count < edgeCollisionBuffer.Length - 1)
                {
                    edgeCollisionBuffer[count] = new CellEdgeCollisionBufferElement()
                    {
                        Start = b.Position,
                        End = c.Position,
                        StartOffset = -ab,
                        EndOffset = ca,
                        StartEnd = bc
                    };
                }
                else
                {
                    edgeCollisionBuffer.Add(new CellEdgeCollisionBufferElement()
                    {
                        Start = b.Position,
                        End = c.Position,
                        StartOffset = -ab,
                        EndOffset = ca,
                        StartEnd = bc
                    });
                }

                count++;
            }
        }
    }
}