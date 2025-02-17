#region Libraries

using System.Collections;
using NUnit.Framework;
using Runtime.World;
using Runtime.World.Overworld.Lights;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

#endregion

namespace Tests.Playmode
{
    public class WordTimeTests
    {
        [UnityTest]
        public IEnumerator SetCurrentDayTimeEnum()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            new GameObject("DummyLight").AddComponent<DayTimeLight>();

            WorldTime.SetCurrentDayTime(WorldTimeZone.Midnight);

            Assert.IsTrue(WorldTime.GetCurrentTime() == 0);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Morning);

            Assert.IsTrue(WorldTime.GetCurrentTime() == 6);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Evening);

            Assert.IsTrue(WorldTime.GetCurrentTime() == 12);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Afternoon);

            Assert.IsTrue(WorldTime.GetCurrentTime() == 16);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Night);

            Assert.IsTrue(WorldTime.GetCurrentTime() == 20);
        }

        [UnityTest]
        public IEnumerator AddRemoveLights()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            Assert.IsTrue(WorldTime.GetLights().Count == 0);

            GameObject lightObj = new GameObject("DummyLight");
            lightObj.AddComponent<DayTimeLight>();

            Assert.IsTrue(WorldTime.GetLights().Count == 1);

            Object.Destroy(lightObj);

            Assert.IsTrue(WorldTime.GetLights().Count == 0);
        }

        [UnityTest]
        public IEnumerator SetTimeByEnum()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            GameObject lightObj = new GameObject("DummyLight");
            lightObj.AddComponent<DayTimeLight>().SetLight(lightObj.AddComponent<Light>());
            lightObj.GetComponent<DayTimeLight>().Validate();

            yield return null;

            Assert.IsTrue(WorldTime.GetLights().Count > 0);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Midnight);
            Assert.IsTrue(WorldTime.GetCurrentTime() == 0);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Morning);
            Assert.IsTrue(WorldTime.GetCurrentTime() == 6);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Evening);
            Assert.IsTrue(WorldTime.GetCurrentTime() == 12);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Afternoon);
            Assert.IsTrue(WorldTime.GetCurrentTime() == 16);

            WorldTime.SetCurrentDayTime(WorldTimeZone.Night);
            Assert.IsTrue(WorldTime.GetCurrentTime() == 20);
        }

        [UnityTest]
        public IEnumerator SetTimeByFloat()
        {
            //Equivalence and boundary test

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            GameObject lightObj = new GameObject("DummyLight");
            lightObj.AddComponent<DayTimeLight>().SetLight(lightObj.AddComponent<Light>());
            lightObj.GetComponent<DayTimeLight>().Validate();

            yield return null;

            Assert.IsTrue(WorldTime.GetLights().Count > 0);

            #region Equivalence

            Vector2[] partitions =
            {
                new Vector2(-1, 1), //Midnight
                new Vector2(1, 6), //Night
                new Vector2(6, 12), //Morning
                new Vector2(12, 16), //Evening
                new Vector2(16, 20), //Afternoon
                new Vector2(20, 23) //Night
            };

            WorldTimeZone[] successfulResults =
            {
                WorldTimeZone.Midnight,
                WorldTimeZone.Night,
                WorldTimeZone.Morning,
                WorldTimeZone.Evening,
                WorldTimeZone.Afternoon,
                WorldTimeZone.Night
            };

            for (int i = 0; i < partitions.Length; i++)
            {
                float time = Random.Range(partitions[i].x, partitions[i].y);
                WorldTime.SetCurrentDayTime(time);
                Assert.IsTrue(WorldTime.GetCurrentTime() is >= 0 or <= 24);
                Assert.IsTrue(WorldTime.GetCurrentTimeZone() == successfulResults[i]);
            }

            #endregion

            #region Boundary

            for (int i = 0; i < partitions.Length; i++)
            {
            }

            #endregion
        }
    }
}