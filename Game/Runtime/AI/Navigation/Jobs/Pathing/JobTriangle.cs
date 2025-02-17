#region Libraries

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    public struct JobTriangle : IDisposable
    {
        #region Values

        public readonly int id, a, b, c, area;

        public UnsafeList<int> neighbors;
        private UnsafeList<int> navigationPoints;

        public UnsafeList<float> widths;

        #endregion

        #region Build In States

        public JobTriangle(NavTriangle triangle)
        {
            this.id = triangle.ID;
            this.a = triangle.GetA;
            this.b = triangle.GetB;
            this.c = triangle.GetC;
            this.area = triangle.Area;

            this.neighbors = new UnsafeList<int>(triangle.Neighbors.Count, Allocator.Temp);
            foreach (int t in triangle.Neighbors)
                this.neighbors.Add(t);

            this.navigationPoints = new UnsafeList<int>(triangle.NavPoints.Count, Allocator.Temp);
            foreach (int t in triangle.NavPoints)
                this.navigationPoints.Add(t);

            this.widths = new UnsafeList<float>(triangle.Widths.Count, Allocator.Temp);
            foreach (float t in triangle.Widths)
                this.widths.Add(t);
        }

        public void Dispose()
        {
            if (this.neighbors.IsCreated)
                this.neighbors.Dispose();

            if (this.navigationPoints.IsCreated)
                this.navigationPoints.Dispose();

            if (this.widths.IsCreated)
                this.widths.Dispose();
        }

        #endregion
    }
}