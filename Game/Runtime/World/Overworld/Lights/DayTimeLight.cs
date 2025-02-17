#region Libraries

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Lights
{
    public sealed class DayTimeLight : MonoBehaviour
    {
        #region Values

        [SerializeField] [PropertyOrder(-2)] [Required]
        private Light affectedLight;

        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "label", HideRemoveButton = true, HideAddButton = true,
            DraggableItems = false)]
        private List<DayTimeLightSettings> settings = new List<DayTimeLightSettings>();

#if UNITY_EDITOR
        [ShowInInspector] [HorizontalGroup("Save")] [ValueDropdown(nameof(saveAsOptions))] [HideLabel]
        private WorldTimeZone saveAsLabel;

        private WorldTimeZone[] saveAsOptions =
        {
            WorldTimeZone.Midnight, WorldTimeZone.Morning, WorldTimeZone.Evening, WorldTimeZone.Afternoon,
            WorldTimeZone.Night
        };
#endif

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = this.settings.Count; i < Enum.GetValues(typeof(WorldTimeZone)).Length; i++)
                this.settings.Add(new DayTimeLightSettings((WorldTimeZone)i));
        }
#endif

        private void OnEnable()
        {
            WorldTime.AddLight(this);
        }

        private void OnDisable()
        {
            WorldTime.RemoveLight(this);
        }

        #endregion

        #region In

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="towards"></param>
        /// <param name="interpolateTime">Between 0 - 1</param>
        public void Interpolate(WorldTimeZone from, WorldTimeZone towards, float interpolateTime)
        {
        }

        #endregion

        #region Internal

#if UNITY_EDITOR
        [HorizontalGroup("Save")]
        [PropertyOrder(-1)]
        [Button]
        private void SaveSetting()
        {
            DayTimeLightSettings setting = this.settings[(int)this.saveAsLabel];
            setting.SetValues(this.affectedLight);
            this.settings[(int)this.saveAsLabel] = setting;
        }
#endif

        #endregion

        #region Tests

#if UNITY_INCLUDE_TESTS
        public void SetSaveAsLabel(WorldTimeZone label)
        {
            this.saveAsLabel = label;
        }

        public void Validate()
        {
            this.OnValidate();
        }

        public void TestOdinSaveSetting()
        {
            this.SaveSetting();
        }

        public List<DayTimeLightSettings> GetSettings()
        {
            return this.settings;
        }

        public void SetLight(Light set)
        {
            this.affectedLight = set;
        }

        public WorldTimeZone GetLabel()
        {
            return this.saveAsLabel;
        }
#endif

        #endregion
    }
}