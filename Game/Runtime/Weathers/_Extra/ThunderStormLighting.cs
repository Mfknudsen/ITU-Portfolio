#region Packages

using System.Collections;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Weathers._Extra
{
    public class ThunderStormLighting : MonoBehaviour, IOperation
    {
        private bool done;

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            this.done = true;

            yield break;
        }

        public void OperationEnd()
        {
            Destroy(this.gameObject);
        }
    }
}