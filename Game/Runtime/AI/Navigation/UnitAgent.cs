#region Libraries

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.EntityComponents;
using Runtime.AI.Navigation.PathActions;
using Runtime.AI.Navigation.RayCast;
using Runtime.Core;
using Runtime.Editor.Tests;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.AI.Navigation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public sealed class UnitAgent : MonoBehaviour
    {
        #region Values

        private Entity? entity;

        [ShowInInspector] private int agentID = -1, groupID = -1;

        [SerializeField] [InlineEditor] [Required]
        private UnitAgentSettings settings;

        [ShowInInspector] private UnitPath currentPath;

        private int currentTriangleIndex = -1;

        [SerializeField] [HideInInspector] private Rigidbody rb;

        private bool pathPending, isOnNavMesh, isStopped = true;

        private UnityEvent onPathComplete;

        private const int AvoidanceCheckStep = 10;

        private Vector2 currentNavMeshPathDirection = Vector2.zero;

        private const float GroupCenterPull = .35f;

        private Vector3 dir = Vector3.zero;

        private List<NavigationRayCastObject> navigationRayCastObjects;

        #endregion

        #region Build In States

        private void OnDrawGizmos()
        {
            return;
            if (this.groupID != -1)
            {
                UnitWalkGroup group = UnitNavigation.GetGroupByIndex(this.groupID);
                Debug.DrawLine(this.transform.position + Vector3.up,
                    new Vector3(group.GetCenter().x, 2, group.GetCenter().y));

                Debug.DrawRay(group.GetCenter().ToV3(2), Vector3.up, Color.red);
            }

            Debug.DrawRay(this.transform.position + Vector3.up,
                this.currentNavMeshPathDirection.ToV3(0).normalized,
                Color.red);
            Debug.DrawRay(this.transform.position + Vector3.up * 1.25f, this.dir.normalized, Color.green);
        }

        private void Reset()
        {
            this.rb = this.gameObject.GetComponent<Rigidbody>();
            this.rb.useGravity = false;
            this.rb.constraints = RigidbodyConstraints.FreezeRotation;

            CapsuleCollider capsuleCollider = this.GetComponent<CapsuleCollider>();
            capsuleCollider.center = Vector3.up;
            capsuleCollider.height = 2;
        }

        private void OnEnable()
        {
            this.entity = UnitNavigation.AddAgent(this);
        }

        private void OnDisable()
        {
            UnitNavigation.RemoveAgent(this, this.entity);
            this.entity = null;
        }

        private IEnumerator Start()
        {
            this.onPathComplete = new UnityEvent();

            yield return new WaitUntil(() => UnitNavigation.Ready);

            //this.currentTriangleIndex = UnitNavigation.PlaceAgentOnNavMesh(this);

            //this.isOnNavMesh = this.currentTriangleIndex != -1;

            if (this.isOnNavMesh)
                this.rb.useGravity = true;
        }

        private void Update()
        {
            if (!this.entity.HasValue) return;

            DestinationComponent destinationComponent = Unity.Entities.World.DefaultGameObjectInjectionWorld
                .EntityManager.GetComponentData<DestinationComponent>(this.entity.Value);

            this.transform.position +=
                (Vector3)destinationComponent.MoveDirection * (this.settings.MoveSpeed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (!this.entity.HasValue) return;

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld
                .EntityManager;
            LocalTransform localTransformComponent = entityManager.GetComponentData<LocalTransform>(this.entity.Value);
            localTransformComponent.Position = this.transform.position;
            localTransformComponent.Rotation = this.transform.rotation;
            entityManager.SetComponentData(this.entity.Value, localTransformComponent);
        }

        #endregion

        #region Getters

        public int CurrentTriangleIndex()
        {
            return this.currentTriangleIndex;
        }

        public UnitAgentSettings Settings => this.settings;

        public bool IsStopped()
        {
            return this.isStopped;
        }

        public bool IsOnNavMesh()
        {
            return this.isOnNavMesh;
        }

        public bool HasPath()
        {
            return !this.currentPath.Empty;
        }

        public Vector2 GetCurrentNavMeshDirection()
        {
            return this.currentNavMeshPathDirection;
        }

        internal int GetID()
        {
            return this.agentID;
        }

        internal int GetGroupID()
        {
            return this.groupID;
        }

        public bool HasEntity()
        {
            return this.entity != null &&
                   Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager.Exists(this.entity.Value);
        }

#if UNITY_EDITOR
        public List<NavigationRayCastObject> GetRayCastObjects()
        {
            return this.navigationRayCastObjects;
        }
#endif

        #endregion

        #region Setters

        public void SetStopped(bool set)
        {
            this.isStopped = set;
        }

        internal void SetID(int set)
        {
            this.agentID = set;
        }

        internal void SetGroupID(int set)
        {
            this.groupID = set;
        }

        #endregion

        #region In

        public void UpdateInTriangle()
        {
        }

        public void UpdateIntendedDirection()
        {
            this.currentNavMeshPathDirection = Vector2.zero;

            if (this.currentPath is not { Empty: false, Complete: false })
                return;

            PathAction pathAction = this.currentPath.GetCurrentPathAction();
            this.currentNavMeshPathDirection = (pathAction.Destination() - this.transform.position).XZ();
        }

        internal void UpdateAgent()
        {
            if (this.currentPath is not { Empty: false, Complete: false } || this.isStopped)
                return;

            PathAction pathAction = this.currentPath.GetCurrentPathAction();
            if (pathAction.IsWalkAction())
            {
                Vector3 position = this.transform.position;
                Vector3 currentMoveVector = (pathAction.Destination() - position).Mul(y: 0).normalized;
                Vector3 newDirection = this.AgentAvoidance(currentMoveVector);
                currentMoveVector = Vector3.Lerp(currentMoveVector, newDirection, Time.deltaTime);
                this.dir = currentMoveVector;

                Vector3 lookDirection = Vector3.RotateTowards(this.transform.forward,
                    currentMoveVector,
                    this.settings.TurnSpeed * Time.deltaTime,
                    0f).Mul(y: 0);
                this.transform.rotation = Quaternion.LookRotation(lookDirection);

                //if (Vector3.Angle(this.transform.forward, currentMoveVector) < this.settings.WalkTurnAngle)
                this.rb.MovePosition(position + currentMoveVector.normalized * this.settings.MoveSpeed *
                    Time.deltaTime);
            }
            else
            {
            }

            this.currentPath.CheckIndex(this);

            if (this.currentPath.Complete)
                this.onPathComplete.Invoke();
        }

        public void MoveTo(Vector3 position)
        {
            if (this.entity == null) return;

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<DestinationComponent>(this.entity.Value))
            {
                DestinationComponent component =
                    entityManager.GetComponentData<DestinationComponent>(this.entity.Value);

                component.Point = position;
                component.Stop = false;
                component.MoveDirection = float3.zero;
                component.CurrentPathIndex = 0;
                entityManager.SetComponentData(this.entity.Value, component);
                entityManager.SetComponentEnabled<DestinationComponent>(this.entity.Value, true);
            }

            return;
            if (this.PositionInCurrentTriangle(position))
            {
                NavTriangle t = UnitNavigation.GetTriangleByID(this.currentTriangleIndex);
                MathC.PointWithinTriangle2D(position.XZ(),
                    UnitNavigation.Get2DVertByIndex(t.GetA),
                    UnitNavigation.Get2DVertByIndex(t.GetB),
                    UnitNavigation.Get2DVertByIndex(t.GetC),
                    out float w1, out float w2);
                Debug.Log($"{w1}  |  {w2}");
            }

            if (this.PositionInCurrentTriangle(position)) return;

            UnitNavigation.QueueForPath(this, position);
        }

        public void MoveToAndFace(Vector3 position, Quaternion direction)
        {
            if (this.PositionInCurrentTriangle(position))
            {
                if (Vector3.Angle(this.transform.forward, direction.ForwardFromRotation()) >
                    this.settings.WalkTurnAngle)
                {
                    //
                }

                return;
            }

            UnitNavigation.QueueForPath(this, position);
        }

        public void SetPath(UnitPath path)
        {
            this.currentPath = path;
        }

        internal void Place(Vector3 position)
        {
            this.transform.position =
                position + Vector3.up * this.GetComponent<CapsuleCollider>().center.y / 2f;

            this.rb.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Move the agent directly without affecting the current path or calculating a new path.
        /// </summary>
        /// <param name="moveDirection">Move direction. Expects normalized direction.</param>
        public void Move(Vector3 moveDirection)
        {
            this.rb.MovePosition(this.transform.position + moveDirection * this.settings.MoveSpeed * Time.deltaTime);
        }

        #endregion

        #region Internal

        /// <summary>
        ///     Attempt to avoid walking into other agents
        /// </summary>
        /// <param name="currentMoveDirection">Current move direction from the agent to it's destination</param>
        /// <returns>Offset move vector</returns>
        private Vector3 AgentAvoidance(Vector3 currentMoveDirection)
        {
            //Move the agent closer towards the center of the group.
            if (this.groupID != -1)
            {
                Vector3 towardsGroupCenter =
                    (UnitNavigation.GetGroupByIndex(this.groupID).GetCenter() - this.transform.position.XZ()).ToV3(
                        this.transform.position.y).normalized;
                float affect = GroupCenterPull * ((-Vector3.Dot(currentMoveDirection, towardsGroupCenter) + 1) / 2);
                currentMoveDirection = Vector3.Lerp(currentMoveDirection, towardsGroupCenter, affect);
            }

            currentMoveDirection.Normalize();
            Vector2 directionXZ = currentMoveDirection.XZ();

            this.navigationRayCastObjects =
                UnitNavigation.GetRayCastObjects(this, directionXZ, this.settings.AvoidanceRadius);

            if (!this.navigationRayCastObjects.DoRayCast(this, directionXZ, out RayHitResult hitResult))
                return currentMoveDirection;

            List<RayCheck> results = new List<RayCheck> { new RayCheck(directionXZ, hitResult.squaredDist) };

            int i = 0;
            const float angle = 90f / AvoidanceCheckStep;
            while (i < AvoidanceCheckStep)
            {
                i++;
                Vector2 rightDir = MathC.RotateBy(directionXZ, i * angle);

                if (!this.navigationRayCastObjects.DoRayCast(this, rightDir, out hitResult))
                    return rightDir.ToV3(0);
                results.Add(new RayCheck(rightDir, hitResult.squaredDist));

                Vector2 leftDir = MathC.RotateBy(directionXZ, -i * angle);

                if (!this.navigationRayCastObjects.DoRayCast(this, leftDir, out hitResult))
                    return leftDir.ToV3(0);

                results.Add(new RayCheck(leftDir, hitResult.squaredDist));
            }

            return results.OrderBy(r => r.HitDistanceSquared).Last().RayDirection.ToV3(0);
        }

        private bool PositionInCurrentTriangle(Vector3 point)
        {
            return UnitNavigation.AgentWithinTriangle(this, point);
        }

        private void OnDirectionUpdate(Vector3 direction)
        {
        }

#if UNITY_EDITOR

        [FoldoutGroup("Editor")]
        [Button]
        private void SavePath()
        {
            if (this.currentPath.Empty)
                return;

            string path = EditorUtility.SaveFilePanel("Save Path", "Assets/ScriptableObjects/Editor/Test Paths", "path",
                "asset");

            path = "Assets" + path.Split("Assets", 2)[1];

            if (path.Length == 0) return;

            try
            {
                SavedUnitPath unitPath = AssetDatabase.LoadAssetAtPath<SavedUnitPath>(path);
                unitPath.SetValues(this.currentPath, this.transform.position, this.currentPath.Destination());
                EditorUtility.SetDirty(unitPath);
            }
            catch
            {
                SavedUnitPath unitPath = ScriptableObject.CreateInstance<SavedUnitPath>();
                unitPath.SetValues(this.currentPath, this.transform.position, this.currentPath.Destination());
                EditorUtility.SetDirty(unitPath);
                AssetDatabase.CreateAsset(unitPath, path);
            }

            AssetDatabase.SaveAssets();
        }
#endif

        #endregion

        #region Test

#if UNITY_INCLUDE_TESTS
        public void SetSettings(UnitAgentSettings set)
        {
            this.settings = set;
        }
#endif

        #endregion
    }

    internal readonly struct RayCheck
    {
        #region Values

        public readonly Vector2 RayDirection;
        public readonly float HitDistanceSquared;

        #endregion

        #region Build In States

        public RayCheck(Vector2 rayDirection, float hitDistanceSquared)
        {
            this.RayDirection = rayDirection;
            this.HitDistanceSquared = hitDistanceSquared;
        }

        #endregion
    }
}