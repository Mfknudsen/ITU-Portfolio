#region Packages

using UnityEngine;

#endregion

namespace Runtime.Player
{
    public struct PlayerInputContainer
    {
        public Vector2 moveDir, rotDir;
        public bool run;
    }
}