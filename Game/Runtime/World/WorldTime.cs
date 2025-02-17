#region Libraries

using System.Collections.Generic;
using Runtime.World.Overworld.Lights;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#endregion

namespace Runtime.World
{
    #region Enums

    /// <summary>
    /// Total of 5 different times of day. 0 - 4
    /// </summary>
    public enum WorldTimeZone
    {
        Midnight,
        Morning,
        Evening,
        Afternoon,
        Night
    }

    #endregion

    public static class WorldTime
    {
        #region Values

        /// <summary>
        /// Time in hours (00 - 24)
        /// </summary>
        private static float _currentTime;

        private const int RealtimeMinutesToADay = 60;

        private static List<DayTimeLight> _lights;

        private static WorldTimeZone currentTimeZone;

        #endregion

        #region Getters

#if UNITY_EDITOR
        public static IReadOnlyList<DayTimeLight> GetLights()
        {
            return _lights;
        }

        public static float GetCurrentTime()
        {
            return _currentTime;
        }

        public static WorldTimeZone GetCurrentTimeZone()
        {
            return currentTimeZone;
        }
#endif

        #endregion

        #region In

        public static void SetCurrentDayTime(float set)
        {
            _currentTime = set;
            currentTimeZone = NumberToDayTime(_currentTime);
            WorldTimeZone nextWorldTimeZone = currentTimeZone + 1 <= WorldTimeZone.Night
                ? currentTimeZone + 1
                : WorldTimeZone.Midnight;
            int timeCurrent = DayTimeToNumber(currentTimeZone), timeNext = DayTimeToNumber(nextWorldTimeZone);
            float timeInterloped = 1f / (timeNext - timeCurrent) * (_currentTime - timeCurrent);
            UpdateRealtimeLights(currentTimeZone, nextWorldTimeZone, timeInterloped);
        }

        public static void SetCurrentDayTime(WorldTimeZone set)
        {
            _currentTime = DayTimeToNumber(set);
            UpdateRealtimeLights(set, set == WorldTimeZone.Night ? WorldTimeZone.Midnight : set + 1, 0);
        }

        public static void AddLight(DayTimeLight light)
        {
            _lights.Add(light);
        }

        public static void RemoveLight(DayTimeLight light)
        {
            _lights.Remove(light);
        }

        #endregion

        #region Internal

        private static int DayTimeToNumber(WorldTimeZone worldTimeZone)
        {
            return worldTimeZone switch
            {
                WorldTimeZone.Midnight => 0,
                WorldTimeZone.Morning => 6,
                WorldTimeZone.Evening => 12,
                WorldTimeZone.Afternoon => 16,
                _ => 20
            };
        }

        private static WorldTimeZone NumberToDayTime(float time)
        {
            if (time < 0)
                time = 24.0f - Mathf.Abs(time) % 24.0f;
            else
                time %= 24.0f;

            return time is < 1 or > 23 ? WorldTimeZone.Midnight :
                time is < 12 and > 6 ? WorldTimeZone.Morning :
                time is < 16 and > 12 ? WorldTimeZone.Evening :
                time is < 20 and > 16 ? WorldTimeZone.Afternoon :
                WorldTimeZone.Night;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type != typeof(Update))
                    continue;

                playerLoop.subSystemList[i].updateDelegate += UpdateTimeLighting;

                break;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);

            SetCurrentDayTime(12);

            _lights = new List<DayTimeLight>();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Clean up on exiting play mode.
        /// </summary>
        /// <param name="state">State giving by Unity</param>
        private static void OnExitPlayMode(PlayModeStateChange state)
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type != typeof(Update))
                    continue;

                playerLoop.subSystemList[i].updateDelegate -= UpdateTimeLighting;

                break;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }
#endif

        private static void UpdateTimeLighting()
        {
            _currentTime += 24f / RealtimeMinutesToADay * Time.deltaTime;

            if (_currentTime > 24f)
                _currentTime -= 24f;

            WorldTimeZone currentWorldTimeZone = NumberToDayTime(_currentTime),
                nextWorldTimeZone = currentWorldTimeZone + 1 <= WorldTimeZone.Night
                    ? currentWorldTimeZone + 1
                    : WorldTimeZone.Midnight;
            int timeCurrent = DayTimeToNumber(currentWorldTimeZone), timeNext = DayTimeToNumber(nextWorldTimeZone);
            float timeInterloped = 1f / (timeNext - timeCurrent) * (_currentTime - timeCurrent);
            UpdateLightmap(currentWorldTimeZone, nextWorldTimeZone, timeInterloped);
            UpdateRealtimeLights(currentWorldTimeZone, nextWorldTimeZone, timeInterloped);
        }

        private static void UpdateRealtimeLights(WorldTimeZone from, WorldTimeZone towards, float time)
        {
            if (_lights == null)
                return;

            foreach (DayTimeLight dayTimeLight in _lights)
            {
                if (dayTimeLight != null)
                    dayTimeLight.Interpolate(from, towards, time);
            }
        }

        private static void UpdateLightmap(WorldTimeZone from, WorldTimeZone towards, float time)
        {
            LightmapData[] current = GetLightMapData(from),
                next = GetLightMapData(towards);

            List<LightmapData> result = new List<LightmapData>();

            for (int i = 0; i < current.Length; i++)
            {
                LightmapData data = new LightmapData
                {
                    lightmapColor = new Texture2D(current[i].lightmapColor.width, current[i].lightmapColor.height)
                };

                for (int x = 0; x < current[i].lightmapColor.width; x++)
                {
                    for (int y = 0; y < current[i].lightmapColor.height; y++)
                    {
                        data.lightmapColor.SetPixel(x, y, Color.Lerp(
                            current[i].lightmapColor.GetPixel(x, y),
                            next[i].lightmapColor.GetPixel(x, y),
                            time
                        ));
                    }
                }

                result.Add(data);
            }

            LightmapSettings.lightmaps = result.ToArray();
        }

        private static LightmapData[] GetLightMapData(WorldTimeZone timeZone)
        {
            return new LightmapData[0];
        }

        #endregion
    }
}