#region Libraries

using Runtime.Variables;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld
{
    public sealed class ConnectionPoint : MonoBehaviour
    {
        #region Values

        [SerializeField] private SceneReference connectTo;

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.name = !this.connectTo.IsEmpty
                ? "ConnectionPoint - " + this.connectTo.ScenePath.Split('/')[^1].Replace(".unity", "")
                : "EMPTY CONNECTION POINT";
        }
#endif

        #endregion

        #region Getters

        public string ScenePath => this.connectTo.ScenePath;

        #endregion
    }
}