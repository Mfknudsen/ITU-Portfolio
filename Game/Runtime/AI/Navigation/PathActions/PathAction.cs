#region Libraries

using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.PathActions
{
    public abstract class PathAction
    {
        public abstract bool CheckAction(UnitPath path, NavigationAgent agent);

        public abstract Vector3 Destination();

        public abstract bool IsWalkAction();
    }
}