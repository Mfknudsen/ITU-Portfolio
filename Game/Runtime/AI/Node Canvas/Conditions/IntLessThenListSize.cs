#region Packages

using System.Collections.Generic;
using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Conditions
{
    public class IntLessThenListSize : ConditionTask
    {
        public BBParameter<int> index;
        public BBParameter<List<Transform>> list;

        protected override bool OnCheck()
        {
            return this.index.value < this.list.value.Count;
        }
    }
}