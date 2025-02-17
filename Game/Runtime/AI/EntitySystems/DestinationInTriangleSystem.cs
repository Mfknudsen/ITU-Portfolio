using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    internal partial struct DestinationInTriangleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity navmeshEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();

            NavigationMeshSingletonComponent navmeshSingletonComponent =
                SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            DynamicBuffer<TriangleFlattenSizeBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenSizeBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            DynamicBuffer<TriangleFlattenStartIndexBufferElement> startIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenStartIndexBufferElement>(navmeshEntity);

            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);

            DynamicBuffer<VertYBufferElement> vertsY =
                SystemAPI.GetBuffer<VertYBufferElement>(navmeshEntity);
            DynamicBuffer<VertXZBufferElement> simpleVerts =
                SystemAPI.GetBuffer<VertXZBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            DestinationInTriangleJob destinationInTriangleJob = new DestinationInTriangleJob(
                navmeshSingletonComponent.GroupDivision,
                navmeshSingletonComponent.MinFloorX,
                navmeshSingletonComponent.MinFloorZ,
                navmeshSingletonComponent.CellXLength,
                navmeshSingletonComponent.CellZLength,
                vertsY, simpleVerts, triangles,
                sizeBufferElements, startIndexBufferElements, triangleIndexBufferElements);
            state.Dependency = destinationInTriangleJob.ScheduleParallel(state.Dependency);
        }
    }
}