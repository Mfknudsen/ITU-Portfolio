#region Libraries

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Runtime.AI.Navigation;
using Runtime.World.Overworld;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.TestTools;

#endregion

namespace Tests.Editmode
{
    public sealed class AgentNavigationTests : BaseTestSetup
    {
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NavMeshBaking()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            gameObject.isStatic = true;
            TileSubController controller = gameObject.AddComponent<TileSubController>();
            NavMeshSurface surface = gameObject.AddComponent<NavMeshSurface>();
            surface.BuildNavMesh();

            UniTask<NavigationMesh> task = BuildNavigationMesh.BakeNavmesh(controller, Array.Empty<NavigationPoint>());
            yield return new WaitWhile(() => task.Status == UniTaskStatus.Pending);

            Assert.IsTrue(task.Status == UniTaskStatus.Succeeded);

            NavigationMesh mesh = task.AsTask().Result;

            Assert.IsNotNull(mesh);
            
            surface.RemoveData();
            GameObject.DestroyImmediate(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator NavMeshBakingNullControllerError()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            TileSubController nullController = null;
            
            Assert.IsNull(nullController);
            
            // ReSharper disable once ExpressionIsAlwaysNull
            UniTask<NavigationMesh> task = BuildNavigationMesh.BakeNavmesh(nullController, Array.Empty<NavigationPoint>());

            yield return new WaitWhile(() => task.Status == UniTaskStatus.Pending);
            
            Assert.IsTrue(task.Status == UniTaskStatus.Succeeded);

            NavigationMesh mesh = task.AsTask().Result;
            
            Assert.IsNull(mesh);
        }

        [UnityTest]
        public IEnumerator OdinNavigationMeshBaking()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}