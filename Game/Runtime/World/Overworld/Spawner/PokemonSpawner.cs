#region Libraries

using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Spawner
{
    public abstract class PokemonSpawner : MonoBehaviour
    {
        #region Values

#if UNITY_EDITOR
        [SerializeField] protected string spawnerName;
#endif

        protected Timer checkStateTimer;

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnValidate() => 
                this.name = this.spawnerName + " Spawner - " + this.SpawnListName();
#endif

        protected virtual void OnEnable() =>
            this.CheckState();

        #endregion

        #region Internal

        protected abstract void CheckState();

#if UNITY_EDITOR
        protected abstract string SpawnListName();
#endif

        #endregion
    }
}