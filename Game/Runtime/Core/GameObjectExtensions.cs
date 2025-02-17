#region Packages

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.Core
{
    public static class GameObjectExtensions
    {
        public static T[] GetAllComponentsByRoot<T>(this GameObject gameObject) where T : Component
        {
            List<T> result = new List<T>();

            if (gameObject.GetComponent<T>() is { } component)
                result.Add(component);

            foreach (Transform child in gameObject.transform)
                result.AddRange(child.gameObject.GetAllComponentsByRoot<T>());

            return result.Where(i => i != null)
                .ToArray();
        }

        public static T GetFirstComponentByRoot<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.GetComponent<T>() is { } component)
                return component;

            List<GameObject> toCheck = (from Transform t in gameObject.transform select t.gameObject)
                .ToList();

            while (toCheck.Count > 0)
            {
                GameObject child = toCheck[0];

                if (child.GetComponent<T>() is { } childComponent)
                    return childComponent;

                toCheck.AddRange(from Transform t in child.transform select t.gameObject);

                toCheck.RemoveAt(0);
            }

            return null;
        }

        public static T[] GetAllMonoBehavioursByRoot<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            List<T> result = new List<T>();

            if (gameObject.GetComponent<T>() is { } monoBehaviour)
                result.Add(monoBehaviour);

            foreach (Transform child in gameObject.transform)
                result.AddRange(child.gameObject.GetAllComponentsByRoot<T>());

            return result.Where(i => i != null).ToArray();
        }

        public static T GetFirstComponentTowardsRoot<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            while (true)
            {
                if (gameObject.transform.parent == null) return null;

                T component = gameObject.GetComponent<T>();

                if (component != null) return component;

                gameObject = gameObject.transform.parent.gameObject;
            }
        }

        public static GameObject GetChildByName(this GameObject parent, string name)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Transform t in parent.transform)
            {
                if (t.name.Equals(name))
                    return t.gameObject;
            }

            return null;
        }

        public static Transform LocalDefaults(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            return transform;
        }
    }
}