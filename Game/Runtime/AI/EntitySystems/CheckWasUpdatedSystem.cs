using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup), OrderFirst = true)]
    [BurstCompile]
    internal partial struct CheckWasUpdatedSystem : ISystem
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
            NavigationMeshSingletonComponent navigationMeshSingletonComponent =
                SystemAPI.GetComponent<NavigationMeshSingletonComponent>(navmeshEntity);

            DynamicBuffer<VertWasUpdatedBufferElement> vertWasUpdatedBufferElements =
                SystemAPI.GetBuffer<VertWasUpdatedBufferElement>(navmeshEntity);

            DynamicBuffer<VertBufferElement> vertBufferElements =
                SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);

            NativeQueue<int> results = new NativeQueue<int>(Allocator.TempJob);
            results.AsParallelWriter();

            CheckVertWasUpdatedJob checkVertWasUpdatedJob =
                new CheckVertWasUpdatedJob(ref results, vertBufferElements);
            state.Dependency = checkVertWasUpdatedJob.Schedule(vertBufferElements.Length, 64, state.Dependency);
            state.Dependency.Complete();

            int count = 0;
            while (results.Count > 0)
            {
                int index = results.Dequeue();

                if (vertWasUpdatedBufferElements.Length > count)
                {
                    VertWasUpdatedBufferElement t = vertWasUpdatedBufferElements[count];
                    t.Index = index;
                    vertWasUpdatedBufferElements[count] = t;
                }
                else
                    vertWasUpdatedBufferElements.Add(new VertWasUpdatedBufferElement(index));

                count++;
            }

            navigationMeshSingletonComponent.VertsWasUpdatedSize = count;
            results.Clear();

            DynamicBuffer<NavTriangleBufferElement> triangles =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);

            CheckTriangleWasUpdateJob checkTriangleWasUpdateJob =
                new CheckTriangleWasUpdateJob(ref results, triangles, vertBufferElements);
            state.Dependency = checkTriangleWasUpdateJob.Schedule(triangles.Length, 64, state.Dependency);
            state.Dependency.Complete();

            count = 0;
            DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdateBufferElements =
                SystemAPI.GetBuffer<TriangleWasUpdatedBufferElement>(navmeshEntity);

            while (results.Count > 0)
            {
                int index = results.Dequeue();

                if (triangleWasUpdateBufferElements.Length > count)
                {
                    TriangleWasUpdatedBufferElement t = triangleWasUpdateBufferElements[count];
                    t.Index = index;
                    triangleWasUpdateBufferElements[count] = t;
                }
                else
                    triangleWasUpdateBufferElements.Add(new TriangleWasUpdatedBufferElement(index));

                count++;
            }

            navigationMeshSingletonComponent.TrianglesWasUpdatedSize = count;

            SystemAPI.SetComponent(navmeshEntity, navigationMeshSingletonComponent);
            results.Dispose();
        }
    }

    [BurstCompile]
    internal struct CheckVertWasUpdatedJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> simpleVerts;

        [WriteOnly] private NativeQueue<int>.ParallelWriter results;

        public CheckVertWasUpdatedJob(ref NativeQueue<int> results,
            DynamicBuffer<VertBufferElement> simpleVerts) : this()
        {
            this.results = results.AsParallelWriter();
            this.simpleVerts = simpleVerts.ToNativeArray(Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(int index)
        {
            if (this.simpleVerts[index].WasUpdated)
                this.results.Enqueue(index);
        }
    }

    [BurstCompile]
    internal struct CheckTriangleWasUpdateJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> verts;

        [WriteOnly] private NativeQueue<int>.ParallelWriter results;

        public CheckTriangleWasUpdateJob(ref NativeQueue<int> results,
            DynamicBuffer<NavTriangleBufferElement> triangles, DynamicBuffer<VertBufferElement> verts) : this()
        {
            this.results = results.AsParallelWriter();
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.verts = verts.ToNativeArray(Allocator.TempJob);
        }

        public void Execute(int index)
        {
            NavTriangleBufferElement triangle = this.triangles[index];

            if (this.verts[triangle.A].WasUpdated)
            {
                this.results.Enqueue(index);
                return;
            }

            if (this.verts[triangle.B].WasUpdated)
            {
                this.results.Enqueue(index);
                return;
            }

            if (!this.verts[triangle.C].WasUpdated)
                return;

            this.results.Enqueue(index);
        }
    }
}