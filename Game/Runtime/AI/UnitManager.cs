#region Libraries

using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Player;
using Runtime.ScriptableVariables.Events;
using Runtime.Systems;
using Runtime.Systems.PersistantRunner;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.AI
{
    [CreateAssetMenu(menuName = "Manager/Unit")]
    public sealed class UnitManager : Manager, IFrameStart, IFrameUpdate, IFrameLateUpdate
    {
        #region Values

        [SerializeField] [Required] private PlayerStateEvent playerStateChangeEvent;

        [SerializeField] [Required] private PlayerManager playerManager;

        [SerializeField] [Min(1)] private int mediumDelay = 1, farDelay = 1;

        [SerializeField] private float closeDistance, mediumDistance;

        private readonly List<UnitBase> controllers = new List<UnitBase>(),
            close = new List<UnitBase>(),
            medium = new List<UnitBase>(),
            far = new List<UnitBase>();

        private readonly Dictionary<Type, List<UnitBase>> controllersByType = new Dictionary<Type, List<UnitBase>>();

        private int count;

        private bool shouldUpdate = true;

        #endregion

        #region In

        public IEnumerator FrameStart(PersistantRunner runner)
        {
            this.playerStateChangeEvent.AddListener(this.OnPlayerStateChange);

            yield break;
        }

        public void FrameUpdate()
        {
            if (!this.shouldUpdate)
                return;

            this.close.ForEach(unit => unit.UpdateUnit());

            if (this.count % this.farDelay == 0)
                this.far.ForEach(unit => unit.UpdateUnit());

            if (this.count % this.mediumDelay == 0)
                this.medium.ForEach(unit => unit.UpdateUnit());

            if (this.count * this.farDelay == this.count) this.count = 0;
        }

        public void FrameLateUpdate()
        {
            this.UpdateUnitDistanceLists();
        }

        public void Register(UnitBase add)
        {
            if (add == null || this.controllers.Contains(add))
                return;

            this.controllers.Add(add);

            Vector3 cPos = add.transform.position,
                pPos = this.playerManager.GetAgent() != null
                    ? this.playerManager.GetAgent().transform.position
                    : Vector3.zero;

            float distance = Vector3.Distance(cPos, pPos);

            switch (distance)
            {
                case < 50:
                    this.close.Add(add);
                    break;
                case < 100:
                    this.medium.Add(add);
                    break;
                default:
                    this.far.Add(add);
                    break;
            }

            if (this.controllersByType.ContainsKey(add.GetType()))
                this.controllersByType[add.GetType()].Add(add);
            else
                this.controllersByType.Add(add.GetType(), new List<UnitBase> { add });
        }

        public void Unregister(UnitBase remove)
        {
            if (remove == null || !this.controllers.Contains(remove))
                return;

            this.controllers.Remove(remove);
        }

        public void PauseAllUnits()
        {
            this.shouldUpdate = false;

            foreach (UnitBase unitBase in this.controllers)
                unitBase.PauseUnit();
        }

        public void ResumeAllUnits()
        {
            this.shouldUpdate = true;

            foreach (UnitBase unitBase in this.controllers)
                unitBase.ResumeUnit();
        }

        #endregion

        #region Out

        public T[] GetAllUnitsOfType<T>() where T : UnitBase
        {
            if (!this.controllersByType.ContainsKey(typeof(T)))
                return Array.Empty<T>();

            return this.controllersByType[typeof(T)].ToArray() as T[];
        }

        #endregion

        #region Internal

#if UNITY_EDITOR
        protected override void OnManagerDisabled()
        {
            this.playerStateChangeEvent.RemoveListener(this.OnPlayerStateChange);
        }
#endif

        private void UpdateUnitDistanceLists()
        {
            Vector3 playerPos = this.playerManager.GetController().transform.position;

            float sqrClose = this.closeDistance * this.closeDistance,
                sqrMedium = this.mediumDistance * this.mediumDistance;


            this.close.Clear();
            this.medium.Clear();
            this.far.Clear();
            foreach (UnitBase unitBase in this.controllers)
            {
                float sqrDistance = (playerPos - unitBase.transform.position).sqrMagnitude;
                if (sqrDistance < sqrClose)
                    this.close.Add(unitBase);
                else if (sqrDistance < sqrMedium)
                    this.medium.Add(unitBase);
                else
                    this.far.Add(unitBase);
            }
        }

        private void OnPlayerStateChange(PlayerState playerState)
        {
            if (playerState == PlayerState.Paused)
            {
                if (this.shouldUpdate)
                    this.PauseAllUnits();

                this.shouldUpdate = false;
            }
            else
            {
                if (!this.shouldUpdate)
                    this.ResumeAllUnits();

                this.shouldUpdate = true;
            }
        }

        #endregion
    }
}