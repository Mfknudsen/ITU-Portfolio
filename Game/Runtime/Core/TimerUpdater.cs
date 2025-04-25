#region Libraries

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#endregion

namespace Runtime.Core
{
    public static class TimerUpdater
    {
        #region Values

        private static List<Timer> ActiveTimers, InactiveTimers;

        #endregion

        #region Build In States

        private static void Update()
        {
            foreach (Timer activeTimer in ActiveTimers)
                activeTimer.Update();
        }

        #endregion

        #region Set

        internal static void AddToActives(Timer toAdd)
        {
            if (toAdd == null)
                return;

            if (!ActiveTimers.Contains(toAdd))
                ActiveTimers.Add(toAdd);
        }

        internal static void AddToInactives(Timer toAdd)
        {
            if (!InactiveTimers.Contains(toAdd))
                InactiveTimers.Add(toAdd);
        }

        #endregion

        #region Internal

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
#if !UNITY_INCLUDE_TESTS
            return;
#endif
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(Update))
                    playerLoop.subSystemList[i].updateDelegate += Update;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);

            ActiveTimers = new List<Timer>();
            InactiveTimers = new List<Timer>();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += ClearOnExitPlaymode;
#endif
        }

#if UNITY_EDITOR
        private static void ClearOnExitPlaymode(PlayModeStateChange state)
        {
            if (!state.Equals(PlayModeStateChange.ExitingPlayMode))
                return;

            Reset();
        }
#endif

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        private static void Reset()
        {
            ActiveTimers.Clear();
            InactiveTimers.Clear();
        }
#endif

        #endregion

        #region Tests

#if UNITY_INCLUDE_TESTS
        public static void InitializeForTests()
        {
            Initialize();
        }

        public static void ClearForTests()
        {
            Reset();
        }
#endif

        #endregion
    }
}