#region Libraries

using Runtime.Core;
using Unity.Collections;
using Unity.Mathematics;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    public readonly struct JobNode
    {
        public readonly int triangleID, costVertIndex;
        private readonly float moveCost, distanceToGoal;
        public readonly int previousNodeTriangleID;

        public JobNode(JobTriangle triangle, NativeArray<float2> simpleVerts, float2 destination,
            JobNode previousJobNode,
            NativeArray<int> areas, NativeArray<JobTriangle> triangles, int previousCostVertIndex)
        {
            this.previousNodeTriangleID = previousJobNode.triangleID;
            this.triangleID = triangle.id;

            this.costVertIndex = triangle.a;
            this.moveCost = 0;
            this.distanceToGoal = 0;

            this.SetBestCostAndDistance(triangle, simpleVerts, destination, out float d, out float c);

            this.moveCost = 0;
            //this.previousCost + (areas[this.triangleID] + 1) *
        }

        public JobNode(JobTriangle triangle, NativeArray<float2> simpleVerts, float2 destination, float startMoveCost)
        {
            this.previousNodeTriangleID = -1;
            this.triangleID = triangle.id;

            this.moveCost = startMoveCost;

            this.distanceToGoal = 0;
            //TODO:
            this.costVertIndex = 0;

            this.SetBestCostAndDistance(triangle, simpleVerts, destination, out float d, out float _);

            this.distanceToGoal = d;
        }

        public float Total()
        {
            return this.moveCost + this.distanceToGoal;
        }

        private void SetBestCostAndDistance(JobTriangle triangle, NativeArray<float2> simpleVerts, float2 destination,
            out float d, out float c)
        {
            d = (destination - simpleVerts[triangle.a]).Distance();

            for (int i = 1; i < 3; i++)
            {
            }

            c = d;
        }
    }
}