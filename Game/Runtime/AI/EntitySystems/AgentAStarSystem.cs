using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public partial struct AgentAStarSystem : ISystem
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

            DynamicBuffer<VertYBufferElement> vertsY = SystemAPI.GetBuffer<VertYBufferElement>(navmeshEntity);
            DynamicBuffer<VertXZBufferElement> simpleVerts = SystemAPI.GetBuffer<VertXZBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            AgentAStarJob pathingJob = new AgentAStarJob(vertsY, simpleVerts, triangles,
                SystemAPI.GetBuffer<AreaBufferElement>(navmeshEntity)
            );
            JobHandle pathingHandle = pathingJob.ScheduleParallel(state.Dependency);
            state.Dependency = pathingHandle;
        }
    }
}