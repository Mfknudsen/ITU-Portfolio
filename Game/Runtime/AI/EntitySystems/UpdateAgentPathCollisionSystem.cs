using System;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [BurstCompile]
    internal partial struct UpdateAgentPathCollisionSystem : ISystem
    {
        private EntityQuery agentEntityQuery, cellEntityQuery;

        private ComponentLookup<NavMeshCellComponent> cellLookup;
        private BufferLookup<CellAgentCollisionIndexBufferElement> cellIndexBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.agentEntityQuery = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>());
            this.cellEntityQuery = state.GetEntityQuery(ComponentType.ReadOnly<NavMeshCellComponent>());

            this.cellLookup = state.GetComponentLookup<NavMeshCellComponent>();
            this.cellIndexBufferLookup = state.GetBufferLookup<CellAgentCollisionIndexBufferElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            NavigationMeshSingletonComponent navigationMeshSingletonComponent =
                SystemAPI.GetComponent<NavigationMeshSingletonComponent>(navmeshEntity);
            DynamicBuffer<AgentPathCollisionBufferElement> agentPathCollisionBufferElements =
                SystemAPI.GetBuffer<AgentPathCollisionBufferElement>(navmeshEntity);
            DynamicBuffer<AgentPathCollisionBufferElement> collisions =
                SystemAPI.GetBuffer<AgentPathCollisionBufferElement>(navmeshEntity);

            this.cellLookup.Update(ref state);
            this.cellIndexBufferLookup.Update(ref state);

            NativeArray<Entity> agentEntities = this.agentEntityQuery.ToEntityArray(Allocator.TempJob),
                cellEntities = this.cellEntityQuery.ToEntityArray(Allocator.TempJob);

            if (agentEntities.Length == 0)
                return;

            if (agentPathCollisionBufferElements.Length < agentEntities.Length)
            {
                agentPathCollisionBufferElements.AddRange(
                    new NativeArray<AgentPathCollisionBufferElement>(
                        agentEntities.Length - agentPathCollisionBufferElements.Length, Allocator.Temp));
            }

            UpdateAgentCollisionJob updateAgentCollisionJob = new UpdateAgentCollisionJob(
                agentEntities, agentPathCollisionBufferElements,
                state.GetComponentLookup<UnitAgentComponent>(true),
                state.GetComponentLookup<AgentSettingsComponent>(true));
            state.Dependency = updateAgentCollisionJob.Schedule(agentEntities.Length / SystemInfo.processorCount,
                state.Dependency);

            AgentCollisionsInCellJob agentCollisionsInCellJob = new AgentCollisionsInCellJob(
                cellEntities, this.cellLookup, this.cellIndexBufferLookup,
                collisions,
                navigationMeshSingletonComponent.GroupDivision,
                navigationMeshSingletonComponent.MinFloorX,
                navigationMeshSingletonComponent.MinFloorZ);
            state.Dependency = agentCollisionsInCellJob.Schedule(agentEntities.Length / SystemInfo.processorCount,
                state.Dependency);
        }
    }

    [BurstCompile]
    internal struct UpdateAgentCollisionJob : IJobParallelForDefer, IJobFor, IDisposable
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction]
        private DynamicBuffer<AgentPathCollisionBufferElement> collisionBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<UnitAgentComponent> agentLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<AgentSettingsComponent> settingsLookup;

        public UpdateAgentCollisionJob(NativeArray<Entity> entities,
            DynamicBuffer<AgentPathCollisionBufferElement> collisionBufferElements,
            ComponentLookup<UnitAgentComponent> agentLookup, ComponentLookup<AgentSettingsComponent> settingsLookup)
        {
            this.entities = entities;
            this.collisionBufferElements = collisionBufferElements;
            this.agentLookup = agentLookup;
            this.settingsLookup = settingsLookup;
        }

        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            UnitAgentComponent agentComponent = this.agentLookup[entity];
            AgentSettingsComponent settingsComponent = this.settingsLookup[entity];

            this.collisionBufferElements[index] = new AgentPathCollisionBufferElement
            {
                Position = agentComponent.Position,
                Height = settingsComponent.Height,
                Radius = settingsComponent.Radius
            };
        }

        public void Dispose()
        {
            this.entities.Dispose();
        }
    }

    [BurstCompile]
    internal struct AgentCollisionsInCellJob : IJobParallelForDefer, IJobFor, IDisposable
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction] private ComponentLookup<NavMeshCellComponent> cellLookup;

        [NativeDisableParallelForRestriction]
        private BufferLookup<CellAgentCollisionIndexBufferElement> cellIndexBufferLookup;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<AgentPathCollisionBufferElement> collisions;

        private readonly float cellSize, xStart, zStart;

        public AgentCollisionsInCellJob(NativeArray<Entity> entities,
            ComponentLookup<NavMeshCellComponent> cellLookup,
            BufferLookup<CellAgentCollisionIndexBufferElement> cellIndexBufferLookup,
            DynamicBuffer<AgentPathCollisionBufferElement> collisions, float cellSize, float xStart, float zStart)
        {
            this.entities = entities;
            this.cellLookup = cellLookup;
            this.cellIndexBufferLookup = cellIndexBufferLookup;
            this.collisions = collisions.ToNativeArray(Allocator.TempJob);
            this.cellSize = cellSize;

            this.xStart = xStart;
            this.zStart = zStart;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            int listIndex = 0;
            Entity entity = this.entities[index];
            NavMeshCellComponent cellComponent = this.cellLookup[entity];
            DynamicBuffer<CellAgentCollisionIndexBufferElement> cellBuffer = this.cellIndexBufferLookup[entity];

            float xMin = cellComponent.X * this.cellSize + this.xStart,
                zMin = cellComponent.Z * this.cellSize + this.zStart,
                xMax = xMin + this.cellSize,
                zMax = zMin + this.cellSize;

            for (int i = 0; i < this.collisions.Length; i++)
            {
                AgentPathCollisionBufferElement agentPathCollisionBufferElement = this.collisions[i];
                float3 pos = agentPathCollisionBufferElement.Position;

                if (pos.x < xMin || pos.z < zMin || pos.x > xMax || pos.z > zMax)
                    continue;

                if (listIndex < cellBuffer.Length)
                {
                    cellBuffer[listIndex] = new CellAgentCollisionIndexBufferElement { Index = i };
                }
                else
                    cellBuffer.Add(new CellAgentCollisionIndexBufferElement { Index = i });

                listIndex++;
            }
        }

        public void Dispose()
        {
            this.entities.Dispose();
        }
    }
}