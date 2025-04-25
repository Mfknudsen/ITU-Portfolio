#region Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Pokémon;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace Runtime.World.Overworld.Spawner
{
    public sealed class SpawnLocation : MonoBehaviour
    {
        #region Values

        [SerializeField] [HideReferenceObjectPicker]
        private ISpawnType spawnType = new Area();

        [SerializeField] private Pokemon[] allowedToSpawn = Array.Empty<Pokemon>();

#if UNITY_EDITOR
        [ValueDropdown("ValueOptions")] public string selectedDropdownOption = "Area";

#pragma warning disable IDE0052 // Remove unread private members
        private static readonly string[] ValueOptions = { "Area", "Points" };
#pragma warning restore IDE0052 // Remove unread private members
#endif

        #endregion

        #region Build In States

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.name = (this.spawnType is Area ? "Area" : "Point") + " Spawn Location";

            if (this.spawnType is not Location && this.selectedDropdownOption.Equals("Points"))
                this.spawnType = new Location();

            if (this.spawnType is not Area && this.selectedDropdownOption.Equals("Area"))
                this.spawnType = new Area();

            this.spawnType.Validate(this);
        }

        private void OnDrawGizmosSelected()
        {
            this.spawnType.Gizmo(this);
        }
#endif

        #endregion

        #region Getters

#if UNITY_EDITOR
        public bool IsAreaSpawnType => this.spawnType is Area;

        public Vector3[] GetAreaPoints => ((Area)this.spawnType).GetPoints;

        public Transform[] GetLocationPoints => ((Location)this.spawnType).GetTransforms;

#endif

        #endregion

        #region Setters

#if UNITY_EDITOR
        public void SetAreaPointPosition(int i, Vector3 pos)
        {
            ((Area)this.spawnType).SetPointByID(i, pos);
        }
#endif

        #endregion

        #region In

#if UNITY_EDITOR

        public void CreateAreaPoint()
        {
            ((Area)this.spawnType).AddPoint(this.transform.position);
        }

        public bool TryRemoveAreaPoint(int id)
        {
            return ((Area)this.spawnType).TryRemovePoint(id);
        }

        public bool TryCreateNewAreaTriangle(int[] ids)
        {
            return ((Area)this.spawnType).TryCreateNewTriangle(ids);
        }

        public bool TryRemoveAreaTriangle(int[] ids)
        {
            return ((Area)this.spawnType).TryRemoveTriangle(ids);
        }

        public bool TryCleanAreaPoints()
        {
            return ((Area)this.spawnType).TryCleanPoints();
        }

#endif

        #endregion

        #region Out

        public bool Allowed(Pokemon pokemon)
        {
            return this.allowedToSpawn.Count() == 0 || this.allowedToSpawn.Contains(pokemon);
        }

        public SpawnTypeResult GetSpawnResult => this.spawnType.GetResult();

        #endregion
    }

    public struct SpawnTypeResult
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    #region Area Types

    internal interface ISpawnType
    {
#if UNITY_EDITOR
        public void Gizmo(SpawnLocation self);

        public void Validate(SpawnLocation self);
#endif

        public SpawnTypeResult GetResult();
    }

    [Serializable]
    internal struct Location : ISpawnType
    {
        #region Values

        [SerializeField] private Transform[] locations;

        #endregion

        public SpawnTypeResult GetResult()
        {
            Transform t = this.locations[Random.Range(0, this.locations.Length)];

            return new SpawnTypeResult
            {
                Position = t.position,
                Rotation = t.rotation
            };
        }

        #region Getters

#if UNITY_EDITOR
        public Transform[] GetTransforms => this.locations;
#endif

        #endregion

        #region In

#if UNITY_EDITOR
        public void Gizmo(SpawnLocation self)
        {
        }

        public void Validate(SpawnLocation self)
        {
            this.locations ??= new Transform[0];
        }
#endif

        #endregion
    }

    [Serializable]
    internal struct Area : ISpawnType
    {
        #region Values

        [SerializeField] private List<Vector3> storedPoints;
        [SerializeField] private List<Vector3Int> storedTriangles;

        #endregion

        #region Getters

#if UNITY_EDITOR
        public Vector3[] GetPoints => this.storedPoints.ToArray();
#endif

        #endregion

        #region Setters

#if UNITY_EDITOR
        public void SetPointByID(int i, Vector3 position)
        {
            this.storedPoints[i] = position;
        }
#endif

        #endregion

        #region In

#if UNITY_EDITOR
        public void Gizmo(SpawnLocation self)
        {
            for (int i = 0; i < this.storedTriangles.Count; i++)
            {
                Vector3Int ids = this.storedTriangles[i];
                Debug.DrawLine(this.storedPoints[ids.x], this.storedPoints[ids.y], Color.red);
                Debug.DrawLine(this.storedPoints[ids.x], this.storedPoints[ids.z], Color.red);
                Debug.DrawLine(this.storedPoints[ids.z], this.storedPoints[ids.y], Color.red);
            }
        }

        public void Validate(SpawnLocation self)
        {
            this.storedPoints ??= new List<Vector3>(0);
            this.storedTriangles ??= new List<Vector3Int>(0);

            if (this.storedPoints.Count < 3)
            {
                for (int i = this.storedPoints.Count; i < 3; i++)
                    this.storedPoints.Add(self.transform.position);
            }

            if (this.storedTriangles.Count == 0)
                this.storedTriangles.Add(new Vector3Int(0, 1, 2));
        }

        public void AddPoint(Vector3 startPosition)
        {
            this.storedPoints.Add(startPosition);
        }

        public bool TryRemovePoint(int toDelete)
        {
            if (this.storedTriangles.Count == 1)
            {
                Vector3Int t = this.storedTriangles[0];
                if (t.x == toDelete || t.y == toDelete || t.z == toDelete)
                    return false;
            }

            if (this.storedPoints.Count < toDelete - 1)
                return false;

            this.storedPoints.RemoveAt(toDelete);

            List<Vector3Int> replaceList = new List<Vector3Int>();
            for (int i = 0; i < this.storedTriangles.Count; i++)
            {
                Vector3Int v = this.storedTriangles[i];

                if (v.x == toDelete || v.y == toDelete || v.z == toDelete)
                    continue;
                Debug.Log("Adding: " + v.ToString());
                replaceList.Add(v);
            }

            this.storedTriangles = replaceList;
            Debug.Log(this.storedTriangles.Count);

            return true;
        }

        public bool TryCreateNewTriangle(int[] ids)
        {
            Vector3Int t = new Vector3Int(ids[0], ids[1], ids[2]);

            foreach (Vector3Int v in this.storedTriangles)
            {
                if ((v.x == t.x || v.x == t.y || v.x == t.z) &&
                    (v.y == t.x || v.y == t.y || v.y == t.z) &&
                    (v.z == t.x || v.z == t.y || v.z == t.z))
                    return false;
            }

            this.storedTriangles.Add(t);

            return true;
        }

        public bool TryRemoveTriangle(int[] ids)
        {
            if (this.storedTriangles.Count == 1)
                return false;

            foreach (Vector3Int v in this.storedTriangles)
            {
                if (!(v.x == ids[0] || v.x == ids[1] || v.x == ids[2]) ||
                    !(v.y == ids[0] || v.y == ids[1] || v.y == ids[2]) ||
                    !(v.z == ids[0] || v.z == ids[1] || v.z == ids[2]))
                    continue;

                this.storedTriangles.Remove(v);

                return true;
            }

            return false;
        }

        public bool TryCleanPoints()
        {
            bool changeHappend = false;

            List<int> toRemove = new List<int>();
            for (int i = 0; i < this.storedPoints.Count; i++)
            {
                if (!this.storedTriangles.Any(v => v.x == i || v.y == i || v.z == i))
                {
                    toRemove.Add(i);
                    changeHappend = true;
                }
            }

            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                this.storedPoints.RemoveAt(toRemove[i]);
                this.RecalculateIndexs(toRemove[i]);
            }

            return changeHappend;
        }
