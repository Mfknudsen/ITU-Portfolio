#region Packages

using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI.Senses.Sight
{
    public abstract class UnitSight : MonoBehaviour
    {
        [SerializeField, Required] protected ViewableRegistry registry;

        [SerializeField, Min(0)] protected float radius, size;

        [SerializeField, Required] protected BehaviourTreeOwner behaviourTree;

        [SerializeField, Required] protected Transform originTransform;

        protected readonly List<Viewable> inSight = new List<Viewable>();

        public abstract void UpdateSight();

        public bool IsPlayerInSight =>
            this.inSight.Any(v => v.ContainsTag(ViewableTag.Player));
    }
}