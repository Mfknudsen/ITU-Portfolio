#region Packages

using System;
using Sirenix.OdinInspector;

#endregion

namespace Runtime.AI.Battle.Evaluator
{
    [Serializable]
    public struct EvaluatorSetting
    {
        #region Values

        [FoldoutGroup("Setting")] [TableColumnWidth(25)]
        public int depth;

        [HorizontalGroup("Setting/Con")] public float continuesIncrease;

        [HorizontalGroup("Setting/Toggles")] public bool canSwitchOut;

        [HorizontalGroup("Setting/Toggles")] public bool canUseItems;

        public PersonalitySetting personalitySetting { get; private set; }

        #endregion

        #region Constructors

        public EvaluatorSetting(int depth, float continuesIncrease, bool canSwitchOut, bool canUseItems)
        {
            this.depth = depth;
            this.continuesIncrease = continuesIncrease;
            this.canSwitchOut = canSwitchOut;
            this.canUseItems = canUseItems;
            this.personalitySetting = null;
        }

        public EvaluatorSetting(int depth, float continuesIncrease, bool canSwitchOut, bool canUseItems,
            PersonalitySetting personalitySetting)
        {
            this.depth = depth;
            this.continuesIncrease = continuesIncrease;
            this.canSwitchOut = canSwitchOut;
            this.canUseItems = canUseItems;
            this.personalitySetting = personalitySetting;
        }

        #endregion

        public void SetPersonalitySetting(PersonalitySetting set)
        {
            this.personalitySetting = set;
        }
    }
}