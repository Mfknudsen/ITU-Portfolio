#region Libraries

using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.Jobs.Updating
{
    public struct UpdateDirectionJob : IJobParallelFor
    {
        #region Values

        [WriteOnly] private NativeArray<Vector2> directions;
        [ReadOnly] private NativeArray<float> radius;
        [ReadOnly] private NativeArray<Vector3> agentPositions;
        [ReadOnly] private RayCastObjectHolder rayCastObjectHolder;

        #endregion

        #region Build In States

        public void Execute(int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}