#endif

        #endregion

        #region Out

        public SpawnTypeResult GetResult()
        {
            Vector3Int ids = this.storedTriangles[Random.Range(0, this.storedTriangles.Count)];
            Vector3 dirA = this.storedPoints[ids.y] - this.storedPoints[ids.x],
                dirB = this.storedPoints[ids.z] - this.storedPoints[ids.x];

            float aRandom = Random.Range(0f, 1f), bRandom = Random.Range(0f, 1f);

            if (!(aRandom + bRandom <= 1))
            {
                aRandom = 1 - aRandom;
                bRandom = 1 - bRandom;
            }

            Vector3 newPoint = this.storedPoints[ids.x] +
                               Vector3.Lerp(Vector3.zero, dirA, aRandom) +
                               Vector3.Lerp(Vector3.zero, dirB, bRandom);

            return new SpawnTypeResult
            {
                Position = newPoint,
                Rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0))
            };
        }

        #endregion

        #region Internal

#if UNITY_EDITOR
        private void RecalculateIndexs(int removed)
        {
            for (int i = 0; i < this.storedTriangles.Count; i++)
            {
                Vector3Int v = this.storedTriangles[i];
                v = new Vector3Int(
                    v.x > removed ? v.x - 1 : v.x,
                    v.y > removed ? v.y - 1 : v.y,
                    v.z > removed ? v.z - 1 : v.z
                );

                this.storedTriangles[i] = v;
            }
        }
#endif

        #endregion
    }

    #endregion
}