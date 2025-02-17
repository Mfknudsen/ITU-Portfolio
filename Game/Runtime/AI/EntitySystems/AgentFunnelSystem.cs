using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    internal partial struct AgentFunnelSystem : ISystem
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

            DynamicBuffer<VertYBufferElement> vertsY =
                SystemAPI.GetBuffer<VertYBufferElement>(navmeshEntity);
            DynamicBuffer<VertXZBufferElement> simpleVerts =
                SystemAPI.GetBuffer<VertXZBufferElement>(navmeshEntity);
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            AgentFunnelJob agentFunnelJob = new AgentFunnelJob(
                triangles,
                simpleVerts,
                vertsY
            );
            state.Dependency = agentFunnelJob.ScheduleParallel(state.Dependency);
        }
    }
}