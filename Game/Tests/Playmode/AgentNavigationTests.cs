#region Libraries

using System.Collections;
using Runtime.AI.Navigation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

#endregion

namespace Tests.Playmode
{
    public sealed class AgentNavigationTests : BaseTestSetup
    {
        [UnityTest]
        public IEnumerator AgentAddRemove()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitWhile(() => UnitNavigation.GetAllAgents() == null);

            UnitNavigation.SetNavMesh(BuildNavigationMesh.BuildDummyMesh());
            Assert.IsTrue(UnitNavigation.GetAllAgents().Count == 0);

            GameObject obj = new GameObject("DummyAgent");
            obj.SetActive(false);
            obj.AddComponent<NavigationAgent>().SetSettings(UnitAgentSettings.CreateDummySettings());
            obj.SetActive(true);
            Assert.IsTrue(UnitNavigation.GetAllAgents().Count == 1);
            obj.SetActive(false);
            Assert.IsTrue(UnitNavigation.GetAllAgents().Count == 0);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ApplyNavigationMeshToRuntimeSingletonManager()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            #region Before

            //Dummy data for testing
            NavigationMesh dummyNavigationMesh = BuildNavigationMesh.BuildDummyMesh();

            Assert.IsFalse(UnitNavigation.Ready);

            #endregion

            #region Test 1 - Apply not empty navigatio mesh.

            UnitNavigation.SetNavMesh(dummyNavigationMesh);

            yield return null;

            Assert.IsTrue(UnitNavigation.Ready);

            #endregion

            #region Test 2 - Apply empty navigation mesh.

            //To ensure agents don't break. An empty navigation mesh can't be applied.
            UnitNavigation.SetNavMesh(null);

            yield return null;

            Assert.IsTrue(UnitNavigation.Ready);

            #endregion

            #region After

            //Delete dummy data.
            BuildNavigationMesh.DestroyDummyMesh(dummyNavigationMesh);

            #endregion
        }
    }
}