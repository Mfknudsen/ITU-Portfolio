#region Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.AI.Navigation.Jobs.Pathing;
using Runtime.AI.Navigation.RayCast;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

#endregion

namespace Runtime.AI.Navigation
{
    public static class UnitNavigation
    {
        #region Values

        /// <summary>
        ///     The current main navigation mesh
        /// </summary>
        private static NavigationMesh navMesh;

        /// <summary>
        ///     For when navigation mesh changes then all agents will try to recalculate their path to use the new mesh
        /// </summary>
        private static UnityAction onNavMeshChanged;

        /// <summary>
        ///     All currently active agents
        /// </summary>
        private static List<NavigationAgent> allUnitAgents;

        private static List<UnitWalkGroup> allWalkGroups;

        private static NavMeshCell[,] navMeshCells;

        private static List<QueuedAgentRequest> requests;

        private static NativeArray<float3> _verts;
        private static NativeArray<float2> _simpleVerts;
        private static NativeArray<int> _areas;
        private static NativeArray<JobTriangle> _triangles;

        private static bool _switchNativeArrays;

        private static JobHandle currentJob;

        private static LayerMask unitAgentLayer;

        private static readonly Vector2Int[] AroundIndexOffsets =
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(1, 0), new Vector2Int(1, 0),
            new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
        };

        private const float GroupAngle = 45, SquaredMaxDistanceBetweenGroups = 35;

        #endregion

        #region Getters

        /// <summary>
        ///     Navigation should only be used when there is a navigation mesh to use
        /// </summary>
        public static bool Ready => navMesh != null;

        public static Vector3[] GetVerts()
        {
            return navMesh.Vertices();
        }

        public static LayerMask GetUnitAgentLayerMask()
        {
            return unitAgentLayer;
        }

        public static UnitWalkGroup GetGroupByIndex(int index)
        {
            if (index >= allWalkGroups.Count)
                return allWalkGroups[0];

            return allWalkGroups[index];
        }

        #endregion

        #region Setters

        public static void SetNavMesh(NavigationMesh set)
        {
            if (set == null || navMesh == set)
                return;


            navMesh = set;

            ResetGrouping();

            onNavMeshChanged?.Invoke();

            _switchNativeArrays = true;

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(NavigationMeshSingletonComponent));

            Entity singletonEntity;

            if (!query.TryGetSingleton(out NavigationMeshSingletonComponent component))
            {
                singletonEntity = entityManager.CreateSingleton<NavigationMeshSingletonComponent>(
                    "Navigation Mesh Singleton Holder");

                component = entityManager.CreateEntityQuery(typeof(NavigationMeshSingletonComponent))
                    .GetSingleton<NavigationMeshSingletonComponent>();
                entityManager.AddBuffer<VertBufferElement>(singletonEntity);
                entityManager.AddBuffer<NavTriangleBufferElement>(singletonEntity);
                entityManager.AddBuffer<AreaBufferElement>(singletonEntity);

                entityManager.AddBuffer<TriangleFlattenIndexBufferElement>(singletonEntity);
                entityManager.AddBuffer<TriangleFlattenBufferElement>(singletonEntity);

                entityManager.AddBuffer<VertInTrianglesFlattenIndexBufferElement>(singletonEntity);
                entityManager.AddBuffer<VertInTrianglesFlattenBufferElement>(singletonEntity);

                entityManager.AddBuffer<VertWasUpdatedBufferElement>(singletonEntity);
                entityManager.AddBuffer<TriangleWasUpdatedBufferElement>(singletonEntity);

                entityManager.AddBuffer<AgentPathCollisionBufferElement>(singletonEntity);
                entityManager.AddBuffer<EdgeCollisionBufferElement>(singletonEntity);
            }
            else
                singletonEntity = query.GetSingletonEntity();

            DynamicBuffer<VertBufferElement> verts =
                entityManager.GetBuffer<VertBufferElement>(singletonEntity);
            for (int i = 0; i < set.SimpleVertices.Length; i++)
            {
                verts.Add(new VertBufferElement
                {
                    Position = new float3(set.SimpleVertices[i].x, set.GetVertY[i], set.SimpleVertices[i].y),
                    WasUpdated = true
                });
            }

            DynamicBuffer<NavTriangleBufferElement> triangles =
                entityManager.GetBuffer<NavTriangleBufferElement>(singletonEntity);
            for (int index = 0; index < set.Triangles.Length; index++)
            {
                NavTriangle navTriangle = set.Triangles[index];
                int neighborOne = navTriangle.Neighbors.Count > 0 ? navTriangle.Neighbors[0] : -1,
                    neighborTwo = navTriangle.Neighbors.Count > 1 ? navTriangle.Neighbors[1] : -1,
                    neighborThree = navTriangle.Neighbors.Count > 2 ? navTriangle.Neighbors[2] : -1;

                int[] sharedOne = Shared(index, neighborOne, set.Triangles),
                    sharedTwo = Shared(index, neighborTwo, set.Triangles),
                    sharedThree = Shared(index, neighborThree, set.Triangles);


                triangles.Add(
                    new NavTriangleBufferElement(
                        index,
                        navTriangle.GetA,
                        navTriangle.GetB,
                        navTriangle.GetC,
                        0,
                        navTriangle.GetAB,
                        navTriangle.GetBC,
                        navTriangle.GetAC,
                        navTriangle.MaxY,
                        verts[sharedOne[0]].Distance2D(verts[sharedOne[1]]),
                        neighborTwo != -1 ? verts[sharedTwo[0]].Distance2D(verts[sharedTwo[1]]) : 0,
                        neighborThree != -1 ? verts[sharedThree[0]].Distance2D(verts[sharedThree[1]]) : 0,
                        navTriangle.GetEdgeAB,
                        navTriangle.GetEdgeBC,
                        navTriangle.GetEdgeAC,
                        verts,
                        neighborOne, neighborTwo, neighborThree,
                        sharedOne,
                        sharedTwo,
                        sharedThree));
            }

