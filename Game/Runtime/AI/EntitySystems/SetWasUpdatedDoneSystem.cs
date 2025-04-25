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
        private NavigationMeshSingletonComponent navmeshComponent;

        private DynamicBuffer<VertBufferElement> vertBufferElements;
        private DynamicBuffer<VertWasUpdatedBufferElement> vertWasUpdateBufferElements;

        private DynamicBuffer<NavTriangleBufferElement> triangleBufferElements;
        private DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdateBufferElements;

        private Entity navmeshEntity;

        private bool singletonReady;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.singletonReady = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.singletonReady)
            {
                if (SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out this.navmeshEntity))
                {
                    this.navmeshComponent =
                        SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

                    this.vertBufferElements =
                        SystemAPI.GetBuffer<VertBufferElement>(this.navmeshEntity);
                    this.vertWasUpdateBufferElements =
                        SystemAPI.GetBuffer<VertWasUpdatedBufferElement>(this.navmeshEntity);

                    this.triangleBufferElements =
                        SystemAPI.GetBuffer<NavTriangleBufferElement>(this.navmeshEntity);
                    this.triangleWasUpdateBufferElements =
                        SystemAPI.GetBuffer<TriangleWasUpdatedBufferElement>(this.navmeshEntity);

                    this.singletonReady = true;
                }
                else
                    return;
            }

            if (this.navmeshComponent.VertsWasUpdatedSize > 0)
            {
                SetVertWasUpdatedFalseJob setVertWasUpdatedFalseJob =
                    new SetVertWasUpdatedFalseJob(this.vertBufferElements, this.vertWasUpdateBufferElements);
                state.Dependency = setVertWasUpdatedFalseJob.Schedule(this.vertWasUpdateBufferElements.Length,
                    this.vertWasUpdateBufferElements.Length / SystemInfo.processorCount, state.Dependency);

                this.navmeshComponent.VertsWasUpdatedSize = 0;
            }

            if (this.navmeshComponent.TrianglesWasUpdatedSize > 0)
            {
                SetTriangleWasUpdatedFalseJob setTriangleWasUpdatedFalseJob =
                    new SetTriangleWasUpdatedFalseJob(this.triangleBufferElements,
                        this.triangleWasUpdateBufferElements);
                state.Dependency = setTriangleWasUpdatedFalseJob.Schedule(this.navmeshComponent.TrianglesWasUpdatedSize,
                    this.navmeshComponent.TrianglesWasUpdatedSize / SystemInfo.processorCount, state.Dependency);

                this.navmeshComponent.TrianglesWasUpdatedSize = 0;
            }

            state.CompleteDependency();
            SystemAPI.SetComponent(this.navmeshEntity, this.navmeshComponent);
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