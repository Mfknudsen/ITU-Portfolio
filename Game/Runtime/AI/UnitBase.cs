#region Libraries

using NodeCanvas.BehaviourTrees;
using Runtime.AI.Senses.Sight;
using Runtime.World.Overworld.Interactions;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using NodeCanvas.Framework;
using Runtime.AI.Navigation;
using Runtime.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

#endregion

namespace Runtime.AI
{
    [RequireComponent(typeof(UnitAgent), typeof(BehaviourTreeOwner))]
    public abstract class UnitBase : MonoBehaviour, IInteractable
    {
        #region Values

        [SerializeField, FoldoutGroup("Base"), Required]
        private UnitManager unitManager;

        [SerializeField, FoldoutGroup("Base/Visual")]
        protected GameObject visualsObject;

        [SerializeField, FoldoutGroup("Base/Navmesh"), Required]
        protected UnitAgent agent;

        [SerializeField, FoldoutGroup("Base/Senses")]
        private UnitSight sightCone;

        [SerializeField, FoldoutGroup("Base"), Required]
        private BehaviourTreeOwner behaviourTreeOwner;

        [SerializeField, FoldoutGroup("Base"), Required]
        private Blackboard behaviourBlackboard;

        [SerializeField, FoldoutGroup("Base")] private UnitState unitState;

        private readonly Dictionary<string, object> memoryBank = new Dictionary<string, object>();

        private UnityEvent<UnitBase> disableEvent;

        private Coroutine currentRotateOrder;

        #endregion

        #region Build In States

        private void OnEnable() =>
            this.unitManager.Register(this);

        private void OnDisable()
        {
            this.disableEvent.Invoke(this);
            this.unitManager.Unregister(this);
        }

        #endregion

        #region Getters

        public UnitAgent GetAgent() =>
            this.agent;

        public TObject GetFromMemory<TObject>(string key) where TObject : Object
        {
            if (!this.memoryBank.ContainsKey(key)) return null;

            return this.memoryBank[key] as TObject;
        }

        public UnitState GetUnitState() =>
            this.unitState;

        public Blackboard GetBlackboard() => this.behaviourBlackboard;

        #endregion

        #region Setters

        public void SetMemory(string key, object value)
        {
            if (this.memoryBank.ContainsKey(key))
            {
                this.memoryBank[key] = value;
                return;
            }

            this.memoryBank.Add(key, value);
        }

        public void SetUnitState(UnitState set) =>
            this.unitState = set;

        #endregion

        #region In

        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void UpdateUnit()
        {
            if (this.sightCone != null)
                this.sightCone.UpdateSight();

            if (this.behaviourTreeOwner != null)
                this.behaviourTreeOwner.UpdateBehaviour();
        }

        public abstract void InteractTrigger();

        public void AddDisableEventListener(UnityAction<UnitBase> action)
        {
            this.disableEvent ??= new UnityEvent<UnitBase>();

            this.disableEvent.AddListener(action);
        }

        public void RemoveDisableEventListener(UnityAction<UnitBase> action) =>
            this.disableEvent?.RemoveListener(action);

        public void PauseUnit() =>
            this.agent.SetStopped(true);

        public void ResumeUnit() =>
            this.agent.SetStopped(false);

        public bool MoveAndRotateUnitAgent(Vector3 position, Quaternion rotation, UnityAction onComplete = null)
        {
            if (this.currentRotateOrder != null)
                this.StopCoroutine(this.currentRotateOrder);

            this.agent.MoveTo(position);

            this.agent.SetStopped(false);

            this.currentRotateOrder = this.StartCoroutine(this.Rotate(rotation, onComplete));

            return true;
        }

        #endregion

        #region Out

        public object GetStateByKey(string s)
        {
            return null;
        }

        #endregion

        #region Internal

        private IEnumerator Rotate(Quaternion rotation, UnityAction onComplete)
        {
            yield return new UnityEngine.WaitUntil(() => this.agent.IsStopped());

            Transform t = this.transform;
            Vector3 rotationForward = rotation.ForwardFromRotation();

            while (Vector3.Angle(rotationForward, t.forward) < .5f)
            {
                t.LookAt(t.position + Vector3.Lerp(t.forward, rotationForward,
                    Time.deltaTime * this.agent.Settings.TurnSpeed));
                yield return null;
            }

            onComplete?.Invoke();
        }

        #endregion
    }
}