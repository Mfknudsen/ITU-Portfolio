#region Libraries

#endregion

using System;
using UnityEngine;

namespace Runtime.AI.Navigation.PathActions
{
    public sealed class InteractAction : PathAction
    {
        public override Vector3 Destination()
        {
            throw new NotImplementedException();
        }

        public override bool CheckAction(UnitPath path, UnitAgent agent)
        {
            throw new NotImplementedException();
        }

        #region Getters

        public override bool IsWalkAction()
        {
            return false;
        }

        #endregion
    }
}