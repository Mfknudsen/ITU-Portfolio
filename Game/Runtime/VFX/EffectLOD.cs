#region Packages

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

#endregion

namespace Runtime.VFX
{
    [Serializable]
    public class EffectLOD
    {
        #region Values

        [SerializeField] private LOD[] effectLoDs = Array.Empty<LOD>();

        #endregion

        #region In

        public void Validate()
        {
            for (int i = 1; i < this.effectLoDs.Length; i++)
            {
                this.effectLoDs[i].distance = Mathf.Clamp(
                    this.effectLoDs[i].distance,
                    this.effectLoDs[i - 1].distance,
                    Mathf.Infinity);
            }
        }

        public VisualEffect[] GetByDistance(float sqrDistance)
        {
            LOD selected = null;

            foreach (LOD lod in this.effectLoDs)
            {
                if (lod.distance * lod.distance < sqrDistance)
                    selected = lod;
                else
                    break;
            }

            return selected is not null ? selected.effects : Array.Empty<VisualEffect>();
        }
        
        public VisualEffect[] GetByDistance(float sqrDistance, out int level)
        {
            LOD selected = null;
            level = 0;

            for (int index = 0; index < this.effectLoDs.Length; index++)
            {
                LOD lod = this.effectLoDs[index];
                if (lod.distance * lod.distance < sqrDistance)
                {
                    level = index;
                    selected = lod;
                }
                else
                    break;
            }

            return selected is not null ? selected.effects : Array.Empty<VisualEffect>();
        }
        
        public VisualEffect[] GetByID(int id)
        {
            if (id < 0 || id >= this.effectLoDs.Length)
                return Array.Empty<VisualEffect>();

            return this.effectLoDs[id].effects;
        }

        public VisualEffect[] GetAll() => 
            this.effectLoDs.SelectMany(e => e.effects).ToArray();

        #endregion
    }

    [Serializable]
    internal class LOD
    {
        #region Values

        [SerializeField, Min(0)] internal float distance;
        [SerializeField] internal VisualEffect[] effects = Array.Empty<VisualEffect>();

        #endregion
    }
}