#region Libraries

using System;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Lights
{
    [Serializable]
    public struct DayTimeLightSettings
    {
        #region Values

        [SerializeField] private Color lightColor;
        [SerializeField] private Vector3 rotation;

#if UNITY_EDITOR
        [SerializeField] [HideInInspector] private WorldTimeZone label;
#endif

        #endregion

        #region Build In States

#if UNITY_EDITOR
        public DayTimeLightSettings(WorldTimeZone label)
        {
            this.label = label;
            this.lightColor = Color.white;
            this.rotation = Vector3.zero;
        }
#endif

        #endregion

        #region Getters

        public Color GetLightColor()
        {
            return this.lightColor;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(this.rotation);
        }

        #endregion

        #region Setters

#if UNITY_EDITOR
        public void SetValues(Light light)
        {
            this.lightColor = light.color;
        }
#endif

        #endregion
    }
}