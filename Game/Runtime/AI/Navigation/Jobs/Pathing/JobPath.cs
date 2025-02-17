#region Libraries

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

#endregion

namespace Runtime.AI.Navigation.Jobs.Pathing
{
    public struct JobPath : IDisposable
    {
        #region Values

        public UnsafeList<int> nodePath;

        #endregion

        #region Build In States

        public JobPath(JobNode lastJobNode, UnsafeList<JobNode> nodes, NativeArray<JobTriangle> triangles,
            int agentStartId, int agentEndId)
        {
            this.nodePath = new UnsafeList<int>(nodes.Length / 4, Allocator.TempJob);
            JobNode checking = lastJobNode;
            while (checking.triangleID != agentStartId)
            {
                this.nodePath.Add(checking.triangleID);

                JobNode bestNeighbor = nodes[0];

                for (int i = 0; i < nodes.Length; i++)
                    if (nodes[i].triangleID == checking.previousNodeTriangleID)
                    {
                        bestNeighbor = nodes[i];
                        break;
                    }

                checking = bestNeighbor;
            }

            this.nodePath.Add(checking.triangleID);
        }

        public void Dispose()
        {
            if (this.nodePath.IsCreated)
                this.nodePath.Dispose();
        }

        #endregion

        #region Internal

        private static bool Contains(UnsafeList<int> nodes, int target)
        {
            foreach (int t in nodes)
                if (t == target)
                    return true;

            return false;
        }

        #endregion
    }
}