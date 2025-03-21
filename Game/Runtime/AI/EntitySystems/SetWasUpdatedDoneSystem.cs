using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup), OrderLast = true)]
    internal partial struct SetWasUpdatedDoneSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity singletonEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();
            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            if (navmesh.TrianglesWasUpdatedSize > 0)
            {
                DynamicBuffer<VertBufferElement> vertBufferElements =
                    SystemAPI.GetBuffer<VertBufferElement>(singletonEntity);
                DynamicBuffer<VertWasUpdatedBufferElement> vertWasUpdateBufferElements =
                    SystemAPI.GetBuffer<VertWasUpdatedBufferElement>(singletonEntity);


                SetVertWasUpdatedFalseJob setVertWasUpdatedFalseJob =
                    new SetVertWasUpdatedFalseJob(vertBufferElements, vertWasUpdateBufferElements);
                state.Dependency = setVertWasUpdatedFalseJob.Schedule(vertWasUpdateBufferElements.Length,
                    vertWasUpdateBufferElements.Length / SystemInfo.processorCount, state.Dependency);

                navmesh.VertsWasUpdatedSize = 0;
            }

            if (navmesh.TrianglesWasUpdatedSize > 0)
            {
                DynamicBuffer<NavTriangleBufferElement> triangleBufferElements =
                    SystemAPI.GetBuffer<NavTriangleBufferElement>(singletonEntity);
                DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdateBufferElements =
                    SystemAPI.GetBuffer<TriangleWasUpdatedBufferElement>(singletonEntity);

                SetTriangleWasUpdatedFalseJob setTriangleWasUpdatedFalseJob =
                    new SetTriangleWasUpdatedFalseJob(triangleBufferElements, triangleWasUpdateBufferElements);
                state.Dependency = setTriangleWasUpdatedFalseJob.Schedule(navmesh.TrianglesWasUpdatedSize,
                    navmesh.TrianglesWasUpdatedSize / SystemInfo.processorCount, state.Dependency);

                navmesh.TrianglesWasUpdatedSize = 0;
            }
        }
    }

    [BurstCompile]
    internal struct SetVertWasUpdatedFalseJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] private DynamicBuffer<VertBufferElement> vertBufferElements;

        [NativeDisableParallelForRestriction]
        private DynamicBuffer<VertWasUpdatedBufferElement> vertWasUpdateBufferElements;

        public SetVertWasUpdatedFalseJob(DynamicBuffer<VertBufferElement> vertBufferElements,
            DynamicBuffer<VertWasUpdatedBufferElement> vertWasUpdateBufferElements)
        {
            this.vertBufferElements = vertBufferElements;
            this.vertWasUpdateBufferElements = vertWasUpdateBufferElements;
        }

        public void Execute(int index)
        {
            int wasUpdatedIndex = this.vertWasUpdateBufferElements[index].Index;
            VertBufferElement t = this.vertBufferElements[wasUpdatedIndex];
            t.WasUpdated = false;
            this.vertBufferElements[wasUpdatedIndex] = t;
        }
    }

    [BurstCompile]
    internal struct SetTriangleWasUpdatedFalseJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] private DynamicBuffer<NavTriangleBufferElement> triangleBufferElements;

        [NativeDisableParallelForRestriction]
        private DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdatedBufferElements;

        public SetTriangleWasUpdatedFalseJob(DynamicBuffer<NavTriangleBufferElement> triangleBufferElements,
            DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdatedBufferElements)
        {
            this.triangleBufferElements = triangleBufferElements;
            this.triangleWasUpdatedBufferElements = triangleWasUpdatedBufferElements;
        }

        public void Execute(int index)
        {
            int wasUpdatedIndex = this.triangleWasUpdatedBufferElements[index].Index;
            NavTriangleBufferElement t = this.triangleBufferElements[wasUpdatedIndex];
            t.WasUpdated = false;
            this.triangleBufferElements[wasUpdatedIndex] = t;
        }
    }
}