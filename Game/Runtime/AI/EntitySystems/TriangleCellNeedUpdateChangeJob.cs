using Runtime.AI.EntityBuffers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Runtime.AI.EntitySystems
{
    [BurstCompile]
    public struct TriangleCellNeedUpdateChangeJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] private DynamicBuffer<NavTriangleBufferElement> triangles;

        public TriangleCellNeedUpdateChangeJob(ref DynamicBuffer<NavTriangleBufferElement> triangles)
        {
            this.triangles = triangles;
        }

        public void Execute(int index)
        {
            NavTriangleBufferElement t = this.triangles[index];
            t.CellNeedsUpdate = false;
            this.triangles[index] = t;
        }
    }
}