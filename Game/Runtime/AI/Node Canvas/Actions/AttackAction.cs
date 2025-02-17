#region Packages

using NodeCanvas.Framework;
using UnityEngine;

#endregion

namespace Runtime.AI.Node_Canvas.Actions
{
    public class AttackAction : ActionTask
    {
        public BBParameter<bool> meleeAttack;
        public BBParameter<UnitBase> npcBase;
        public BBParameter<GameObject> target;


        protected override void OnUpdate()
        {
            this.EndAction(true);
        }
    }
}