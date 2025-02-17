#region Libraries

using NUnit.Framework;
using Runtime.World;
using Runtime.World.Overworld.Lights;
using UnityEngine;

#endregion

namespace Tests.Editmode
{
    public class WorldTimeTests
    {
        [Test]
        public void TestOdinSaveSetting()
        {
            Color color = Color.red;

            GameObject obj = new GameObject("DummyLight");

            Light light = obj.AddComponent<Light>();
            light.color = color;

            Assert.IsTrue(light.color == color);

            DayTimeLight dayTimeLight = obj.AddComponent<DayTimeLight>();
            dayTimeLight.Validate();
            dayTimeLight.SetLight(light);

            for (int i = 0; i < 5; i++)
            {
                WorldTimeZone z = (WorldTimeZone)i;
                dayTimeLight.SetSaveAsLabel(z);
                Assert.IsTrue(dayTimeLight.GetLabel() == z);
                dayTimeLight.TestOdinSaveSetting();
                Assert.IsTrue(dayTimeLight.GetSettings()[(int)z].GetLightColor() == color);
            }
        }
    }
}