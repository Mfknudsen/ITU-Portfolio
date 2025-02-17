#region Packages

using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using Runtime.AI.Senses;
using Runtime.AI.Senses.Sight;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public sealed class SightContainsDislikedTarget : ConditionTask
    {
        public BBParameter<UnitSightCone> sight;
        public BBParameter<List<string>> disliked;

        protected override bool OnCheck()
        {
            foreach (GameObject gameObject in this.sight.value.GetInSight().Select(v => v.gameObject))
            {
                if (this.disliked.value.Contains(gameObject.name))
                    return true;
            }

            return false;
        }
    }
}