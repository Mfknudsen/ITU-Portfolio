#region Packages

using Runtime.AI.Battle.Evaluator;

#endregion

namespace Runtime.Settings
{
    public static class GameplaySetting
    {
        private static readonly EvaluatorSetting EasyEvaluatorSetting, MediumEvaluatorSetting, HardEvaluatorSetting;

        private static bool invertX, invertY;
        private static float mouseSensitivity; 

        static GameplaySetting()
        {
            PersonalitySetting personalitySetting = new PersonalitySetting();

            EasyEvaluatorSetting =
                new EvaluatorSetting(1, 10, false, false, personalitySetting);
            MediumEvaluatorSetting =
                new EvaluatorSetting(3, 5, true, false, personalitySetting);
            HardEvaluatorSetting =
                new EvaluatorSetting(5, 0, true, true, personalitySetting);
        }

        #region Getters

        public static EvaluatorSetting GetDefaultEvaluatorSetting(Difficultly difficultly)
        {
            return difficultly switch
            {
                Difficultly.Easy => EasyEvaluatorSetting,
                Difficultly.Medium => MediumEvaluatorSetting,
                _ => HardEvaluatorSetting,
            };
        }

        #endregion

        #region Setters

        public static void SetInvert(bool set, string axis)
        {
            if (axis.Equals("X"))
                invertX = set;
            else
                invertY = set;
        }

        public static void SetMouseSensitivity(float set)
        {
            mouseSensitivity = set;
        }

        #endregion
        
        #region In

        public static void ApplySettings()
        {
            
        }

        #endregion
    }
}