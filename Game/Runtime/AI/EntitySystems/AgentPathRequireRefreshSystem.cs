using Unity.Burst;
using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    internal partial struct AgentPathRequireRefreshSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }
    }
}