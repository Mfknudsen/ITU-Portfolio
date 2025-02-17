#region Libraries

using System.Collections;
using NUnit.Framework;
using Runtime.Systems.Pooling;
using UnityEngine;
using UnityEngine.TestTools;

#endregion

namespace Tests.Playmode
{
    public class PoolingTests
    {
        [UnityTest]
        public IEnumerator CreatePoolItem()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            #region Before

            GameObject dummyPrefab = new GameObject("DummyPrefab");

            Assert.IsNotNull(dummyPrefab);

            #endregion

            GameObject result = Pool.Create(dummyPrefab);

            Assert.IsNotNull(result);

            bool holderExist = Pool.GetHolder(dummyPrefab.GetHashCode(), out PoolHolder holder);

            Assert.IsTrue(holderExist);

            Assert.IsTrue(holder.MatchPrefab(dummyPrefab));

            GameObject.DestroyImmediate(dummyPrefab);
            GameObject.DestroyImmediate(result);
        }

        [UnityTest]
        public IEnumerator CreatePoolAtTransformItem()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            #region Before

            GameObject dummyPrefab = new GameObject("DummyPrefab");
            Transform dummyTransform = new GameObject("DummyTransform").transform;
            dummyTransform.position = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f));
            dummyTransform.rotation = Quaternion.Euler(
                new Vector3(
                    Random.Range(0, 360f),
                    Random.Range(0, 360f),
                    Random.Range(0, 360f)));

            Assert.IsNotNull(dummyPrefab);

            #endregion

            GameObject result = Pool.CreateAtTransform(dummyPrefab, dummyTransform);

            Assert.IsNotNull(result);

            bool holderExist = Pool.GetHolder(dummyPrefab.GetHashCode(), out PoolHolder holder);

            Assert.IsTrue(holderExist);

            Assert.IsTrue(holder.MatchPrefab(dummyPrefab));

            Assert.IsTrue(dummyTransform.position == result.transform.position);
            Assert.IsTrue(dummyTransform.rotation == result.transform.rotation);

            GameObject.DestroyImmediate(dummyPrefab);
            GameObject.DestroyImmediate(dummyTransform.gameObject);
            GameObject.DestroyImmediate(result);
        }

        [UnityTest]
        public IEnumerator CreatePoolItemAtPositionAndRotation()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;

            #region Before

            GameObject dummyPrefab = new GameObject("DummyPrefab");
            Vector3 dummyPosition = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f),
                Random.Range(-100f, 100f));
            Quaternion dummyRotation = Quaternion.Euler(
                new Vector3(
                    Random.Range(0, 360f),
                    Random.Range(0, 360f),
                    Random.Range(0, 360f)));

            Assert.IsNotNull(dummyPrefab);

            #endregion

            GameObject result = Pool.CreateAtPositionAndRotation(dummyPrefab, dummyPosition, dummyRotation);

            Assert.IsNotNull(result);

            bool holderExist = Pool.GetHolder(dummyPrefab.GetHashCode(), out PoolHolder holder);

            Assert.IsTrue(holderExist);

            Assert.IsTrue(holder.MatchPrefab(dummyPrefab));
            Assert.IsTrue(dummyPosition == result.transform.position);
            Assert.IsTrue(dummyRotation == result.transform.rotation);

            GameObject.DestroyImmediate(dummyPrefab);
            GameObject.DestroyImmediate(result);
        }
    }
}