            DynamicBuffer<AreaBufferElement> areaType =
                entityManager.GetBuffer<AreaBufferElement>(singletonEntity);
            foreach (int area in set.Areas)
                areaType.Add(new AreaBufferElement { AreaType = area });

            component.GroupDivision = set.GetGroupDivisionSize();

            component.MinFloorX = set.GetMinX();
            component.MinFloorZ = set.GetMinZ();
            component.MaxFloorX = set.GetMaxX();
            component.MaxFloorZ = set.GetMaxZ();

            component.CellXLength = navMeshCells.GetLength(0);
            component.CellZLength = navMeshCells.GetLength(1);

            entityManager.SetComponentData(singletonEntity, component);

            EntityQuery cellQuery = entityManager.CreateEntityQuery(typeof(NavMeshCellComponent));
            entityManager.DestroyEntity(cellQuery);
            for (int x = 0; x < component.CellXLength; x++)
            {
                for (int z = 0; z < component.CellZLength; z++)
                {
                    Entity cellEntity = entityManager.CreateEntity();
                    entityManager.SetName(cellEntity, $"Cell: {x} | {z}");
                    entityManager.AddComponent<NavMeshCellComponent>(cellEntity);
                    NavMeshCellComponent navMeshCellComponent =
                        entityManager.GetComponentData<NavMeshCellComponent>(cellEntity);
                    navMeshCellComponent.X = x;
                    navMeshCellComponent.Z = z;
                    entityManager.SetComponentData(cellEntity, navMeshCellComponent);

                    entityManager.AddBuffer<NavMeshCellVertIndexBufferElement>(cellEntity);
                    entityManager.AddBuffer<CellAgentCollisionIndexBufferElement>(cellEntity);
                    entityManager.AddBuffer<CellEdgeCollisionBufferElement>(cellEntity);

                    DynamicBuffer<NavMeshCellTriangleIndexBufferElement> navMeshCellTriangleIndexBufferElement =
                        entityManager.AddBuffer<NavMeshCellTriangleIndexBufferElement>(cellEntity);

                    foreach (int triangleID in navMeshCells[x, z].GetTriangleIDs())
                        navMeshCellTriangleIndexBufferElement.Add(new NavMeshCellTriangleIndexBufferElement
                            { Index = triangleID });
                }
            }
        }

        #endregion

        #region In

        internal static Entity AddAgent(NavigationAgent agent)
        {
            allUnitAgents.Add(agent);

            agent.SetID(allUnitAgents.Count - 1);

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = entityManager.CreateEntity();
            entityManager.SetName(entity, agent.gameObject.name);

            entityManager.AddComponent<UnitAgentComponent>(entity);
            UnitAgentComponent agentComponent = entityManager.GetComponentData<UnitAgentComponent>(entity);
            agentComponent.ID = agent.GetID();
            agentComponent.CurrentTriangleID = -1;
            agentComponent.Position = agent.transform.position;
            agentComponent.Rotation = agent.transform.rotation;
            entityManager.SetComponentData(entity, agentComponent);

            entityManager.AddComponent<DestinationComponent>(entity);
            entityManager.SetComponentEnabled<DestinationComponent>(entity, false);

            entityManager.AddComponent<AgentSettingsComponent>(entity);
            AgentSettingsComponent agentSettingsComponent =
                entityManager.GetComponentData<AgentSettingsComponent>(entity);
            agentSettingsComponent.Radius = agent.Settings.Radius;
            agentSettingsComponent.Height = agent.Settings.Height;
            agentSettingsComponent.ID = agent.Settings.ID;
            agentSettingsComponent.MoveSpeed = agent.Settings.MoveSpeed;
            entityManager.SetComponentData(entity, agentSettingsComponent);

            entityManager.AddBuffer<WayPointBufferElement>(entity);
            entityManager.AddBuffer<AgentTrianglePathBufferElement>(entity);

            return entity;
        }

        internal static void RemoveAgent(NavigationAgent agent, Entity? entity)
        {
            if (agent == null || !allUnitAgents.Contains(agent))
                return;

            allUnitAgents.Remove(agent);

            RemoveUnitFromCell(agent);

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

            if (!entity.HasValue) return;

            for (int i = agent.GetID(); i < allUnitAgents.Count; i++)
            {
                NavigationAgent t = allUnitAgents[i];
                t.SetID(t.GetID() - 1);
                allUnitAgents[i] = t;
            }

            entityManager.DestroyEntity(entity.Value);
        }

        public static void AddOnNavMeshChange(UnityAction action)
        {
            onNavMeshChanged += action;
        }

        public static void RemoveOnNavMeshChange(UnityAction action)
        {
            onNavMeshChanged -= action;
        }

        public static void QueueForPath(NavigationAgent agent, Vector3 destination)
        {
            requests.Add(new QueuedAgentRequest(destination, agent));
        }

        public static int PlaceAgentOnNavMesh(NavigationAgent agent)
        {
            Vector3 agentPosition = agent.transform.position;
            Vector2 agentPosition2D = agentPosition.XZ();

            List<int> triangleIds = GetTriangleIdsByPosition(agentPosition);

            if (triangleIds.Count == 0)
                triangleIds = GetTriangleIdsByPositionSpiralOutwards(agentPosition, 1, 2, 1);

            foreach (int i in triangleIds)
            {
                NavTriangle t = navMesh.Triangles[i];

                if (!MathC.PointWithinTriangle2D(agentPosition2D,
                        navMesh.SimpleVertices[t.GetA],
                        navMesh.SimpleVertices[t.GetB],
                        navMesh.SimpleVertices[t.GetC],
                        out float w1,
                        out float w2))
                    continue;

                Vector3 vectorA = navMesh.VertByIndex(t.GetA),
                    vectorB = navMesh.VertByIndex(t.GetB),
                    vectorC = navMesh.VertByIndex(t.GetC);

                agent.Place(vectorA + (vectorB - vectorA) * w1 + (vectorC - vectorA) * w2);

                return i;
            }

            foreach (int triangleId in triangleIds)
            {
                NavTriangle t = navMesh.Triangles[triangleId];
                MathC.PointWithinTriangle2D(agentPosition2D,
                    navMesh.SimpleVertices[t.GetA],
                    navMesh.SimpleVertices[t.GetB],
                    navMesh.SimpleVertices[t.GetC]);
            }

            int result = triangleIds[0];

            int firstVertexIndex = navMesh.Triangles[triangleIds[0]].GetA;

            float firstDist = agentPosition.QuickSquareDistance(navMesh.VertByIndex(firstVertexIndex));

            foreach (int id in triangleIds)
            foreach (int vertex in navMesh.Triangles[id].Vertices)
            {
                if (firstVertexIndex == vertex)
                    continue;

                float dist = agentPosition.QuickSquareDistance(navMesh.VertByIndex(vertex));

                if (dist > firstDist) continue;

                firstDist = dist;
                firstVertexIndex = vertex;
                result = vertex;
            }

            int secondVertexIndex = navMesh.Triangles[result].GetA;
            if (firstVertexIndex == secondVertexIndex)
                secondVertexIndex = navMesh.Triangles[result].GetB;

            foreach (int vertex in navMesh.Triangles[result].Vertices)
            {
                if (vertex == firstVertexIndex || vertex == secondVertexIndex)
                    continue;

                if (agentPosition.QuickSquareDistance(navMesh.VertByIndex(vertex)) >
                    agentPosition.QuickSquareDistance(navMesh.VertByIndex(secondVertexIndex)))
                    continue;

                secondVertexIndex = vertex;
                break;
            }

            //Place
            Vector3 center = navMesh.Triangles[result].Center(navMesh);

            Vector3 a = navMesh.VertByIndex(firstVertexIndex),
                b = navMesh.VertByIndex(secondVertexIndex);

            a += (center - a).normalized * agent.Settings.Radius;
            b += (center - b).normalized * agent.Settings.Radius;

            agent.Place(MathC.ClosetPointOnLine(agentPosition, a, b));

            return result;
        }

        #endregion

        #region Out

        public static int ClosestTriangleIndex(Vector3 p)
        {
            return navMesh.ClosestTriangleIndex(p);
        }

        public static NavTriangle GetTriangleByID(int id)
        {
            return navMesh.Triangles[id];
        }

        public static Vector3 Get3DVertByIndex(int id)
        {
            return navMesh.Vertices()[id];
        }

        public static Vector2 Get2DVertByIndex(int id)
        {
            return navMesh.SimpleVertices[id];
        }

        public static Vector3[] Get3DVertByIndex(params int[] id)
        {
            return id.Select(i => navMesh.Vertices()[i]).ToArray();
        }

        public static Vector2[] Get2DVertByIndex(params int[] id)
        {
            return id.Select(i => navMesh.SimpleVertices[i]).ToArray();
        }

        /// <summary>
        ///     Get a list of agents 3x3 radius based on the grouped 2D list of agents
        /// </summary>
        /// <param name="agent">Will get agents around this agent while also not including it in the result</param>
        /// <returns>Agents around input agent</returns>
        public static List<NavigationAgent> GetAgentsByAgentPosition(NavigationAgent agent)
        {
            Vector2Int id = GroupingIDByPosition(agent.transform.position);

            List<NavigationAgent> result = new List<NavigationAgent>();

            for (int x = -1; x <= 1; x++)
            {
                if (id.x + x < 0 ||
                    id.x + x >= navMeshCells.GetLength(0))
                    continue;

                for (int y = -1; y <= 1; y++)
                {
                    if (id.y + y < 0 ||
                        id.y + y >= navMeshCells.GetLength(1))
                        continue;

                    foreach (int unitID in navMeshCells[id.x + x, id.y + y].GetAgentIDs())
                    {
                        if (Mathf.Abs(allUnitAgents[unitID].transform.position.y - agent.transform.position.y) > 50)
                            continue;

                        result.Add(allUnitAgents[unitID]);
                    }
                }
            }

            result.Remove(agent);

            return result;
        }

        private static List<int> GetTriangleIdsByPosition(Vector3 pos)
        {
            Vector2Int id = GroupingIDByPosition(pos);

            List<int> result = new List<int>();

            for (int x = -1; x <= 1; x++)
            {
                if (id.x + x < 0 || id.x + x >= navMeshCells.GetLength(0))
                    continue;

                for (int y = -1; y <= 1; y++)
                {
                    if (id.y + y < 0 || id.y + y >= navMeshCells.GetLength(1))
                        continue;

                    foreach (int i in navMeshCells[id.x + x, id.y + y].GetTriangleIDs())
                        if (!result.Contains(i))
                            result.Add(i);
                }
            }

            return result;
        }

        private static List<int> GetTriangleIdsByPositionSpiralOutwards(Vector3 pos, int min, int max,
            int increaseForSpiral)
        {
            if (max <= min) throw new Exception("Max must be greater then min");

            if (increaseForSpiral < 0) throw new Exception("Increase must be zero or greater");

            List<int> result = new List<int>();
            while (result.Count == 0 &&
                   (max - (max - min) < navMeshCells.GetLength(0) ||
                    max - (max - min) < navMeshCells.GetLength(1)))
            {
                Vector2Int id = GroupingIDByPosition(pos);

                for (int x = -max; x <= max; x++)
                {
                    if (Mathf.Abs(x) <= min) continue;

                    if (id.x + x < 0 || id.x + x >= navMeshCells.GetLength(0)) continue;

                    for (int y = -max; y <= max; y++)
                    {
                        if (id.y + y < 0 || id.y + y >= navMeshCells.GetLength(1)) continue;

                        foreach (int i in navMeshCells[id.x + x, id.y + y].GetTriangleIDs())
                            if (!result.Contains(i))
                                result.Add(i);
                    }
                }

                if (increaseForSpiral == 0)
                    break;

                min += increaseForSpiral;
                max += increaseForSpiral;
            }

            return result;
        }

        public static bool AgentWithinTriangle(NavigationAgent agent)
        {
            return false;
        }

        public static bool AgentWithinTriangle(NavigationAgent agent, Vector3 point)
        {
            return false;
        }

        public static List<NavigationRayCastObject> GetRayCastObjects(NavigationAgent currentAgent, Vector2 direction,
            float radius)
        {
            List<NavigationRayCastObject> result = new List<NavigationRayCastObject>();

            const float angle = 70f;
            Vector2 currentAgentPosition = currentAgent.transform.position.XZ();
            Vector2Int groupingID = GroupingIDByPosition(currentAgent.transform.position);
            List<int> agentIDs = new List<int>();
            List<int> triangleIDs = new List<int>();

            foreach (int agentID in navMeshCells[groupingID.x, groupingID.y].GetAgentIDs())
            {
                if (agentID == currentAgent.GetID())
                    continue;

                agentIDs.Add(agentID);
            }

            triangleIDs.AddRange(navMeshCells[groupingID.x, groupingID.y].GetTriangleIDs());

            foreach (Vector2Int offset in AroundIndexOffsets)
            {
                if (!navMeshCells.ValidIndex(groupingID.x + offset.x, groupingID.y + offset.y))
                    continue;

                foreach (int agentID in navMeshCells[groupingID.x + offset.x, groupingID.y + offset.y].GetAgentIDs())
                {
                    if (agentIDs.Contains(agentID) || agentID == currentAgent.GetID())
                        continue;

                    agentIDs.Add(agentID);
                }

                foreach (int triangleID in navMeshCells[groupingID.x + offset.x, groupingID.y + offset.y]
                             .GetTriangleIDs())
                    if (!triangleIDs.Contains(triangleID))
                        triangleIDs.Add(triangleID);
            }

            foreach (int agentID in agentIDs)
            {
                NavigationAgent otherAgent = allUnitAgents[agentID];
                Vector2 otherAgentPosition = otherAgent.transform.position.XZ();

                if (Vector2.Angle(direction, otherAgentPosition - currentAgentPosition) >
                    angle)
                    continue;

                if ((otherAgentPosition - currentAgentPosition).sqrMagnitude <
                    Mathf.Pow(radius + allUnitAgents[agentID].Settings.Radius, 2f))
                {
                    result.Add(new CircleObject(
                        otherAgentPosition + otherAgent.GetCurrentNavMeshDirection() * otherAgent.Settings.MoveSpeed *
                        Time.deltaTime,
                        otherAgent.Settings.Radius));
                }
            }

            foreach (int triangleID in triangleIDs)
            {
                NavTriangle triangle = navMesh.Triangles[triangleID];

                bool ab = triangle.GetEdgeAB,
                    bc = triangle.GetEdgeBC,
                    ac = triangle.GetEdgeAC;

                if (ab &&
                    (Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetA] - currentAgentPosition) < angle ||
                     Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetB] - currentAgentPosition) < angle))
                {
                    result.Add(new LineObject(
                        navMesh.SimpleVertices[triangle.GetA],
                        navMesh.SimpleVertices[triangle.GetB],
                        triangle.Center(navMesh).XZ() - Vector2.Lerp(
                            navMesh.SimpleVertices[triangle.GetA],
                            navMesh.SimpleVertices[triangle.GetB], .5f).normalized));
                }

                if (bc &&
                    (Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetC] - currentAgentPosition) < angle ||
                     Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetB] - currentAgentPosition) < angle))
                {
                    result.Add(new LineObject(
                        navMesh.SimpleVertices[triangle.GetC],
                        navMesh.SimpleVertices[triangle.GetB],
                        triangle.Center(navMesh).XZ() - Vector2.Lerp(
                            navMesh.SimpleVertices[triangle.GetC],
                            navMesh.SimpleVertices[triangle.GetB], .5f).normalized));
                }

                if (ac &&
                    (Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetA] - currentAgentPosition) < angle ||
                     Vector2.Angle(direction, navMesh.SimpleVertices[triangle.GetC] - currentAgentPosition) < angle))
                {
                    result.Add(new LineObject(
                        navMesh.SimpleVertices[triangle.GetA],
                        navMesh.SimpleVertices[triangle.GetC],
                        triangle.Center(navMesh).XZ() - Vector2.Lerp(
                            navMesh.SimpleVertices[triangle.GetA],
                            navMesh.SimpleVertices[triangle.GetC], .5f).normalized));
                }
            }

            return result;
        }

        #endregion

        #region Internal

        /// <summary>
        ///     Add the update function to the game loop and setup the request list.
        ///     In editor, it will need to add the on exit play mode function to stop the update from happening multiple times
        ///     during play mode and while play mode is not active.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(PreUpdate))
                    playerLoop.subSystemList[i].updateDelegate += UpdateAgentPositionsInEcs;

                if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
                    playerLoop.subSystemList[i].updateDelegate += UpdateAgentPositionsFromEcs;

                if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
                    playerLoop.subSystemList[i].updateDelegate += UpdateAgents;

                if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
                    playerLoop.subSystemList[i].updateDelegate += UpdateNavigationValues;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);

            requests = new List<QueuedAgentRequest>();
            allUnitAgents = new List<NavigationAgent>();
            allWalkGroups = new List<UnitWalkGroup>();

            unitAgentLayer = LayerMask.NameToLayer("AI");

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Clean up on exiting play mode.
        /// </summary>
        /// <param name="state">State giving by Unity</param>
        private static void OnExitPlayMode(PlayModeStateChange state)
        {
            if (!state.Equals(PlayModeStateChange.ExitingPlayMode))
                return;

            ResetState();
        }

        private static void ResetState()
        {
            foreach (NavigationAgent agent in allUnitAgents)
                agent.SetGroupID(-1);
            allUnitAgents.Clear();
            allWalkGroups.Clear();

            if (!currentJob.IsCompleted)
                currentJob.Complete();
            DisposeNatives();
            navMesh = null;

            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(PreUpdate))
                    playerLoop.subSystemList[i].updateDelegate -= UpdateAgentPositionsInEcs;

                if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
                    playerLoop.subSystemList[i].updateDelegate -= UpdateAgentPositionsFromEcs;

                if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
                    playerLoop.subSystemList[i].updateDelegate -= UpdateAgents;

                if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
                    playerLoop.subSystemList[i].updateDelegate -= UpdateNavigationValues;
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }
#endif

        private static void UpdateAgentPositionsFromEcs()
        {
            Profiler.BeginSample("UpdateAgentFromECS");

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (NavigationAgent allUnitAgent in allUnitAgents)
                allUnitAgent.UpdatePositionFromEcs(entityManager);

            Profiler.EndSample();
        }

        [BurstCompile]
        private static void UpdateAgentPositionsInEcs()
        {
            Profiler.BeginSample("UpdateECSFromAgent");

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (NavigationAgent allUnitAgent in allUnitAgents)
                allUnitAgent.UpdatePositionInEcs(entityManager);
            Profiler.EndSample();
        }

        /// <summary>
        ///     Update all currently active agents.
        ///     Using one update call instead of each agent having their own individual call reduces time spent by Unity.
        /// </summary>
        private static void UpdateAgents()
        {
            return;

            if (navMesh == null)
                return;

            foreach (NavigationAgent unitAgent in allUnitAgents)
                unitAgent.UpdateInTriangle();

            foreach (NavigationAgent unitAgent in allUnitAgents)
                unitAgent.UpdateIntendedDirection();

            UpdateAgentWalkGroups();

            foreach (NavigationAgent unitAgent in allUnitAgents)
                unitAgent.UpdateAgent();
        }

        /// <summary>
        ///     Update the navigation loop
        /// </summary>
        private static void UpdateNavigationValues()
        {
            UpdateCuts();
            UpdateNativeArrays();
            CalculatePaths();
        }

        /// <summary>
        ///     Update the navigation mesh dynamically based on active cut objects in the scene.
        /// </summary>
        private static void UpdateCuts()
        {
        }

        /// <summary>
        ///     When a new navmesh is set ass current its values will be added to native array for use during the pathing jobs
        /// </summary>
        private static void UpdateNativeArrays()
        {
            if (!_switchNativeArrays)
                return;

            DisposeNatives();

            if (navMesh == null)
                return;

            Vector3[] navVerts = navMesh.Vertices();
            _verts = new NativeArray<float3>(navVerts.Length, Allocator.Persistent);
            for (int i = 0; i < navVerts.Length; i++)
                _verts[i] = navVerts[i];

            Vector2[] navSimpleVerts = navMesh.SimpleVertices;
            _simpleVerts = new NativeArray<float2>(navSimpleVerts.Length, Allocator.Persistent);
            for (int i = 0; i < navSimpleVerts.Length; i++)
                _simpleVerts[i] = navSimpleVerts[i];

            int[] navAreas = navMesh.Areas;
            _areas = new NativeArray<int>(navAreas, Allocator.Persistent);

            NavTriangle[] navTriangles = navMesh.Triangles.ToArray();
            _triangles = new NativeArray<JobTriangle>(navTriangles.Length, Allocator.Persistent);
            for (int i = 0; i < navTriangles.Length; i++)
                _triangles[i] = new JobTriangle(navTriangles[i]);

            _switchNativeArrays = false;
        }

        private static void ResetGrouping()
        {
            int lengthX =
                    Mathf.FloorToInt((navMesh.GetMaxX() - navMesh.GetMinX()) / navMesh.GetGroupDivisionSize()),
                lengthY =
                    Mathf.FloorToInt((navMesh.GetMaxZ() - navMesh.GetMinZ()) / navMesh.GetGroupDivisionSize());

            navMeshCells = new NavMeshCell[lengthX, lengthY];

            Vector2 startPoint = new Vector2(navMesh.GetMinX(), navMesh.GetMinZ());

            for (int x = 0; x < navMeshCells.GetLength(0); x++)
            for (int y = 0; y < navMeshCells.GetLength(1); y++)
            {
                List<int> triangleIDs = new List<int>();

                foreach (NavTriangle t in navMesh.Triangles)
                    if (MathC.PointWithinTriangle2D(
                            startPoint + new Vector2(
                                x * navMesh.GetGroupDivisionSize(),
                                y * navMesh.GetGroupDivisionSize()),
                            navMesh.SimpleVertices[t.GetA],
                            navMesh.SimpleVertices[t.GetB],
                            navMesh.SimpleVertices[t.GetC])
                        ||
                        MathC.PointWithinTriangle2D(
                            startPoint + new Vector2(
                                (x + 1) * navMesh.GetGroupDivisionSize(),
                                y * navMesh.GetGroupDivisionSize()),
                            navMesh.SimpleVertices[t.GetA],
                            navMesh.SimpleVertices[t.GetB],
                            navMesh.SimpleVertices[t.GetC])
                        ||
                        MathC.PointWithinTriangle2D(
                            startPoint + new Vector2(
                                x * navMesh.GetGroupDivisionSize(),
                                (y + 1) * navMesh.GetGroupDivisionSize()),
                            navMesh.SimpleVertices[t.GetA],
                            navMesh.SimpleVertices[t.GetB],
                            navMesh.SimpleVertices[t.GetC])
                        ||
                        MathC.PointWithinTriangle2D(
                            startPoint + new Vector2(
                                (x + 1) * navMesh.GetGroupDivisionSize(),
                                (y + 1) * navMesh.GetGroupDivisionSize()),
                            navMesh.SimpleVertices[t.GetA],
                            navMesh.SimpleVertices[t.GetB],
                            navMesh.SimpleVertices[t.GetC]))
                    {
                        triangleIDs.Add(t.ID);
                    }
                    else
                    {
                        float xMin = startPoint.x + x * navMesh.GetGroupDivisionSize(),
                            xMax = xMin + (x + 1) * navMesh.GetGroupDivisionSize(),
                            zMin = startPoint.y + y * navMesh.GetGroupDivisionSize(),
                            zMax = zMin + (y + 1) * navMesh.GetGroupDivisionSize();

                        foreach (int tVertex in t.Vertices)
                        {
                            if (navMesh.SimpleVertices[tVertex].x < xMin ||
                                navMesh.SimpleVertices[tVertex].x > xMax ||
                                navMesh.SimpleVertices[tVertex].y < zMin ||
                                navMesh.SimpleVertices[tVertex].y > zMax)
                                continue;

                            triangleIDs.Add(t.ID);
                            break;
                        }
                    }

                navMeshCells[x, y] = new NavMeshCell(triangleIDs);
            }

            foreach (NavigationAgent agent in allUnitAgents)
                AddUnitToCell(agent);
        }

        /// <summary>
        ///     Calculate paths using jobs
        /// </summary>
        private static void CalculatePaths()
        {
            if (requests.Count == 0)
                return;
            NativeArray<JobAgent> agents = new NativeArray<JobAgent>(requests.Count, Allocator.TempJob);
            NativeArray<JobPath> paths = new NativeArray<JobPath>(requests.Count, Allocator.TempJob);

            for (int i = 0; i < requests.Count; i++)
                agents[i] = new JobAgent(requests[i]);

            AStartCalculationJob job =
                new AStartCalculationJob(paths, agents, _simpleVerts, _areas, _triangles);

            currentJob = job.Schedule(requests.Count, 100);
            currentJob.Complete();

            for (int i = 0; i < requests.Count; i++)
            {
                requests[i].Agent.SetPath(ToUnitPath(agents[i], paths[i], requests[i].Agent));
                paths[i].Dispose();
            }

            if (agents.IsCreated)
                agents.Dispose();
            if (paths.IsCreated)
                paths.Dispose();

            requests.Clear();
        }

        /// <summary>
        ///     Dispose native arrays to insure no memory leaks
        /// </summary>
        private static void DisposeNatives()
        {
            if (_verts.IsCreated)
                _verts.Dispose();

            if (_simpleVerts.IsCreated)
                _simpleVerts.Dispose();

            if (_areas.IsCreated)
                _areas.Dispose();

            if (_triangles.IsCreated)
                _triangles.Dispose();
        }

        private static void UpdateAgentWalkGroups()
        {
            for (int i = 0; i < allWalkGroups.Count; i++)
            {
                UnitWalkGroup g = allWalkGroups[i];
                g.Update();
                allWalkGroups[i] = g;
            }

            for (int x = 0; x < navMeshCells.GetLength(0); x++)
            for (int y = 0; y < navMeshCells.GetLength(1); y++)
            {
                NavMeshCell cell = navMeshCells[x, y];
                List<int> agentIDs = cell.GetAgentIDs();

                foreach (Vector2Int offset in AroundIndexOffsets)
                    if (navMeshCells.ValidIndex(x + offset.x, y + offset.y))
                        foreach (int agentID in navMeshCells[x + offset.x, y + offset.y].GetAgentIDs())
                            if (!agentIDs.Contains(agentID))
                                agentIDs.Add(agentID);

                for (int i = 0; i < agentIDs.Count; i++)
                {
                    NavigationAgent current = allUnitAgents[agentIDs[i]];

                    if (current.GetGroupID() != -1)
                    {
                        UnitWalkGroup group = allWalkGroups[current.GetGroupID()];
                        if ((group.GetCenter() - current.transform.position.XZ()).sqrMagnitude >
                            SquaredMaxDistanceBetweenGroups ||
                            current.GetCurrentNavMeshDirection() == Vector2.zero ||
                            Vector2.Angle(current.GetCurrentNavMeshDirection(), group.GetDirection()) >
                            GroupAngle * .5f)
                        {
                            if (group.RemoveAgent(current))
                                RemoveWalkGroup(group);
                        }
                    }

                    if (current.GetCurrentNavMeshDirection() == Vector2.zero)
                        continue;

                    for (int j = i + 1; j < agentIDs.Count; j++)
                    {
                        NavigationAgent other = allUnitAgents[agentIDs[j]];

                        if (other.GetCurrentNavMeshDirection() == Vector2.zero)
                            continue;

                        if (Mathf.Abs(current.transform.position.y - other.transform.position.y) > 4)
                            continue;

                        if (current.GetGroupID() == -1 && other.GetGroupID() == -1)
                        {
                            if ((current.transform.position.XZ() - other.transform.position.XZ()).sqrMagnitude >
                                SquaredMaxDistanceBetweenGroups ||
                                Vector2.Angle(current.GetCurrentNavMeshDirection(),
                                    other.GetCurrentNavMeshDirection()) > GroupAngle)
                                continue;

                            UnitWalkGroup group = new UnitWalkGroup(current, other, allWalkGroups.Count);
                            allWalkGroups.Add(group);
                        }
                        else if (current.GetGroupID() == -1 && other.GetGroupID() != -1)
                        {
                            UnitWalkGroup group = allWalkGroups[other.GetGroupID()];
                            if ((current.transform.position.XZ() - group.GetCenter()).sqrMagnitude >
                                SquaredMaxDistanceBetweenGroups ||
                                Vector2.Angle(current.GetCurrentNavMeshDirection(), group.GetDirection()) >
                                GroupAngle * .5f)
                                continue;

                            group.AddAgent(current);
                            allWalkGroups[other.GetGroupID()] = group;
                        }
                        else if (current.GetGroupID() != -1 && other.GetGroupID() != -1)
                        {
                            if ((other.transform.position.XZ() - allWalkGroups[current.GetGroupID()].GetCenter())
                                .sqrMagnitude < SquaredMaxDistanceBetweenGroups &&
                                Vector2.Angle(other.GetCurrentNavMeshDirection(),
                                    allWalkGroups[current.GetGroupID()].GetDirection()) <
                                Vector2.Angle(other.GetCurrentNavMeshDirection(),
                                    allWalkGroups[other.GetGroupID()].GetDirection()))
                            {
                                UnitWalkGroup o = allWalkGroups[other.GetGroupID()];
                                if (o.RemoveAgent(other))
                                    RemoveWalkGroup(o);

                                UnitWalkGroup c = allWalkGroups[current.GetGroupID()];
                                c.AddAgent(other);
                                allWalkGroups[c.GetID()] = c;
                            }
                            else if ((current.transform.position.XZ() - allWalkGroups[other.GetGroupID()].GetCenter())
                                     .sqrMagnitude < SquaredMaxDistanceBetweenGroups &&
                                     Vector2.Angle(current.GetCurrentNavMeshDirection(),
                                         allWalkGroups[other.GetGroupID()].GetDirection()) <
                                     Vector2.Angle(current.GetCurrentNavMeshDirection(),
                                         allWalkGroups[current.GetGroupID()].GetDirection()))
                            {
                                UnitWalkGroup c = allWalkGroups[current.GetGroupID()];
                                if (c.RemoveAgent(current))
                                    RemoveWalkGroup(c);

                                UnitWalkGroup o = allWalkGroups[other.GetGroupID()];
                                o.AddAgent(other);
                                allWalkGroups[o.GetID()] = o;
                            }
                        }
                        else if (current.GetGroupID() != -1 && other.GetGroupID() == -1)
                        {
                            UnitWalkGroup group = allWalkGroups[current.GetGroupID()];
                            if ((other.transform.position.XZ() - group.GetCenter()).sqrMagnitude >
                                SquaredMaxDistanceBetweenGroups ||
                                Vector2.Angle(other.GetCurrentNavMeshDirection(), group.GetDirection()) >
                                GroupAngle * .5f)
                                continue;

                            group.AddAgent(other);
                            allWalkGroups[current.GetGroupID()] = group;
                        }
                    }
                }
            }
        }

        private static UnitPath ToUnitPath(JobAgent jobAgent, JobPath jobPath, NavigationAgent agent)
        {
            int[] ids = new int[jobPath.nodePath.Length];
            for (int i = 0; i < jobPath.nodePath.Length; i++)
                ids[i] = jobPath.nodePath[jobPath.nodePath.Length - 1 - i];

            return new UnitPath(
                jobAgent.startPosition,
                jobAgent.endPoint,
                ids,
                navMesh.Triangles,
                navMesh.Vertices(),
                navMesh.SimpleVertices,
                agent);
        }

        private static Vector2Int GroupingIDByPosition(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(Mathf.Clamp(
                    (position.x - navMesh.GetMinX()) / navMesh.GetGroupDivisionSize(),
                    0,
                    navMeshCells.GetLength(0) - 1)),
                Mathf.FloorToInt(Mathf.Clamp(
                    (position.z - navMesh.GetMinZ()) / navMesh.GetGroupDivisionSize(),
                    0,
                    navMeshCells.GetLength(1) - 1)));
        }

        private static void AddUnitToCell(NavigationAgent agent)
        {
            Vector2Int id = GroupingIDByPosition(agent.transform.position);
            navMeshCells[id.x, id.y].AddAgentID(agent.GetID());
        }

        private static void RemoveUnitFromCell(NavigationAgent agent)
        {
            Vector2Int id = GroupingIDByPosition(agent.transform.position);
            navMeshCells[id.x, id.y].RemoveAgentID(agent.GetID());
        }

        private static void RemoveWalkGroup(UnitWalkGroup group)
        {
            allWalkGroups.RemoveAt(group.GetID());
            for (int index = 0; index < allWalkGroups.Count; index++)
            {
                UnitWalkGroup g = allWalkGroups[index];
                g.SetID(index);
                allWalkGroups[index] = g;
            }
        }

        private static int[] Shared(int current, int neighbor, NavTriangle[] triangles)
        {
            int[] result = { -1, -1 };

            if (neighbor == -1)
                return result;

            int count = 0;

            NavTriangle a = triangles[current], b = triangles[neighbor];

            foreach (int aVertex in a.Vertices)
            {
                foreach (int bVertex in b.Vertices)
                {
                    if (aVertex != bVertex)
                        continue;

                    result[count] = aVertex;

                    count++;

                    if (count == 2)
                        return result;

                    break;
                }
            }

            return result;
        }

        #endregion

        #region Test

#if UNITY_INCLUDE_TESTS
        public static void ClearForTests()
        {
            ResetState();

            if (Unity.Entities.World.DefaultGameObjectInjectionWorld == null)
                return;

            EntityManager entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery singletonQuery = entityManager.CreateEntityQuery(typeof(NavigationMeshSingletonComponent));
            entityManager.DestroyEntity(singletonQuery.GetSingletonEntity());

            EntityQuery cellQuery = entityManager.CreateEntityQuery(typeof(NavMeshCellComponent));
            foreach (Entity entity in cellQuery.ToEntityArray(Allocator.Temp))
                entityManager.DestroyEntity(entity);
        }

        public static void InitializeForTest()
        {
            Initialize();
        }

        public static IReadOnlyList<NavigationAgent> GetAllAgents()
        {
            return allUnitAgents;
        }
#endif

        #endregion
    }
}