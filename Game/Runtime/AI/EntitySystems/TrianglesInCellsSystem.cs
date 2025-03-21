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
    public partial struct TrianglesInCellsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity singletonEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();
            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            Debug.Log(navmesh.TrianglesWasUpdatedSize);
            if (navmesh.TrianglesWasUpdatedSize == 0)
                return;

            DynamicBuffer<NavTriangleBufferElement> navTriangleBufferElements =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(singletonEntity);
            DynamicBuffer<VertBufferElement> vertBufferElements =
                SystemAPI.GetBuffer<VertBufferElement>(singletonEntity);
            DynamicBuffer<TriangleWasUpdatedBufferElement> wasUpdateBufferElements =
                SystemAPI.GetBuffer<TriangleWasUpdatedBufferElement>(singletonEntity);


            NativeList<Entity> entities = state.GetEntityQuery(ComponentType.ReadOnly<NavMeshCellComponent>())
                .ToEntityListAsync(Allocator.TempJob, state.Dependency, out JobHandle handle);
            handle.Complete();

            UpdateCellTriangleIndexListsJob updateCellTriangleIndexListsJob = new UpdateCellTriangleIndexListsJob(
                entities,
                navmesh.GroupDivision,
                navmesh.MinFloorX,
                navmesh.MinFloorZ,
                vertBufferElements,
                navTriangleBufferElements,
                wasUpdateBufferElements,
                state.GetBufferLookup<NavMeshCellTriangleIndexBufferElement>(),
                state.GetComponentLookup<NavMeshCellComponent>());
            state.Dependency =
                updateCellTriangleIndexListsJob.Schedule(entities, entities.Length / SystemInfo.processorCount,
                    state.Dependency);
            state.CompleteDependency();

            TriangleCellNeedUpdateChangeJob triangleCellNeedUpdateChangeJob =
                new TriangleCellNeedUpdateChangeJob(ref navTriangleBufferElements);
            JobHandle triangleCellNeedUpdateChangeHandle =
                triangleCellNeedUpdateChangeJob.Schedule(navTriangleBufferElements.Length,
                    navTriangleBufferElements.Length / SystemInfo.processorCount,
                    state.Dependency);

            triangleCellNeedUpdateChangeHandle.Complete();

            DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenBufferElement>(singletonEntity);
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements =
                SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(singletonEntity);

            int t = navmesh.CellXLength * navmesh.CellZLength;
            if (triangleFlattenBufferElements.Length < t)
                triangleFlattenBufferElements.AddRange(
                    new NativeArray<TriangleFlattenBufferElement>(t - triangleFlattenBufferElements.Length,
                        Allocator.Temp));

            UpdateCellFlattenSizeJob updateCellFlattenSizeJob =
                new UpdateCellFlattenSizeJob(triangleFlattenBufferElements, navmesh.CellXLength);
            JobHandle updateCellSizeAndStartHandle =
                updateCellFlattenSizeJob.ScheduleParallel(triangleCellNeedUpdateChangeHandle);
            state.Dependency = updateCellSizeAndStartHandle;

            updateCellSizeAndStartHandle.Complete();

            for (int i = 1; i < triangleFlattenBufferElements.Length; i++)
            {
                TriangleFlattenBufferElement flattenBuffer = triangleFlattenBufferElements[i];
                flattenBuffer.StartIndex =
                    triangleFlattenBufferElements[i - 1].StartIndex + triangleFlattenBufferElements[i - 1].Size;
                triangleFlattenBufferElements[i] = flattenBuffer;
            }

            int totalCount = 0;
            foreach (TriangleFlattenBufferElement triangleFlattenBufferElement in triangleFlattenBufferElements)
            {
                totalCount += triangleFlattenBufferElement.Size;
            }

            if (triangleIndexBufferElements.Length < totalCount)
                triangleIndexBufferElements.AddRange(
                    new NativeArray<TriangleFlattenIndexBufferElement>(totalCount - triangleIndexBufferElements.Length,
                        Allocator.Temp));

            UpdateCellFlattenIndexJob updateCellFlattenIndexJob =
                new UpdateCellFlattenIndexJob(triangleIndexBufferElements, triangleFlattenBufferElements,
                    navmesh.CellXLength);
            JobHandle updateCellFlattenIndexHandle = updateCellFlattenIndexJob.ScheduleParallel(state.Dependency);
            state.Dependency = updateCellFlattenIndexHandle;
        }
    }

    [BurstCompile]
    internal struct ReduceCellIndexList : IJobParallelForDefer
    {
        [ReadOnly] private readonly float groupDivision, groupDivisionSquared;

        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion]
        private NativeArray<TriangleWasUpdatedBufferElement> triangleWasUpdatedBufferElements;

        private readonly int vertUpdatedSize;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction]
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<NavMeshCellComponent> cellLookup;

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            NavMeshCellComponent cell = this.cellLookup[entity];
            DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles = this.cellTrianglesLookup[entity];

            float xMin = this.minFloorX + this.groupDivision * cell.X,
                zMin = this.minFloorZ + this.groupDivision * cell.Z;

            float2 center = new float2(xMin + this.groupDivision * .5f, zMin + this.groupDivision * .5f);

            for (int i = cellTriangles.Length; i >= 0; i--)
            {
                NavTriangleBufferElement navTriangleBufferElement = this.triangles[cellTriangles[i].Index];
                if (!navTriangleBufferElement.WasUpdated ||
                    !(QuickSquareDistance(
                          center,
                          navTriangleBufferElement.Center.x,
                          navTriangleBufferElement.Center.z) >
                      navTriangleBufferElement.SquaredRadius + this.groupDivisionSquared)) continue;

                (cellTriangles[i], cellTriangles[cell.NewSize - 1]) =
                    (cellTriangles[cell.NewSize - 1], cellTriangles[i]);

                cell.NewSize -= 1;
            }
        }

        [BurstCompile]
        private static float QuickSquareDistance(in float2 a, float x, float z)
        {
            float2 v = new float2(a.x - x, a.y - z);
            return v.x * v.x + v.y * v.y;
        }
    }

    [BurstCompile]
    internal struct UpdateCellTriangleIndexListsJob : IJobParallelForDefer, IDisposable
    {
        [ReadOnly] private readonly float groupDivision, groupDivisionSquared;

        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> verts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleWasUpdatedBufferElement> wasUpdateBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeList<Entity> entities;

        [NativeDisableParallelForRestriction]
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<NavMeshCellComponent> cellLookup;

        public UpdateCellTriangleIndexListsJob(
            NativeList<Entity> entities,
            float groupDivision, float minFloorX, float minFloorZ,
            DynamicBuffer<VertBufferElement> simpleVerts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<TriangleWasUpdatedBufferElement> wasUpdateBufferElements,
            BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup,
            ComponentLookup<NavMeshCellComponent> cellLookup) : this()
        {
            this.entities = entities;
            this.groupDivision = groupDivision;
            this.groupDivisionSquared = groupDivision * groupDivision;
            this.minFloorX = minFloorX;
            this.minFloorZ = minFloorZ;
            this.verts = simpleVerts.ToNativeArray(Allocator.TempJob);
            this.triangles = triangles.ToNativeArray(Allocator.TempJob);
            this.wasUpdateBufferElements = wasUpdateBufferElements.ToNativeArray(Allocator.TempJob);

            this.cellTrianglesLookup = cellTrianglesLookup;
            this.cellLookup = cellLookup;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangles = this.cellTrianglesLookup[entity];
            NavMeshCellComponent cellComponent = this.cellLookup[entity];

            float xMin = this.minFloorX + this.groupDivision * cellComponent.X,
                xMax = xMin + this.groupDivision,
                zMin = this.minFloorZ + this.groupDivision * cellComponent.Z,
                zMax = zMin + this.groupDivision;

            NativeHashSet<int> check = new NativeHashSet<int>(8, Allocator.Temp);

            float2 center = new float2(xMin + this.groupDivision * .5f, zMin + this.groupDivision * .5f);

            NativeArray<int> reuseArray = new NativeArray<int>(3, Allocator.Temp);

            int count = 0;

            //foreach (TriangleWasUpdatedBufferElement wasUpdateBufferElement in this.wasUpdateBufferElements)
            foreach (NavTriangleBufferElement navTriangleBufferElement in this.triangles)
            {
                //NavTriangleBufferElement navTriangleBufferElement = this.triangles[wasUpdateBufferElement.Index];

                if (QuickSquareDistance(center,
                        navTriangleBufferElement.Center.x, navTriangleBufferElement.Center.z) >
                    navTriangleBufferElement.SquaredRadius + this.groupDivisionSquared)
                    continue;

                navTriangleBufferElement.Vertices(ref reuseArray);

                bool inserted = false;

                foreach (int i in reuseArray)
                {
                    VertBufferElement v = this.verts[i];
                    if (check.Contains(i) || (v.Position.x >= xMin &&
                                              v.Position.x <= xMax &&
                                              v.Position.z >= zMin &&
                                              v.Position.z <= zMax)) continue;

                    if (cellTriangles.Length > count)
                    {
                        NavMeshCellTriangleIndexBufferElement t = cellTriangles[count];
                        t.Index = navTriangleBufferElement.ID;
                        cellTriangles[count] = t;
                    }
                    else
                        cellTriangles.Add(new NavMeshCellTriangleIndexBufferElement
                            { Index = navTriangleBufferElement.ID });

                    count++;
                    check.Add(navTriangleBufferElement.A);
                    inserted = true;
                    break;
                }

                if (inserted)
                    continue;

                VertBufferElement v1 = this.verts[reuseArray[0]],
                    v2 = this.verts[reuseArray[1]],
                    v3 = this.verts[reuseArray[2]];

                if (PointWithinTriangle2D(center, v1, v2, v3))
                {
                    if (cellTriangles.Length > count)
                    {
                        NavMeshCellTriangleIndexBufferElement t = cellTriangles[count];
                        t.Index = navTriangleBufferElement.ID;
                        cellTriangles[count] = t;
                    }
                    else
                        cellTriangles.Add(new NavMeshCellTriangleIndexBufferElement
                            { Index = navTriangleBufferElement.ID });
                }

                if (this.IsLineSegmentIntersectingCircle(v1, v2, center) ||
                    this.IsLineSegmentIntersectingCircle(v1, v3, center) ||
                    this.IsLineSegmentIntersectingCircle(v2, v3, center))
                {
                    if (cellTriangles.Length > count)
                    {
                        NavMeshCellTriangleIndexBufferElement t = cellTriangles[count];
                        t.Index = navTriangleBufferElement.ID;
                        cellTriangles[count] = t;
                    }
                    else
                        cellTriangles.Add(new NavMeshCellTriangleIndexBufferElement
                            { Index = navTriangleBufferElement.ID });

                    count++;
                }
            }

            cellComponent.Size = count;
        }

        [BurstCompile]
        private static float QuickSquareDistance(in float2 a, float x, float z)
        {
            float2 v = new float2(a.x - x, a.y - z);
            return v.x * v.x + v.y * v.y;
        }

        //https://www.youtube.com/watch?v=HYAgJN3x4GA
        [BurstCompile]
        private static bool PointWithinTriangle2D(in float2 point, in VertBufferElement a, in VertBufferElement b,
            in VertBufferElement c)
        {
            float s1 = c.Position.z - a.Position.z + 0.0001f;
            float s2 = c.Position.x - a.Position.x;
            float s3 = b.Position.z - a.Position.z;
            float s4 = point.y - a.Position.z;

            float w1 = (a.Position.x * s1 + s4 * s2 - point.x * s1) /
                       (s3 * s2 - (b.Position.x - a.Position.x + 0.0001f) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            return w1 >= 0 && w2 >= 0 && w1 + w2 <= 1;
        }

        [BurstCompile]
        private bool IsLineSegmentIntersectingCircle(in VertBufferElement p1, in VertBufferElement p2,
            in float2 circleCenter)
        {
            // Vector from p1 to p2
            float dx = p2.Position.x - p1.Position.x;
            float dy = p2.Position.z - p1.Position.z;

            // Vector from p1 to the circle center
            float fx = circleCenter.x - p1.Position.x;
            float fy = circleCenter.y - p1.Position.z;

            // Calculate coefficients for quadratic equation
            float a = dx * dx + dy * dy;
            float b = 2 * (fx * dx + fy * dy);
            float c = fx * fx + fy * fy - this.groupDivisionSquared;

            // Calculate the discriminant
            float discriminant = b * b - 4 * a * c;

            // If discriminant is negative, no intersection
            if (discriminant < 0)
            {
                return false;
            }

            // Otherwise, calculate the intersection points
            discriminant = (float)Math.Sqrt(discriminant);

            // Calculate the two possible t values for the intersection points
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // Check if either intersection point is within the bounds of the line segment (0 <= t <= 1)
            return t1 is >= 0 and <= 1 || t2 is >= 0 and <= 1;
        }

        public void Dispose()
        {
            this.verts.Dispose();
            this.triangles.Dispose();
            this.wasUpdateBufferElements.Dispose();
        }
    }

    [BurstCompile]
    internal partial struct TrianglesInCellsJob : IJobEntity
    {
        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleFlattenBufferElement> triangleFlattenBufferElements;

        [WriteOnly] public NativeList<TriangleFlattenIndexBufferElement> TriangleIndexBufferElements;

        private int cellXLength;


        public void Execute(
            ref DynamicBuffer<NavMeshCellTriangleIndexBufferElement> navMeshCellTriangleIndexBufferElements,
            in NavMeshCellComponent cellComponent)
        {
            int index = cellComponent.X * this.cellXLength + cellComponent.Z;
            TriangleFlattenBufferElement triangleFlattenElement = this.triangleFlattenBufferElements[index];
            triangleFlattenElement.Size = navMeshCellTriangleIndexBufferElements.Length;
            this.triangleFlattenBufferElements[index] = triangleFlattenElement;

            int startIndex = index > 0 ? this.triangleFlattenBufferElements[index].StartIndex - 1 : 0;
            foreach (NavMeshCellTriangleIndexBufferElement triangleElement in navMeshCellTriangleIndexBufferElements)
            {
                this.TriangleIndexBufferElements[startIndex] =
                    new TriangleFlattenIndexBufferElement { Index = triangleElement.Index };
            }
        }
    }

    [BurstCompile]
    internal partial struct UpdateCellFlattenSizeJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        private DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements;

        private readonly int cellXLength;

        public UpdateCellFlattenSizeJob(DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements,
            int cellXLength) : this()
        {
            this.triangleFlattenBufferElements = triangleFlattenBufferElements;
            this.cellXLength = cellXLength;
        }


        [BurstCompile]
        public void Execute(in NavMeshCellComponent cellComponent)
        {
            int i = cellComponent.X * this.cellXLength + cellComponent.Z;
            TriangleFlattenBufferElement t = this.triangleFlattenBufferElements[i];
            t.Size = cellComponent.Size;
            this.triangleFlattenBufferElements[i] = t;
        }
    }

    [BurstCompile]
    internal partial struct UpdateCellFlattenIndexJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        private DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleFlattenBufferElement> triangleFlattenBufferElements;

        private readonly int cellXLength;

        public UpdateCellFlattenIndexJob(DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements,
            DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements, int cellXLength) :
            this()
        {
            this.triangleIndexBufferElements = triangleIndexBufferElements;
            this.triangleFlattenBufferElements = triangleFlattenBufferElements.ToNativeArray(Allocator.TempJob);
            this.cellXLength = cellXLength;
        }

        public void Execute(in NavMeshCellComponent cellComponent,
            in DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangleIndexBufferElements)
        {
            TriangleFlattenBufferElement f =
                this.triangleFlattenBufferElements[cellComponent.X * this.cellXLength + cellComponent.Z];
            for (int i = 0; i < cellComponent.Size; i++)
            {
                TriangleFlattenIndexBufferElement t = this.triangleIndexBufferElements[f.StartIndex + i];
                t.Index = cellTriangleIndexBufferElements[i].Index;
                this.triangleIndexBufferElements[f.StartIndex + i] = t;
            }
        }
    }

    [BurstCompile]
    internal struct TriangleCellNeedUpdateChangeJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] private DynamicBuffer<NavTriangleBufferElement> triangles;

        public TriangleCellNeedUpdateChangeJob(ref DynamicBuffer<NavTriangleBufferElement> triangles)
        {
            this.triangles = triangles;
        }

        public void Execute(int index)
        {
            NavTriangleBufferElement t = this.triangles[index];
            t.WasUpdated = false;
            this.triangles[index] = t;
        }
    }
}