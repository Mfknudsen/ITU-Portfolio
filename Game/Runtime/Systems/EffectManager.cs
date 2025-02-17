#region Libraries

using Runtime.ScriptableVariables.Objects;
using Runtime.Systems.PersistantRunner;
using Runtime.Systems.Pooling;
using Runtime.VFX;
using Runtime.VFX.Reuseable;
using Runtime.VFX.Scene;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.VFX.SingleUse;
using UnityEngine;

#endregion

namespace Runtime.Systems
{
    [CreateAssetMenu(menuName = "Managers/Effect Manager"), Serializable]
    public sealed class EffectManager : Manager, IFrameStart, IFrameUpdate, IFrameLateUpdate
    {
        [SerializeField, Required] private TransformGenericVariable playerTransformGeneric;
        [SerializeField] private List<EffectLimit> effectsLimits = new List<EffectLimit>();

        private readonly Dictionary<Type, SceneEffectHolder> sceneEffects = new Dictionary<Type, SceneEffectHolder>();
        private readonly Dictionary<Type, SingleUseEffectHolder> singleUseEffects =
            new Dictionary<Type, SingleUseEffectHolder>();
        private readonly Dictionary<Type, ReuseableEffect> reuseableEffects = new Dictionary<Type, ReuseableEffect>();

        #region IFrameUpdate

        public IEnumerator FrameStart(PersistantRunner.PersistantRunner runner)
        {
            foreach (EffectLimit effectsLimit in this.effectsLimits)
            {
                switch (effectsLimit.effect)
                {
                    case SceneEffect sceneEffect:
                        this.sceneEffects.Add(sceneEffect.GetType(),
                            new SceneEffectHolder(effectsLimit.max, sceneEffect, this));
                        break;
                    case SingleUseEffect singleUseEffect:
                        this.singleUseEffects.Add(singleUseEffect.GetType(),
                            new SingleUseEffectHolder(effectsLimit.max, singleUseEffect));
                        break;
                }
            }

            this.ready = true;

            yield break;
        }

        public void FrameUpdate()
        {
            foreach (SceneEffectHolder sceneEffectHolder in this.sceneEffects.Values)
                sceneEffectHolder.UpdateEffects();

            foreach (SingleUseEffectHolder singleUseEffectHolder in this.singleUseEffects.Values)
                singleUseEffectHolder.UpdateEffects();
        }

        public void FrameLateUpdate()
        {
            foreach (SceneEffectHolder sceneEffectHolder in this.sceneEffects.Values)
                sceneEffectHolder.UpdateActives();

            foreach (SingleUseEffectHolder singleUseEffectHolder in this.singleUseEffects.Values)
                singleUseEffectHolder.CheckActives();
        }

        #endregion

        #region In

        public void RegisterEffect(EffectBase effectBase)
        {
            switch (effectBase)
            {
                case SceneEffect sceneEffect:
                    {
                        Type type = sceneEffect.GetType();
                        if (!this.sceneEffects.ContainsKey(type))
                        {
                            EffectLimit limit = this.effectsLimits.FirstOrDefault(e => e.effect.GetType() == type);
                            this.sceneEffects.Add(type, new SceneEffectHolder(limit?.max ?? 0, sceneEffect, this));
                        }

                        this.sceneEffects[type].Add(sceneEffect);
                        sceneEffect.CheckRules();
                        break;
                    }
                case SingleUseEffect singleUseEffect:
                    {
                        Type type = singleUseEffect.GetType();
                        if (!this.singleUseEffects.ContainsKey(type))
                        {
                            EffectLimit limit = this.effectsLimits.FirstOrDefault(e => e.effect.GetType() == type);
                            this.singleUseEffects.Add(type, new SingleUseEffectHolder(limit?.max ?? 0, singleUseEffect));
                        }

                        this.singleUseEffects[type].Add(singleUseEffect);
                        break;
                    }
                case ReuseableEffect reuseableEffect:
                    {
                        this.reuseableEffects.Add(reuseableEffect.GetType(), reuseableEffect);
                        break;
                    }
            }
        }

        public void UnregisterEffect(EffectBase effectBase)
        {
            switch (effectBase)
            {
                case SceneEffect sceneEffect:
                    this.sceneEffects[sceneEffect.GetType()].Remove(sceneEffect);
                    break;
                case SingleUseEffect singleUseEffect:
                    this.singleUseEffects[singleUseEffect.GetType()].Remove(singleUseEffect);
                    break;
                case ReuseableEffect reuseableEffect:
                    this.reuseableEffects.Remove(reuseableEffect.GetType());
                    break;
            }
        }

        #region Reuseable

        public void PlayOne(ReuseableEffect reuseableEffect, Vector3 position)
        {
            if (reuseableEffect is null) return;

            this.GetOrInstanceEffect(reuseableEffect)
                .PlayOne(this.playerTransformGeneric.Position, position, Quaternion.identity);
        }

