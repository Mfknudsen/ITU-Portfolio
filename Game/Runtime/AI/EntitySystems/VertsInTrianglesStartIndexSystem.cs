using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [UpdateAfter(typeof(VertsInTrianglesSizeSystem))]
    internal partial struct VertsInTrianglesStartIndexSystem : ISystem
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

            DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements =
                SystemAPI.GetBuffer<VertInTrianglesFlattenBufferElement>(navmeshEntity);


            for (int i = 1; i < vertInTrianglesFlattenBufferElements.Length; i++)
            {
                VertInTrianglesFlattenBufferElement element = vertInTrianglesFlattenBufferElements[i];
                element.StartIndex = vertInTrianglesFlattenBufferElements[i - 1].StartIndex +
                                     vertInTrianglesFlattenBufferElements[i - 1].Size;
                vertInTrianglesFlattenBufferElements[i] = element;
            }

            return;

            VertsInTrianglesStartIndexJob vertsInTrianglesStartIndexJob =
                new VertsInTrianglesStartIndexJob(vertInTrianglesFlattenBufferElements);
            state.Dependency = vertsInTrianglesStartIndexJob.Schedule(vertInTrianglesFlattenBufferElements.Length,
                64,
                state.Dependency);
        }
    }

    [BurstCompile]
    public struct VertsInTrianglesStartIndexJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        private DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements;

        public VertsInTrianglesStartIndexJob(
            DynamicBuffer<VertInTrianglesFlattenBufferElement> vertInTrianglesFlattenBufferElements)
        {
            this.vertInTrianglesFlattenBufferElements = vertInTrianglesFlattenBufferElements;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            int startIndex = 0;
            for (int i = 0; i < index; i++)
                startIndex += this.vertInTrianglesFlattenBufferElements[i].Size;

            VertInTrianglesFlattenBufferElement vertInTrianglesFlattenBufferElement =
                this.vertInTrianglesFlattenBufferElements[index];
            vertInTrianglesFlattenBufferElement.StartIndex = startIndex;
            this.vertInTrianglesFlattenBufferElements[index] = vertInTrianglesFlattenBufferElement;
        }
    }
}