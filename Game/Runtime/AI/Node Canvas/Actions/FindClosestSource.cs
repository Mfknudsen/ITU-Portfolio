#region Packages

using NodeCanvas.Framework;
using Runtime.World.Overworld.Sources;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class FindClosestSource<T> : ActionTask where T : Source
    {
        public BBParameter<Transform> result;
        public BBParameter<T> toFind;

        protected override void OnExecute()
        {
            this.EndAction(this.result.value != null);
        }
    }
}