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
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity navmeshEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();

            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements =
                SystemAPI.GetBuffer<VertInTrianglesFlattenBufferElement>(navmeshEntity);

            DynamicBuffer<VertBufferElement> simpleVerts =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);

            if (vertInTrianglesFlattenBufferElements.Length < simpleVerts.Length)
            {
                vertInTrianglesFlattenBufferElements.AddRange(
                    new NativeArray<VertInTrianglesFlattenBufferElement>(
                        simpleVerts.Length - vertInTrianglesFlattenBufferElements.Length, Allocator.Temp));
            }
            else if (vertInTrianglesFlattenBufferElements.Length > simpleVerts.Length)
            {
                vertInTrianglesFlattenBufferElements.RemoveRange(simpleVerts.Length,
                    vertInTrianglesFlattenBufferElements.Length - simpleVerts.Length);
            }

            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            for (int i = 0; i < vertInTrianglesFlattenBufferElements.Length; i++)
            {
                VertInTrianglesFlattenBufferElement vertInTrianglesFlattenBufferElement =
                    vertInTrianglesFlattenBufferElements[i];
                vertInTrianglesFlattenBufferElement.Size = 0;
                vertInTrianglesFlattenBufferElements[i] = vertInTrianglesFlattenBufferElement;
            }

            VertsInTrianglesSizeJob vertsInTrianglesSizeJob = new VertsInTrianglesSizeJob(
                triangles,
                vertInTrianglesFlattenBufferElements);
            state.Dependency =
                vertsInTrianglesSizeJob.Schedule(triangles.Length, 64, state.Dependency);
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