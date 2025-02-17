using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    public partial struct AgentDebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            state.RequireForUpdate<AgentSettingsComponent>();
            state.RequireForUpdate<LocalTransform>();
            state.RequireForUpdate<WayPointBufferElement>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalTransform>, DynamicBuffer<WayPointBufferElement>) valueTuple in SystemAPI
                         .Query<RefRO<LocalTransform>, DynamicBuffer<WayPointBufferElement>>())
            {
                if (valueTuple.Item2.Length == 0)
                    continue;

                Debug.DrawLine(valueTuple.Item1.ValueRO.Position + new float3(0, .5f, 0),
                    valueTuple.Item2[0].Point + new float3(0, .5f, 0), Color.red);

                for (int i = 1; i < valueTuple.Item2.Length; i++)
                {
                    Debug.DrawLine(valueTuple.Item2[i - 1].Point + new float3(0, .5f, 0),
                        valueTuple.Item2[i].Point + new float3(0, .5f, 0), Color.red);
                }
            }
        }
    }
}