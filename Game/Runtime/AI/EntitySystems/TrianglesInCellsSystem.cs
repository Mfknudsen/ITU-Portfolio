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
        private EntityQuery entityQuery;

        private BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup;
        private ComponentLookup<NavMeshCellComponent> cellLookup;

        private DynamicBuffer<NavTriangleBufferElement> navTriangleBufferElements;
        private DynamicBuffer<VertBufferElement> vertBufferElements;
        private DynamicBuffer<TriangleWasUpdatedBufferElement> triangleWasUpdateBufferElements;
        private DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements;
        private DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        private bool singletonReady;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NavigationMeshSingletonComponent>();

            this.entityQuery = state.GetEntityQuery(ComponentType.ReadOnly<NavMeshCellComponent>());

            this.cellTrianglesLookup = state.GetBufferLookup<NavMeshCellTriangleIndexBufferElement>();
            this.cellLookup = state.GetComponentLookup<NavMeshCellComponent>();

            this.singletonReady = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.singletonReady)
            {
                if (SystemAPI.TryGetSingletonEntity<NavigationMeshSingletonComponent>(out Entity navmeshEntity))
                {
                    this.navTriangleBufferElements =
                        SystemAPI.GetBuffer<NavTriangleBufferElement>(navmeshEntity);
                    this.vertBufferElements =
                        SystemAPI.GetBuffer<VertBufferElement>(navmeshEntity);
                    this.triangleWasUpdateBufferElements =
                        SystemAPI.GetBuffer<TriangleWasUpdatedBufferElement>(navmeshEntity);
                    this.triangleFlattenBufferElements =
                        SystemAPI.GetBuffer<TriangleFlattenBufferElement>(navmeshEntity);
                    this.triangleIndexBufferElements =
                        SystemAPI.GetBuffer<TriangleFlattenIndexBufferElement>(navmeshEntity);

                    this.singletonReady = true;
                }
                else
                    return;
            }

            this.cellTrianglesLookup.Update(ref state);
            this.cellLookup.Update(ref state);

            NavigationMeshSingletonComponent navmesh = SystemAPI.GetSingleton<NavigationMeshSingletonComponent>();

            if (navmesh.TrianglesWasUpdatedSize == 0)
                return;

            NativeArray<Entity> entities = this.entityQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length == 0)
            {
                entities.Dispose();
                return;
            }

            int batch = math.max(entities.Length / SystemInfo.processorCount, 1);

            UpdateCellTriangleIndexListsJob updateCellTriangleIndexListsJob = new UpdateCellTriangleIndexListsJob(
                entities, navmesh.GroupDivision, navmesh.MinFloorX, navmesh.MinFloorZ,
                this.vertBufferElements,
                this.navTriangleBufferElements,
                this.triangleWasUpdateBufferElements,
                this.cellTrianglesLookup,
                this.cellLookup,
                navmesh.TrianglesWasUpdatedSize);
            state.Dependency =
                updateCellTriangleIndexListsJob.ScheduleParallel(entities.Length, batch, state.Dependency);


            TriangleCellNeedUpdateChangeJob triangleCellNeedUpdateChangeJob =
                new TriangleCellNeedUpdateChangeJob(ref this.navTriangleBufferElements);
            state.Dependency =
                triangleCellNeedUpdateChangeJob.Schedule(this.navTriangleBufferElements.Length,
                    this.navTriangleBufferElements.Length / SystemInfo.processorCount,
                    state.Dependency);

            state.CompleteDependency();

            int t = navmesh.CellXLength * navmesh.CellZLength;
            if (this.triangleFlattenBufferElements.Length < t)
                this.triangleFlattenBufferElements.AddRange(
                    new NativeArray<TriangleFlattenBufferElement>(t - this.triangleFlattenBufferElements.Length,
                        Allocator.Temp));

            UpdateCellFlattenSizeJob updateCellFlattenSizeJob =
                new UpdateCellFlattenSizeJob(this.triangleFlattenBufferElements, navmesh.CellXLength);
            JobHandle updateCellSizeAndStartHandle =
                updateCellFlattenSizeJob.ScheduleParallel(state.Dependency);
            state.Dependency = updateCellSizeAndStartHandle;

            state.CompleteDependency();

            for (int i = 1; i < this.triangleFlattenBufferElements.Length; i++)
            {
                TriangleFlattenBufferElement flattenBuffer = this.triangleFlattenBufferElements[i],
                    previous = this.triangleFlattenBufferElements[i - 1];
                flattenBuffer.StartIndex =
                    previous.StartIndex + previous.Size;
                this.triangleFlattenBufferElements[i] = flattenBuffer;
            }

            int totalCount = 0;
            foreach (TriangleFlattenBufferElement triangleFlattenBufferElement in this.triangleFlattenBufferElements)
            {
                totalCount += triangleFlattenBufferElement.Size;
            }

            if (this.triangleIndexBufferElements.Length < totalCount)
                this.triangleIndexBufferElements.AddRange(
                    new NativeArray<TriangleFlattenIndexBufferElement>(
                        totalCount - this.triangleIndexBufferElements.Length,
                        Allocator.Temp));

            UpdateCellFlattenIndexJob updateCellFlattenIndexJob =
                new UpdateCellFlattenIndexJob(
                    entities,
                    this.triangleIndexBufferElements,
                    this.triangleFlattenBufferElements,
                    this.cellLookup,
                    this.cellTrianglesLookup,
                    navmesh.CellXLength);
            JobHandle handle = updateCellFlattenIndexJob.ScheduleParallel(entities.Length, batch, state.Dependency);
            state.Dependency = handle;
            state.Dependency = entities.Dispose(handle);
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
    internal struct UpdateCellTriangleIndexListsJob : IJobFor
    {
        [ReadOnly] private readonly float groupDivision, groupDivisionSquared;

        [ReadOnly] private readonly float minFloorX, minFloorZ;

        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<VertBufferElement> verts;
        [DeallocateOnJobCompletion] [ReadOnly] private NativeArray<NavTriangleBufferElement> triangles;

        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleWasUpdatedBufferElement> wasUpdateBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction]
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup;

        [NativeDisableParallelForRestriction] private ComponentLookup<NavMeshCellComponent> cellLookup;

        private readonly int wasUpdatedCount;

        public UpdateCellTriangleIndexListsJob(
            NativeArray<Entity> entities,
            float groupDivision, float minFloorX, float minFloorZ,
            DynamicBuffer<VertBufferElement> simpleVerts,
            DynamicBuffer<NavTriangleBufferElement> triangles,
            DynamicBuffer<TriangleWasUpdatedBufferElement> wasUpdateBufferElements,
            BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTrianglesLookup,
            ComponentLookup<NavMeshCellComponent> cellLookup, int wasUpdatedCount) : this()
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

            this.wasUpdatedCount = wasUpdatedCount;
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

            int count = cellComponent.TriangleSize;

            for (int i = 0; i < this.wasUpdatedCount; i++)
            {
                NavTriangleBufferElement navTriangleBufferElement =
                    this.triangles[this.wasUpdateBufferElements[i].Index];

                if (QuickSquareDistance(center,
                        navTriangleBufferElement.Center.x, navTriangleBufferElement.Center.z) >
                    navTriangleBufferElement.SquaredRadius + this.groupDivisionSquared)
                    continue;

                navTriangleBufferElement.Vertices(ref reuseArray);

                bool inserted = false;

                foreach (int vertIndex in reuseArray)
                {
                    VertBufferElement v = this.verts[vertIndex];
                    if (check.Contains(vertIndex) || (v.Position.x >= xMin &&
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

            cellComponent.TriangleSize = count;
            this.cellLookup[entity] = cellComponent;
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
    }

    [BurstCompile]
    internal partial struct TrianglesInCellsJob : IJobEntity
    {
        [DeallocateOnJobCompletion] [ReadOnly]
        private NativeArray<TriangleFlattenBufferElement> triangleFlattenBufferElements;

        [WriteOnly] private NativeList<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        private readonly int cellXLength;

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
                this.triangleIndexBufferElements[startIndex] =
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
            t.Size = cellComponent.TriangleSize;
            this.triangleFlattenBufferElements[i] = t;
        }
    }

    [BurstCompile]
    internal struct UpdateCellFlattenIndexJob : IJobFor, IDisposable
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        private NativeArray<Entity> entities;

        [NativeDisableParallelForRestriction]
        private DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private ComponentLookup<NavMeshCellComponent> cellLookup;

        [NativeDisableParallelForRestriction] [ReadOnly]
        private BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTriangleIndexBufferLookup;

        private readonly int cellXLength;

        public UpdateCellFlattenIndexJob(NativeArray<Entity> entities,
            DynamicBuffer<TriangleFlattenIndexBufferElement> triangleIndexBufferElements,
            DynamicBuffer<TriangleFlattenBufferElement> triangleFlattenBufferElements,
            ComponentLookup<NavMeshCellComponent> cellLookup,
            BufferLookup<NavMeshCellTriangleIndexBufferElement> cellTriangleIndexBufferLookup, int cellXLength)
        {
            this.entities = entities;
            this.triangleIndexBufferElements = triangleIndexBufferElements;
            this.triangleFlattenBufferElements = triangleFlattenBufferElements;
            this.cellLookup = cellLookup;
            this.cellTriangleIndexBufferLookup = cellTriangleIndexBufferLookup;
            this.cellXLength = cellXLength;
        }

        public void Execute(int index)
        {
            Entity entity = this.entities[index];
            NavMeshCellComponent cellComponent = this.cellLookup[entity];
            DynamicBuffer<NavMeshCellTriangleIndexBufferElement> cellTriangleIndexBufferElements =
                this.cellTriangleIndexBufferLookup[entity];

            TriangleFlattenBufferElement f =
                this.triangleFlattenBufferElements[cellComponent.X * this.cellXLength + cellComponent.Z];
            for (int i = 0; i < cellComponent.TriangleSize; i++)
            {
                TriangleFlattenIndexBufferElement t = this.triangleIndexBufferElements[f.StartIndex + i];
                t.Index = cellTriangleIndexBufferElements[i].Index;
                this.triangleIndexBufferElements[f.StartIndex + i] = t;
            }
        }

        public void Dispose()
        {
            this.entities.Dispose();
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