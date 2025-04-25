using System;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [UpdateAfter(typeof(AgentFunnelSystem))]
    [BurstCompile]
    internal partial struct UpdateMoveDirectionSystem : ISystem
    {
        private EntityQuery agentQuery;

        private BufferLookup<WayPointBufferElement> funnelPathLookup;
        private ComponentLookup<DestinationComponent> destinationComponentLookup;
        private ComponentLookup<UnitAgentComponent> agentComponentLookup;
        private ComponentLookup<AgentSettingsComponent> settingsComponentLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
            state.RequireForUpdate<UnitAgentComponent>();

            this.agentQuery = state.GetEntityQuery(ComponentType.ReadOnly<UnitAgentComponent>(),
                ComponentType.ReadOnly<DestinationComponent>());

            this.funnelPathLookup = state.GetBufferLookup<WayPointBufferElement>();
            this.destinationComponentLookup = state.GetComponentLookup<DestinationComponent>();
            this.agentComponentLookup = state.GetComponentLookup<UnitAgentComponent>();
            this.settingsComponentLookup = state.GetComponentLookup<AgentSettingsComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                return;

            this.funnelPathLookup.Update(ref state);
            this.destinationComponentLookup.Update(ref state);
            this.agentComponentLookup.Update(ref state);
            this.settingsComponentLookup.Update(ref state);

            NavigationMeshSingletonComponent navmeshSingletonComponent =
                SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();
            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);
            DynamicBuffer<EdgeCollisionBufferElement> edgeCollisionBuffer =
                SystemAPI.GetBuffer<EdgeCollisionBufferElement>(navmeshEntity);
            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);

            NativeArray<Entity> entities = this.agentQuery
                .ToEntityArray(Allocator.TempJob);

            if (entities.Length == 0)
            {
                entities.Dispose();
                return;
            }

            int batch = math.max(entities.Length / SystemInfo.processorCount, 1);

            MoveJob moveJob = new MoveJob(
                entities,
                this.funnelPathLookup,
                this.destinationComponentLookup,
                this.agentComponentLookup,
                this.settingsComponentLookup,
                edgeCollisionBuffer,
                sizeBufferElements,
                triangleIndexBufferElements,
                triangles,
                navmeshSingletonComponent.GroupDivision,
                navmeshSingletonComponent.MinFloorX,
                navmeshSingletonComponent.MinFloorZ,
                navmeshSingletonComponent.CellXLength,
                navmeshSingletonComponent.CellZLength,
                Time.deltaTime);

            JobHandle handle =
                moveJob.ScheduleParallel(entities.Length, batch, state.Dependency);
            state.Dependency = handle;
            state.Dependency = entities.Dispose(handle);
        }
    }

    [BurstCompile]
    internal struct MoveJob : IJobFor, IDisposable
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction] private BufferLookup<WayPointBufferElement> funnelPathLookup;
        [NativeDisableParallelForRestriction] private ComponentLookup<DestinationComponent> destinationComponentLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<UnitAgentComponent> agentComponentLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<AgentSettingsComponent> settingsComponentLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<EdgeCollisionBufferElement> edgeCollisions;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<NavTriangleBufferElement> triangles;

        private readonly float deltaTime, maxDetectAngle;

        private readonly float3 rotationAxis;

        private readonly int raycastCount;

        private readonly float groupDivision;

        private readonly int cellXLength, cellZLength;
        private readonly float minFloorX, minFloorZ;

        public MoveJob(
            NativeArray<Entity> entities,
            BufferLookup<WayPointBufferElement> funnelPathLookup,
            ComponentLookup<DestinationComponent> destinationComponentLookup,
            ComponentLookup<UnitAgentComponent> agentComponentLookup,
            ComponentLookup<AgentSettingsComponent> settingsComponentLookup,
            DynamicBuffer<EdgeCollisionBufferElement> edgeCollisions,
            DynamicBuffer<TriangleFlattenBufferElement> sizeBufferElements,
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            float groupDivision, float minFloorX, float minFloorZ, int cellXLength, int cellZLength,
            float deltaTime)
        {
            this.entities = entities;

            this.funnelPathLookup = funnelPathLookup;
            this.destinationComponentLookup = destinationComponentLookup;
            this.agentComponentLookup = agentComponentLookup;
            this.settingsComponentLookup = settingsComponentLookup;
            this.edgeCollisions = edgeCollisions;
            this.sizeBufferElements = sizeBufferElements;
            this.triangleIndexBufferElements = triangleIndexBufferElements;
            this.triangles = triangles;
            this.deltaTime = deltaTime;
            this.rotationAxis = new float3(0, 1, 0);
            this.maxDetectAngle = 45;
            this.raycastCount = 10;
            this.groupDivision = groupDivision;
            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;
            this.cellXLength = cellXLength;
            this.cellZLength = cellZLength;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            DestinationComponent destinationComponent = this.destinationComponentLookup[entity];

            if (destinationComponent.FunnelPathCount == 0)
                return;

            UnitAgentComponent agentComponent = this.agentComponentLookup[entity];
            AgentSettingsComponent settingsComponent = this.settingsComponentLookup[entity];
            DynamicBuffer<WayPointBufferElement> wayPointBufferElements = this.funnelPathLookup[entity];

            WayPointBufferElement current = wayPointBufferElements[destinationComponent.CurrentPathIndex];

            if (destinationComponent.Debug)
            {
                for (int i = 1; i < destinationComponent.FunnelPathCount; i++)
                {
                    //Debug.DrawLine(wayPointBufferElements[i - 1].Point, wayPointBufferElements[i].Point, Color.green);
                }
            }

            if (current.IsWalk)
            {
                float3 currentMoveVector = current.Point - agentComponent.Position;
                destinationComponent.MoveDirection = currentMoveVector.Normalize();
            }
            else
            {
                //TODO: NOT IMPLEMENTED YET
            }

            //Debug.DrawRay(current.Point, Vector3.up, Color.red);

            agentComponent.Position +=
                destinationComponent.MoveDirection * (settingsComponent.MoveSpeed * this.deltaTime);
            //Debug.DrawRay(agentComponent.Position + new float3(0, 0.5f, 0), destinationComponent.MoveDirection);

            NativeList<TriangleFlattenIndexBufferElement> triangleIds =
                new NativeList<TriangleFlattenIndexBufferElement>(64, Allocator.Temp);
            int triangleIdSize = Common.GetTriangleIdsByPosition(ref triangleIds, agentComponent.Position,
                this.cellXLength, this.cellZLength, this.minFloorX, this.minFloorZ, this.groupDivision,
                this.triangleIndexBufferElements, this.sizeBufferElements);

            for (int i = 0; i < triangleIdSize; i++)
            {
                int triangleIndex = triangleIds[i].Index;
                NavTriangleBufferElement triangle = this.triangles[triangleIndex];

                for (int j = 0; j < triangle.EdgeCount(); j++)
                {
                    EdgeCollisionBufferElement edge = this.edgeCollisions[triangleIndex * 3 + j];
                    //Debug.DrawLine(edge.Start + math.up(), edge.End + math.up(), Color.magenta);
                }
            }

            this.CastRay(agentComponent.Position, destinationComponent.MoveDirection, 0);
            float dividedAngle = this.maxDetectAngle / (this.raycastCount / 2f);
            for (int i = 0; i < this.raycastCount / 2; i++)
            {
                this.CastRay(agentComponent.Position, destinationComponent.MoveDirection,
                    dividedAngle + dividedAngle * i);
                this.CastRay(agentComponent.Position, destinationComponent.MoveDirection,
                    -(dividedAngle + dividedAngle * i));
            }

            //Check if it should go to next
            if (destinationComponent.CurrentPathIndex + 1 < destinationComponent.FunnelPathCount)
            {
                WayPointBufferElement next = wayPointBufferElements[destinationComponent.CurrentPathIndex + 1];

                if (next.Point.QuickSquareDistance(agentComponent.Position) <
                    next.Point.QuickSquareDistance(current.Point) &&
                    current.Point.QuickSquareDistance(agentComponent.Position) < .25f)
                    destinationComponent.CurrentPathIndex++;
            }

            this.destinationComponentLookup[entity] = destinationComponent;
            this.agentComponentLookup[entity] = agentComponent;
        }

        [BurstCompile]
        private void CastRay(float3 pos, float3 moveDirection, float angle)
        {
            float3 direction = Quaternion.AngleAxis(angle, Vector3.up) * moveDirection;

            //Debug.DrawRay(pos + this.rotationAxis, direction);
        }

        public void Dispose()
        {
            this.entities.Dispose();
        }
    }
}