using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [UpdateAfter(typeof(AgentInTriangleSystem))]
    [UpdateBefore(typeof(AgentAStarSystem))]
    internal partial struct AgentPathRequireRefreshSystem : ISystem
    {
        private EntityQuery entityQuery;

        private ComponentLookup<UnitAgentComponent> agentLookup;
        private ComponentLookup<DestinationComponent> destinationLookup;
        private BufferLookup<AgentTrianglePathBufferElement> trianglePathBuffer;

        private DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements;

        private bool singletonReady;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<UnitAgentComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>());

            this.agentLookup = state.GetComponentLookup<UnitAgentComponent>(true);
            this.destinationLookup = state.GetComponentLookup<DestinationComponent>();
            this.trianglePathBuffer = state.GetBufferLookup<AgentTrianglePathBufferElement>(true);

            this.singletonReady = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.singletonReady)
            {
                if (SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                {
                    this.sizeBufferElements =
                        SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);

                    this.singletonReady = true;
                }
                else
                    return;
            }

            this.agentLookup.Update(ref state);
            this.destinationLookup.Update(ref state);
            this.trianglePathBuffer.Update(ref state);

            if (this.sizeBufferElements.Length == 0)
                return;

            NativeList<Entity> entities = this.entityQuery
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();

            CheckAgentStillInPath checkAgentStillInPath = new CheckAgentStillInPath(
                entities,
                this.agentLookup,
                this.destinationLookup,
                this.trianglePathBuffer);
            state.Dependency = checkAgentStillInPath.Schedule(entities, entities.Length / SystemInfo.processorCount,
                state.Dependency);
            state.CompleteDependency();
            entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct CheckAgentStillInPath : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<UnitAgentComponent> agentLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationLookup;
        [NativeDisableParallelForRestriction] private BufferLookup<AgentTrianglePathBufferElement> trianglePathBuffer;

        public CheckAgentStillInPath(
            NativeList<Entity> entities,
            ComponentLookup<UnitAgentComponent> agentLookup,
            ComponentLookup<DestinationComponent> destinationLookup,
            BufferLookup<AgentTrianglePathBufferElement> trianglePathBuffer)
        {
            this.entities = entities;
            this.agentLookup = agentLookup;
            this.destinationLookup = destinationLookup;
            this.trianglePathBuffer = trianglePathBuffer;
        }

        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            UnitAgentComponent agentComponent = this.agentLookup[entity];
            DestinationComponent destinationComponent = this.destinationLookup[entity];
            DynamicBuffer<AgentTrianglePathBufferElement> pathBuffer = this.trianglePathBuffer[entity];

            if (agentComponent.CurrentTriangleID == -1 || destinationComponent.TrianglePathCount == 0)
            {
                destinationComponent.Refresh = true;
                this.destinationLookup[entity] = destinationComponent;
            }
            else
            {
                for (int i = 0; i < destinationComponent.TrianglePathCount; i++)
                {
                    if (pathBuffer[i].Index == agentComponent.CurrentTriangleID)
                        return;
                }

                destinationComponent.Refresh = true;
                this.destinationLookup[entity] = destinationComponent;
            }
        }
    }
}