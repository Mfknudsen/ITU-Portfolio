using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [BurstCompile]
    internal partial struct MoveDirectionFromPathSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnitAgentComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeList<Entity> entities = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>())
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();
            SetDirectionJob setDirectionJob = new SetDirectionJob(
                entities,
                state.GetBufferLookup<WayPointBufferElement>(),
                state.GetComponentLookup<DestinationComponent>(),
                state.GetComponentLookup<LocalTransform>());

            state.Dependency =
                setDirectionJob.Schedule(entities, entities.Length / SystemInfo.processorCount, state.Dependency);
            state.CompleteDependency();
            entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct SetDirectionJob : IJobParallelForDefer
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction] private BufferLookup<WayPointBufferElement> funnelPathLookup;
        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationComponentLookup;
        [NativeDisableParallelForRestriction] private ComponentLookup<LocalTransform> transformComponentLookup;

        public SetDirectionJob(
            NativeList<Entity> entities,
            BufferLookup<WayPointBufferElement> funnelPathLookup,
            ComponentLookup<DestinationComponent> destinationComponentLookup,
            ComponentLookup<LocalTransform> transformComponentLookup)
        {
            this.entities = entities;

            this.funnelPathLookup = funnelPathLookup;
            this.destinationComponentLookup = destinationComponentLookup;
            this.transformComponentLookup = transformComponentLookup;
        }

        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            DestinationComponent destinationComponent = this.destinationComponentLookup[entity];

            if (destinationComponent.FunnelPathCount == 0)
                return;

            LocalTransform localTransform = this.transformComponentLookup[entity];
            DynamicBuffer<WayPointBufferElement> wayPointBufferElements = this.funnelPathLookup[entity];

            WayPointBufferElement current = wayPointBufferElements[destinationComponent.CurrentPathIndex];

            if (destinationComponent.CurrentPathIndex + 1 < destinationComponent.FunnelPathCount)
            {
                WayPointBufferElement next = wayPointBufferElements[destinationComponent.CurrentPathIndex + 1];

                if (localTransform.Position.QuickSquareDistance(next.Point) <
                    destinationComponent.Point.QuickSquareDistance(next.Point) &&
                    localTransform.Position.QuickSquareDistance(current.Point) >
                    destinationComponent.Point.QuickSquareDistance(current.Point))
                {
                    destinationComponent.CurrentPathIndex++;
                    current = next;
                }
            }

            if (current.IsWalk)
            {
                float3 currentMoveVector = current.Point - localTransform.Position;
                destinationComponent.MoveDirection = currentMoveVector.Normalize();
            }
            else
            {
                //NOT IMPLEMENTED
            }
        }
    }
}