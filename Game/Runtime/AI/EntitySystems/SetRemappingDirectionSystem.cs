using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    internal partial struct SetRemappingDirectionSystem : ISystem
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

            DynamicBuffer<TriangleFlattenSizeBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenSizeBufferElement>(navmeshEntity);

            if (sizeBufferElements.Length == 0)
                return;

            DynamicBuffer<TriangleFlattenStartIndexBufferElement> startIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenStartIndexBufferElement>(navmeshEntity);

            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);

            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            DynamicBuffer<VertYBufferElement> vertsY =
                SystemAPI.GetBuffer<VertYBufferElement>(navmeshEntity);
            DynamicBuffer<VertXZBufferElement> simpleVerts =
                SystemAPI.GetBuffer<VertXZBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            SetRemappingDirectionJob setRemappingDirectionJob = new SetRemappingDirectionJob(
                navmesh.GroupDivision,
                navmesh.MinFloorX,
                navmesh.MinFloorZ,
                navmesh.CellXLength,
                navmesh.CellZLength,
                triangles,
                simpleVerts,
                vertsY,
                sizeBufferElements,
                startIndexBufferElements,
                triangleIndexBufferElements);

            state.Dependency = setRemappingDirectionJob.Schedule(vertsY.Length, 128, state.Dependency);
        }
    }
}