        public void PlayOne(ReuseableEffect reuseableEffect, Vector3 position, Quaternion rotation)
        {
            if (reuseableEffect is null) return;

            this.GetOrInstanceEffect(reuseableEffect)
                .PlayOne(this.playerTransformGeneric.Position, position, rotation);
        }

        public void PlayOne(ReuseableEffect reuseableEffect, Vector3 position, Vector3 forwardDirection)
        {
            if (reuseableEffect is null) return;

            this.GetOrInstanceEffect(reuseableEffect)
                .PlayOne(this.playerTransformGeneric.Position, position, Quaternion.LookRotation(forwardDirection));
        }

        #endregion

        #region Single Use

        public void PlayAtLocation(SingleUseEffect singleUseEffect, Vector3 position)
        {
            if (singleUseEffect is null) return;

            Type type = singleUseEffect.GetType();
            if (!this.singleUseEffects.ContainsKey(type))
                this.InstantiateNew(singleUseEffect);

            SingleUseEffect selected = this.GetOrInstanceEffect(singleUseEffect);

            selected.transform.position = position;
            selected.Play(this.playerTransformGeneric.Position);
        }

        public void PlayAtLocationAndRotation(SingleUseEffect singleUseEffect, Vector3 position, Quaternion rotation)
        {
            if (singleUseEffect is null) return;

            Type type = singleUseEffect.GetType();
            if (!this.singleUseEffects.ContainsKey(type))
                this.InstantiateNew(singleUseEffect);

            SingleUseEffect selected = this.GetOrInstanceEffect(singleUseEffect);

            selected ??= this.InstantiateNew(singleUseEffect);
            Transform selectedTransform = selected.transform;
            selectedTransform.position = position;
            selectedTransform.rotation = rotation;
            selected.Play(this.playerTransformGeneric.Position);
        }

        public void PlayAtLocationAndDirection(SingleUseEffect singleUseEffect, Vector3 position, Vector3 direction)
        {
            if (singleUseEffect is null) return;

            Type type = singleUseEffect.GetType();
            if (!this.singleUseEffects.ContainsKey(type))
                this.InstantiateNew(singleUseEffect);

            SingleUseEffect selected = this.GetOrInstanceEffect(singleUseEffect);

            selected ??= this.InstantiateNew(singleUseEffect);
            Transform selectedTransform = selected.transform;
            selectedTransform.position = position;
            selectedTransform.rotation = Quaternion.LookRotation(direction);
            selected.Play(this.playerTransformGeneric.Position);
        }

        public void PlayAtTransform(SingleUseEffect singleUseEffect, Transform parent)
        {
            if (singleUseEffect is null) return;

            Type type = singleUseEffect.GetType();
            if (!this.singleUseEffects.ContainsKey(type))
                this.InstantiateNew(singleUseEffect);

            SingleUseEffect selected = this.GetOrInstanceEffect(singleUseEffect);

            selected ??= this.InstantiateNew(singleUseEffect);
            Transform selectedTransform = selected.transform;
            selectedTransform.parent = parent;
            selectedTransform.position = parent.position;
            selectedTransform.rotation = parent.rotation;
            selected.Play(this.playerTransformGeneric.Position);
        }

        #endregion

        #endregion

        #region Internal

        private T InstantiateNew<T>(T prefab, Transform parent = null) where T : EffectBase
        {
            this.RegisterEffect(prefab);
            return Pool.Create(prefab, parent, true).GetComponent<T>();
        }

        private T GetOrInstanceEffect<T>(T prefab) where T : EffectBase
        {
            Type type = prefab.GetType();

            T selected = prefab switch
            {
                SingleUseEffect when !this.singleUseEffects.ContainsKey(type) =>
                    this.InstantiateNew(prefab),
                SingleUseEffect =>
                    this.singleUseEffects[type].TryGetEffect() as T,
                SceneEffect when !this.sceneEffects.ContainsKey(type) =>
                    this.InstantiateNew(prefab),
                SceneEffect =>
                    this.sceneEffects[type].TryGetEffect() as T,
                ReuseableEffect when !this.reuseableEffects.ContainsKey(type) =>
                    this.InstantiateNew(prefab),
                ReuseableEffect =>
                    this.reuseableEffects[type] as T,
                _ => null
            };

            if (selected != null)
                selected.ResetEffect();
            return selected;
        }

        #endregion
    }

    [Serializable]
    internal class EffectLimit
    {
        [SerializeField, AssetSelector(Paths = "Assets/Prefabs/VFX"), AssetsOnly]
        public EffectBase effect;

        [SerializeField, Min(0), Tooltip("Maximum allowed instances of the effect prefab. 0 = No limit")]
        public int max;
    }
}