using Runtime.AI.EntityAspects;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    public partial struct UpdateCellsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Entity singletonEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();
            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            DynamicBuffer<NavTriangleBufferElement> navTriangleBufferElements =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(singletonEntity);
            DynamicBuffer<VertXZBufferElement> vertXZBufferElements =
                SystemAPI.GetBuffer<VertXZBufferElement>(singletonEntity);

            UpdateCellTrianglesJob updateCellTrianglesJob = new UpdateCellTrianglesJob(
                navmesh.GroupDivision,
                navmesh.MinFloorX,
                navmesh.MinFloorZ,
                vertXZBufferElements.ToNativeArray(Allocator.TempJob),
                navTriangleBufferElements.ToNativeArray(Allocator.TempJob));
            JobHandle updateCellTrianglesHandle = updateCellTrianglesJob.ScheduleParallel(state.Dependency);

            TriangleCellNeedUpdateChangeJob triangleCellNeedUpdateChangeJob =
                new TriangleCellNeedUpdateChangeJob(ref navTriangleBufferElements);
            JobHandle triangleCellNeedUpdateChangeHandle =
                triangleCellNeedUpdateChangeJob.Schedule(navTriangleBufferElements.Length, 128,
                    updateCellTrianglesHandle);

            state.Dependency = triangleCellNeedUpdateChangeHandle;
            triangleCellNeedUpdateChangeHandle.Complete();

            DynamicBuffer<TriangleFlattenSizeBufferElement> sizeBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenSizeBufferElement>(singletonEntity);
            DynamicBuffer<TriangleFlattenStartIndexBufferElement> startIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenStartIndexBufferElement>(singletonEntity);
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(singletonEntity);

            sizeBufferElements.Clear();
            startIndexBufferElements.Clear();
            triangleIndexBufferElements.Clear();
            sizeBufferElements.Resize(navmesh.CellXLength *
                                      navmesh.CellZLength, NativeArrayOptions.ClearMemory);
            startIndexBufferElements.Resize(sizeBufferElements.Length, NativeArrayOptions.ClearMemory);

            for (int i = 0; i < navmesh.CellXLength * navmesh.CellZLength; i++)
            {
                sizeBufferElements.Add(new TriangleFlattenSizeBufferElement());
                startIndexBufferElements.Add(new TriangleFlattenStartIndexBufferElement());
            }

            foreach (UpdateCellAspect aspect in SystemAPI.Query<UpdateCellAspect>())
            {
                int index = aspect.CellComponent.ValueRO.X * navmesh.CellXLength + aspect.CellComponent.ValueRO.Z;

                TriangleFlattenSizeBufferElement triangleFlattenSizeElement = sizeBufferElements[index];
                triangleFlattenSizeElement.Size = aspect.NavTriangleBufferElements.Length;
                sizeBufferElements[index] = triangleFlattenSizeElement;

                int startIndex = index > 0 ? startIndexBufferElements[index].Index - 1 : 0;
                foreach (NavMeshCellTriangleIndexBufferElement triangleElement in aspect.NavTriangleBufferElements)
                {
                    if (startIndex >= triangleIndexBufferElements.Length)
                        triangleIndexBufferElements.Add(
                            new TriangleFlattenIndexBufferElement { Index = triangleElement.Index });
                    else
                        triangleIndexBufferElements.Insert(startIndex,
                            new TriangleFlattenIndexBufferElement { Index = triangleElement.Index });
                }

                for (int i = startIndexBufferElements.Length - 1; i > index; i--)
                {
                    TriangleFlattenStartIndexBufferElement triangleFlattenStartIndexElement =
                        startIndexBufferElements[i];
                    triangleFlattenStartIndexElement.Index += triangleFlattenSizeElement.Size;
                    startIndexBufferElements[i] = triangleFlattenStartIndexElement;
                }
            }
        }
    }
}