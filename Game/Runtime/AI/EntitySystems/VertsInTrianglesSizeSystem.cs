using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    internal partial struct VertsInTrianglesSizeSystem : ISystem
    {
        private DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements;
        private DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements;
        private DynamicBuffer<VertBufferElement> simpleVerts;
        private DynamicBuffer<NavTriangleBufferElement> triangles;

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

            this.sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);
            this.vertInTrianglesFlattenBufferElements =
                SystemAPI.GetBuffer<VertInTrianglesFlattenBufferElement>(navmeshEntity);
            this.simpleVerts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
            this.triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            if (this.sizeBufferElements.Length == 0)
                return;

            if (this.vertInTrianglesFlattenBufferElements.Length < this.simpleVerts.Length)
            {
                this.vertInTrianglesFlattenBufferElements.AddRange(
                    new NativeArray<VertInTrianglesFlattenBufferElement>(
                        this.simpleVerts.Length - this.vertInTrianglesFlattenBufferElements.Length, Allocator.Temp));
            }
            else if (this.vertInTrianglesFlattenBufferElements.Length > this.simpleVerts.Length)
            {
                this.vertInTrianglesFlattenBufferElements.RemoveRange(this.simpleVerts.Length,
                    this.vertInTrianglesFlattenBufferElements.Length - this.simpleVerts.Length);
            }

            for (int i = 0; i < this.vertInTrianglesFlattenBufferElements.Length; i++)
            {
                VertInTrianglesFlattenBufferElement vertInTrianglesFlattenBufferElement =
                    this.vertInTrianglesFlattenBufferElements[i];
                vertInTrianglesFlattenBufferElement.Size = 0;
                this.vertInTrianglesFlattenBufferElements[i] = vertInTrianglesFlattenBufferElement;
            }

            VertsInTrianglesSizeJob vertsInTrianglesSizeJob = new VertsInTrianglesSizeJob(
                this.triangles,
                this.vertInTrianglesFlattenBufferElements);
            state.Dependency =
                vertsInTrianglesSizeJob.Schedule(this.triangles.Length, 64, state.Dependency);
            state.CompleteDependency();
        }
    }

    [BurstCompile]
    internal struct VertsInTrianglesSizeJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] private readonly NativeArray<NavTriangleBufferElement> triangles;

        [NativeDisableParallelForRestriction]
        private DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements;

        public VertsInTrianglesSizeJob(DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements)
        {
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.vertInTrianglesFlattenBufferElements = vertInTrianglesFlattenBufferElements;
        }

        [BurstCompile]
        public void Execute(int triangleIndex)
        {
            NavTriangleBufferElement navTriangleBufferElement = this.triangles[triangleIndex];

            VertInTrianglesFlattenBufferElement vertInTrianglesFlattenBufferElement =
                this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.A];
            vertInTrianglesFlattenBufferElement.Size++;
            this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.A] = vertInTrianglesFlattenBufferElement;

            vertInTrianglesFlattenBufferElement =
                this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.B];
            vertInTrianglesFlattenBufferElement.Size++;
            this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.B] = vertInTrianglesFlattenBufferElement;

            vertInTrianglesFlattenBufferElement =
                this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.C];
            vertInTrianglesFlattenBufferElement.Size++;
            this.vertInTrianglesFlattenBufferElements[navTriangleBufferElement.C] = vertInTrianglesFlattenBufferElement;
        }
    }
}