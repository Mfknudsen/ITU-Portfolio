#region Packages

using UnityEngine;

#endregion

namespace Runtime.World.Overworld
{
    public sealed class Interior : MonoBehaviour
    {
        #region Values

        private bool playerInInterior;

        #endregion

        #region Getters

        public bool IsPlayerInInterior => 
            this.playerInInterior;

        #endregion

        #region In

        public void Enter()
        {
            this.playerInInterior = true;
        }

        public void Exit()
        {
            this.playerInInterior = false;
        }

        #endregion
    }
}