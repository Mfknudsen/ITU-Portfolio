#region Libraries

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Editor.Systems;
using Runtime.AI.Navigation;
using Runtime.Core;
using Runtime.Editor;
using Runtime.World;
using Runtime.World.Overworld;
using Runtime.World.Overworld.TileHierarchy;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.AI.Navigation;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;
using TileController = Runtime.World.Overworld.TileSubController;

#endregion

namespace Editor.World.TileSubController
{
    public sealed class TileSubControllerProcessor : OdinPropertyProcessor<TileController>
    {
        #region Values

        private const float OverlapCheckDistance = .3f;

        private const int Grouping = 5;

        #endregion

        #region Build In States

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            propertyInfos.AddDelegate("Bake Navigation Mesh",
                () => this.BakeNavmesh(this.ValueEntry.Values[0]),
                new FoldoutGroupAttribute("Navigation"));

            propertyInfos.AddDelegate("Bake Lightning",
                () => this.BakeLighting(this.ValueEntry.Values[0]),
                new FoldoutGroupAttribute("Lightning"));

            propertyInfos.AddDelegate("Optimize Tile",
                () => this.OptimizeTile(this.ValueEntry.Values[0]),
                new FoldoutGroupAttribute("Optimize"), new PropertyOrderAttribute(-2));
        }

        #endregion

        #region Internal

        #region Optimizations

        private async void OptimizeTile(TileController tileSubController)
        {
            if (BakedEditorManager.IsBakeRunning || tileSubController == null)
                return;

            BakedEditorManager.SetRunning(true);

            string editorProgressParTitle = "Optimizing tile: " + tileSubController.gameObject.scene.name;
            EditorUtility.DisplayProgressBar(editorProgressParTitle, "Grouping Terrain Trees", 0);

            TileEnvironment tileEnvironment = tileSubController.GetTileEnvironment();
            List<Terrain> terrains = tileEnvironment.GetTerrains();

            if (terrains.Count > 0)
            {
                int totalInstanceCount = 0, currentCount = 0;
                foreach (Terrain terrain in terrains)
                    totalInstanceCount += terrain.terrainData.treeInstances.Length;

                Bounds currentBounds = terrains[0].terrainData.bounds;
                Vector3 currentTerrainPosition = terrains[0].transform.position;
                float highestX = currentTerrainPosition.x + currentBounds.extents.x * .5f,
                    highestY = currentTerrainPosition.y + currentBounds.extents.y * .5f,
                    lowestX = currentTerrainPosition.x - currentBounds.extents.x * .5f,
                    lowestY = currentTerrainPosition.y - currentBounds.extents.y * .5f;

                if (terrains.Count > 1)
                    for (int i = 1; i < terrains.Count; i++)
                    {
                        currentBounds = terrains[i].terrainData.bounds;
                        currentTerrainPosition = terrains[i].transform.position;
                        highestX = currentTerrainPosition.x + currentBounds.extents.x * .5f;
                        highestY = currentTerrainPosition.y + currentBounds.extents.y * .5f;
                        lowestX = currentTerrainPosition.x - currentBounds.extents.x * .5f;
                        lowestY = currentTerrainPosition.y - currentBounds.extents.y * .5f;
                    }

                int x = Mathf.FloorToInt((highestX - lowestX) / Grouping),
                    y = Mathf.FloorToInt((highestY - lowestY) / Grouping);

                List<OptimizedTreeInstance>[,] terrainPropsResult = new List<OptimizedTreeInstance>[x, y];

                foreach (Terrain terrain in terrains)
                {
                    foreach (TreeInstance terrainDataTreeInstance in terrain.terrainData.treeInstances)
                    {
                        Vector3 pos = terrainDataTreeInstance.position;
                        int tX = Mathf.FloorToInt((pos.x - lowestX) / Grouping),
                            tY = Mathf.FloorToInt((pos.y - lowestY) / Grouping);

                        List<OptimizedTreeInstance> list = terrainPropsResult[tX, tY];
                        list ??= new List<OptimizedTreeInstance>();
                        list.Add(new OptimizedTreeInstance(terrainDataTreeInstance));
                        terrainPropsResult[tX, tY] = list;

                        currentCount++;
                        EditorUtility.DisplayProgressBar(editorProgressParTitle, "Grouping Terrain Trees",
                            1f / totalInstanceCount * currentCount);
                    }

                    await UniTask.NextFrame();
                }

                tileSubController.SetOptimizedInformation(new TileOptimizedInformation(Grouping, lowestX, lowestY,
                    highestX,
                    highestY, terrainPropsResult));
            }
        }

        #endregion

        #region Lighting

        /// <summary>
        ///     Bake the lighting using Bakery asset
        /// </summary>
        /// <param name="tileSubController">The current TileSubController</param>
        private async void BakeLighting(TileController tileSubController)
        {
            if (BakedEditorManager.IsBakeRunning)
                return;

            BakedEditorManager.SetRunning(true);

            EditorSceneManager.SaveScene(tileSubController.gameObject.scene);

            string editorProgressParTitle = "Baking Navigation: " + tileSubController.gameObject.scene.name;

            List<string> neighborsToLoad = new List<string>();

            foreach (string path in UnityObject.FindObjectsOfType<ConnectionPoint>().Select(cp => cp.ScenePath)
                         .ToArray())
                if (!neighborsToLoad.Contains(path))
                    neighborsToLoad.Add(path);

            Scene[] loadedScenes = Array.Empty<Scene>();

            try
            {
                #region Load neighbor scenes

                //Load neighboring scenes that the navigation mesh should cover
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors", .25f);
                List<UniTask<Scene>> loadSceneTasks = new List<UniTask<Scene>>();

                for (int i = 0; i < neighborsToLoad.Count; i++)
                {
                    loadSceneTasks.Add(AsyncLoadScene(neighborsToLoad[i]));
                    EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors",
                        .25f + .5f / neighborsToLoad.Count * i);
                }

                loadedScenes = await UniTask.WhenAll(loadSceneTasks);
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors", 1f);

                foreach (GameObject g in GameObject.FindGameObjectsWithTag("EditorOnly"))
                    g.SetActive(false);

                #endregion

                this.SetupBakeryLightmapPrefab();

                #region Directories

                Scene s = tileSubController.gameObject.scene;
                string assetPath = s.path.Replace(".unity", "");

                if (!AssetDatabase.IsValidFolder(assetPath + "/Lighting"))
                    AssetDatabase.CreateFolder(assetPath, "Lighting");

                assetPath += "/Lighting";

                for (int i = 0; i < Enum.GetValues(typeof(WorldTimeZone)).Length; i++)
                    if (!AssetDatabase.IsValidFolder(assetPath + $"/{((WorldTimeZone)i).ToString()}"))
                        AssetDatabase.CreateFolder(assetPath, ((WorldTimeZone)i).ToString());

                #endregion

                bool lightProbeBake = UnityObject.FindObjectsOfType<LightProbeGroup>().Length > 0;
                bool reflectionProbeBake = UnityObject.FindObjectsOfType<ReflectionProbe>().Length > 0;

                //Calculate light for each time
                //for (int i = 0; i < Enum.GetValues(typeof(WorldTimeZone)).Length; i++)
                for (int i = 0; i < 1; i++)
                {
                    //Replace "Assets/" because bakery already assumes the path is in assets.
                    ftRenderLightmap.outputPathFull =
                        (assetPath + $"/{((WorldTimeZone)i).ToString()}").Replace("Assets/", "");

                    SetDayTime(i);

                    await UniTask.NextFrame();

                    #region Bake

                    await BakePrefabLighting(tileSubController);

                    if (ftRenderLightmap.userCanceled)
                        throw new Exception("Canceled");

                    if (lightProbeBake)
                    {
                        await BakeLightProbeGroup(tileSubController);

                        if (ftRenderLightmap.userCanceled)
                            throw new Exception("Canceled");
                    }

                    if (reflectionProbeBake)
                    {
                        await BakeReflectionProbes(tileSubController);

                        if (ftRenderLightmap.userCanceled)
                            throw new Exception("Canceled");
                    }

                    #endregion
                }

                await UniTask.NextFrame();
            }
            catch (Exception e)
            {
                if (!e.Message.Equals("Canceled"))
                {
                    Debug.LogError("Baking Lighting Failed");
                    Debug.LogError(e);
                }
                else
                {
                    Debug.Log("Lighting bake was canceled by user");
                }
            }

            #region UnLoad neighbor scenes

            if (loadedScenes.Length > 0)
            {
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Unloading Neighbors", 0f);
                List<UniTask> unloadTasks = new List<UniTask>();

                foreach (Scene t in loadedScenes)
                    unloadTasks.Add(AsyncUnloadScene(t));

                await UniTask.WhenAll(unloadTasks);

                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Unloading Neighbors", 1f);
            }

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("EditorOnly"))
                g.SetActive(true);

            #endregion

            EditorUtility.ClearProgressBar();

            BakedEditorManager.SetRunning(false);
        }

        private static void SetDayTime(int index)
        {
            WorldTime.SetCurrentDayTime((WorldTimeZone)index);

            foreach (BakeryDirectLight bakeryDirectLight in UnityObject.FindObjectsOfType<BakeryDirectLight>())
            {
                Light light = bakeryDirectLight.GetComponent<Light>();
                bakeryDirectLight.color = light.color;
                bakeryDirectLight.intensity = light.intensity;
                bakeryDirectLight.indirectIntensity = light.bounceIntensity;
            }

            foreach (BakeryPointLight bakeryPointLight in UnityObject.FindObjectsOfType<BakeryPointLight>())
            {
                Light light = bakeryPointLight.GetComponent<Light>();
                bakeryPointLight.color = light.color;
                bakeryPointLight.intensity = light.intensity;
                bakeryPointLight.falloffMinRadius = light.range;
                bakeryPointLight.indirectIntensity = light.bounceIntensity;
            }
        }

        private static ftRenderLightmap GetFtRenderLightmapInstance()
        {
            return ftRenderLightmap.instance != null
                ? ftRenderLightmap.instance
                : ScriptableObject.CreateInstance<ftRenderLightmap>();
        }

        private void SetupBakeryLightmapPrefab()
        {
        }

        private static async UniTask BakePrefabLighting(TileController subController)
        {
            UniTaskCompletionSource<bool> bakeTask = new UniTaskCompletionSource<bool>();

            EditorCoroutineUtility.StartCoroutine(AsyncBakePrefabLighting(bakeTask), subController);

            await bakeTask.Task;
        }

        private static async UniTask BakeLightProbeGroup(TileController subController)
        {
            UniTaskCompletionSource<bool> bakeTask = new UniTaskCompletionSource<bool>();

            EditorCoroutineUtility.StartCoroutine(AsyncBakeLightProbes(bakeTask), subController);

            await bakeTask.Task;
        }

        private static async UniTask BakeReflectionProbes(TileController subController)
        {
            UniTaskCompletionSource<bool> bakeTask = new UniTaskCompletionSource<bool>();

            EditorCoroutineUtility.StartCoroutine(AsyncBakeReflectionProbes(bakeTask), subController);

            await bakeTask.Task;
        }

        private static IEnumerator AsyncBakePrefabLighting(UniTaskCompletionSource<bool> taskCompletionSource)
        {
            GetFtRenderLightmapInstance().RenderButton(false);

            while (ftRenderLightmap.bakeInProgress)
                yield return null;

            taskCompletionSource.TrySetResult(true);
        }

        private static IEnumerator AsyncBakeLightProbes(UniTaskCompletionSource<bool> taskCompletionSource)
        {
            GetFtRenderLightmapInstance().RenderLightProbesButton(false);

            while (ftRenderLightmap.bakeInProgress)
                yield return null;

            taskCompletionSource.TrySetResult(true);
        }

        private static IEnumerator AsyncBakeReflectionProbes(UniTaskCompletionSource<bool> taskCompletionSource)
        {
            GetFtRenderLightmapInstance().RenderReflectionProbesButton(false);

            while (ftRenderLightmap.bakeInProgress)
                yield return null;

            taskCompletionSource.TrySetResult(true);
        }

        #endregion

        #region Custom Navmesh Baking

        /// <summary>
        ///     Bake a custom Navmesh for use with the custom Navmesh agents.
        /// </summary>
        /// <param name="tileSubController">The current TileSubController</param>
        private async UniTask<NavigationMesh> BakeNavmesh(TileController tileSubController)
        {
            if (BakedEditorManager.IsBakeRunning || tileSubController == null)
                return null;

            //Stops other processes from starting at the same time.
            BakedEditorManager.SetRunning(true);

            //Title for the progressbar displayed during the baking process.
            string editorProgressParTitle = "Baking Navigation: " + tileSubController.gameObject.scene.name;

            //Tile neighbors (Other scenes) to load during baking to allow agents to walk from tile to tile.
            List<string> neighborsToLoad = new List<string>();

            //Finding all tile connection points to determine which to load.
            foreach (string path in UnityObject
                         .FindObjectsByType<ConnectionPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                         .Select(cp => cp.ScenePath)
                         .ToArray())
                if (!neighborsToLoad.Contains(path))
                    neighborsToLoad.Add(path);

            //Find and disable all editor only objects.
            GameObject[] editorGameObjects =
                GameObject.FindGameObjectsWithTag("EditorOnly").Where(o => o.activeSelf).ToArray();
            foreach (GameObject editorGameObject in editorGameObjects)
                editorGameObject.SetActive(false);

            //The current loaded neighbor tiles (Scenes).
            Scene[] loadedScenes = Array.Empty<Scene>();
            NavigationMesh calculatedNavMesh = null;
            try
            {
                #region Load neighbor scenes and build navmesh triangulation

                //Load neighboring scenes that the navigation mesh should cover
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors", .25f);
                List<UniTask<Scene>> loadSceneTasks = new List<UniTask<Scene>>();

                for (int i = 0; i < neighborsToLoad.Count; i++)
                {
                    loadSceneTasks.Add(AsyncLoadScene(neighborsToLoad[i]));
                    EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors",
                        .25f + .5f / neighborsToLoad.Count * i);
                }

                loadedScenes = await UniTask.WhenAll(loadSceneTasks);
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Loading Neighbors", 1f);

                //Construct Navigation Mesh and setup custom navmesh logic
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Calculation Navigation Mesh Triangulation",
                    0f);

                //Use Unitys build in method for creating a navigation mesh and store the information. 
                NavMeshTriangulation navmesh =
                    BuildNavMeshTriangulation(tileSubController.GetComponent<NavMeshSurface>());

                #endregion

                #region Check Vertices and Indices for overlap

                //First iteration of the values to be stored.
                List<Vector3> verts = navmesh.vertices.ToList();
                List<int> indices = navmesh.indices.ToList();
                List<int> areas = navmesh.areas.ToList();
                Dictionary<Vector2Int, List<int>> vertsByPos = new Dictionary<Vector2Int, List<int>>();

                //Group vertices by their 2D (x,z) positions for faster checking later on.
                const float groupSize = 5f;
                for (int i = 0; i < verts.Count; i++)
                {
                    Vector3 v = verts[i];
                    Vector2Int id = new Vector2Int(Mathf.FloorToInt(v.x / groupSize),
                        Mathf.FloorToInt(v.z / groupSize));

                    if (vertsByPos.TryGetValue(id, out List<int> outList))
                        outList.Add(i);
                    else
                        vertsByPos.Add(id, new List<int> { i });
                }

                //Check if some vertices is so close to another that one can be removed while the other takes over the removed ones connection.
                this.CheckOverlap(verts, indices, vertsByPos, groupSize, editorProgressParTitle);

                #endregion

                #region Create first iteration of NavTriangles

                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Creating first iteration NavTriangles", 0f);
                List<NavTriangle> triangles = new List<NavTriangle>();
                Dictionary<int, List<int>> trianglesByVertexID = new Dictionary<int, List<int>>();

                SetupNavTriangles(verts, indices, areas, triangles, trianglesByVertexID, editorProgressParTitle);

                triangles = SetupNeighbors(triangles, trianglesByVertexID, editorProgressParTitle, "First");

                #endregion

                #region Check neighbor connections

                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Checking NavTriangle neighbor connections",
                    .5f);
                Vector3 cleanPoint = tileSubController.GetCleanUpPoint;
                int closestVert = 0;
                float closestDistance = cleanPoint.QuickSquareDistance(verts[closestVert]);

                for (int i = 1; i < verts.Count; i++)
                {
                    float d = cleanPoint.QuickSquareDistance(verts[i]);
                    if (d >= closestDistance)
                        continue;

                    if (trianglesByVertexID.TryGetValue(i, out List<int> value) &&
                        !value.Any(t => triangles[t].Neighbors.Count > 0))
                        continue;

                    closestDistance = d;
                    closestVert = i;
                }

                List<int> connected = new List<int>(), toCheck = new List<int>();
                toCheck.AddRange(trianglesByVertexID[closestVert]);

                while (toCheck.Count > 0)
                {
                    int index = toCheck[0];
                    NavTriangle navTriangle = triangles[index];
                    toCheck.RemoveAt(0);
                    connected.Add(index);
                    foreach (int n in navTriangle.Neighbors)
                        if (!toCheck.Contains(n) && !connected.Contains(n))
                            toCheck.Add(n);

                    EditorUtility.DisplayProgressBar(editorProgressParTitle,
                        "Checking NavTriangle neighbor connections", .5f + .5f / triangles.Count * connected.Count);
                }

                #endregion

                #region Fill holes and final iteration of NavTriangles

                List<Vector3> fixedVertices = new List<Vector3>();
                List<int> fixedIndices = new List<int>(), fixedAreas = new List<int>();
                Dictionary<int, List<int>> fixedTrianglesByVertexID = new Dictionary<int, List<int>>();

                foreach (NavTriangle t in connected.Select(i => triangles[i]))
                {
                    int aID = t.Vertices[0], bID = t.Vertices[1], cID = t.Vertices[2];
                    Vector3 a = verts[aID], b = verts[bID], c = verts[cID];

                    if (!fixedVertices.Contains(a))
                        fixedVertices.Add(a);

                    if (!fixedVertices.Contains(b))
                        fixedVertices.Add(b);

                    if (!fixedVertices.Contains(c))
                        fixedVertices.Add(c);

                    fixedIndices.Add(fixedVertices.IndexOf(a));
                    fixedIndices.Add(fixedVertices.IndexOf(b));
                    fixedIndices.Add(fixedVertices.IndexOf(c));

                    fixedAreas.Add(t.Area);

                    Vector2Int id = new Vector2Int(Mathf.FloorToInt(a.x / groupSize),
                        Mathf.FloorToInt(a.z / groupSize));
                    if (vertsByPos.TryGetValue(id, out List<int> outListA))
                    {
                        if (outListA.Contains(fixedVertices.IndexOf(a)))
                            outListA.Add(fixedVertices.IndexOf(a));
                    }
                    else
                    {
                        vertsByPos.Add(id, new List<int> { fixedVertices.IndexOf(a) });
                    }

                    id = new Vector2Int(Mathf.FloorToInt(b.x / groupSize), Mathf.FloorToInt(b.z / groupSize));
                    if (vertsByPos.TryGetValue(id, out List<int> outListB))
                    {
                        if (outListB.Contains(fixedVertices.IndexOf(b)))
                            outListB.Add(fixedVertices.IndexOf(b));
                    }
                    else
                    {
                        vertsByPos.Add(id, new List<int> { fixedVertices.IndexOf(b) });
                    }

                    id = new Vector2Int(Mathf.FloorToInt(c.x / groupSize), Mathf.FloorToInt(c.z / groupSize));
                    if (vertsByPos.TryGetValue(id, out List<int> outListC))
                    {
                        if (outListC.Contains(fixedVertices.IndexOf(c)))
                            outListC.Add(fixedVertices.IndexOf(c));
                    }
                    else
                    {
                        vertsByPos.Add(id, new List<int> { fixedVertices.IndexOf(c) });
                    }
                }

                FillHoles(fixedVertices, fixedAreas, fixedIndices, editorProgressParTitle);

                List<NavTriangle> fixedTriangles = new List<NavTriangle>();

                SetupNavTriangles(fixedVertices, fixedIndices, fixedAreas, fixedTriangles,
                    fixedTrianglesByVertexID, editorProgressParTitle);

                fixedTriangles = SetupNeighbors(fixedTriangles, fixedTrianglesByVertexID, editorProgressParTitle,
                    "Final");

                for (int i = 0; i < fixedTriangles.Count; i++)
                {
                    fixedTriangles[i].SetBorderWidth(fixedVertices, fixedTriangles);
                    EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle, "Setting border width",
                        1f / fixedTriangles.Count * (i + 1));
                }

                Dictionary<int, List<NavigationPointEntry>> fixedEntryPoints =
                    SetupEntryPointsForTriangles(fixedTriangles.ToArray(), fixedTrianglesByVertexID,
                        fixedVertices.ToArray());

                #endregion

                #region Find Edges

                bool[] vertIsEdge = new bool[fixedVertices.Count];
                for (int i = 0; i < fixedVertices.Count; i++)
                {
                    vertIsEdge[i] = false;

                    List<int> triangleIDs = triangles.Where(t => t.Vertices.Contains(i)).Select(t => t.ID).ToList();

                    if (triangleIDs.Count < 3)
                    {
                        vertIsEdge[i] = true;
                        continue;
                    }

                    foreach (int current in triangleIDs)

                    {
                        int neighborCount = 0;

                        foreach (int other in triangleIDs)
                        {
                            if (current == other)
                                continue;

                            if (triangles[current].Vertices.SharedBetweenCount(triangles[other].Vertices) == 2)
                                neighborCount++;

                            if (neighborCount == 2)
                                break;
                        }

                        if (neighborCount == 2)
                            continue;

                        vertIsEdge[i] = true;

                        break;
                    }
                }

                for (int i = 0; i < fixedTriangles.Count; i++)
                {
                    NavTriangle navTriangle = fixedTriangles[i];
                    int a = navTriangle.GetA, b = navTriangle.GetB, c = navTriangle.GetC;

                    bool ab = vertIsEdge[a] && vertIsEdge[b] && !fixedTriangles.Any(t =>
                        t.ID != navTriangle.ID && t.Vertices.Contains(a) && t.Vertices.Contains(b));

                    bool bc = vertIsEdge[c] && vertIsEdge[b] && !fixedTriangles.Any(t =>
                        t.ID != navTriangle.ID && t.Vertices.Contains(c) && t.Vertices.Contains(b));

                    bool ac = vertIsEdge[a] && vertIsEdge[c] && !fixedTriangles.Any(t =>
                        t.ID != navTriangle.ID && t.Vertices.Contains(a) && t.Vertices.Contains(c));

                    navTriangle.SetIsEdge(ab, bc, ac);
                    fixedTriangles[i] = navTriangle;
                }

                #endregion

                #region Create the storage to contain the final result

                Scene s = tileSubController.gameObject.scene;
                string assetFolderPath = s.path.Replace(".unity", "/"),
                    assetName = s.name + " NavMesh Calculations.asset";

                try
                {
                    calculatedNavMesh =
                        AssetDatabase.LoadAssetAtPath<NavigationMesh>(assetFolderPath + assetName);
                    calculatedNavMesh.SetValues(
                        fixedVertices.ToArray(),
                        fixedTriangles.ToArray(),
                        fixedAreas.ToArray(),
                        fixedEntryPoints);
                    EditorUtility.SetDirty(calculatedNavMesh);
                }
                catch
                {
                    calculatedNavMesh = ScriptableObject.CreateInstance<NavigationMesh>();
                    calculatedNavMesh.name = s.name + " NavMesh Calculations";
                    calculatedNavMesh.SetValues(
                        fixedVertices.ToArray(),
                        fixedTriangles.ToArray(),
                        fixedAreas.ToArray(),
                        fixedEntryPoints);
                    tileSubController.SetCalculatedNavMesh(calculatedNavMesh);

                    EditorUtility.SetDirty(calculatedNavMesh);
                    if (!AssetDatabase.IsValidFolder(assetFolderPath))
                    {
                        string[] split = assetFolderPath.Split('/');
                        AssetDatabase.CreateFolder(
                            assetFolderPath.Remove(assetFolderPath.Length - split[^2].Length - 2, split[^2].Length + 2),
                            split[^2]);
                    }

                    AssetDatabase.CreateAsset(calculatedNavMesh, assetFolderPath + assetName);
                }

                AssetDatabase.SaveAssets();

                tileSubController.gameObject.GetFirstComponentByRoot<NavMeshVisualizer>()?.Create();

                #endregion
            }
            catch (Exception e)
            {
                //Displayed progressbar contains a cancel button which when used should throw an exception with the message: "Cancel".
                if (e.Message.Equals("Cancel"))
                {
                    Debug.Log("Baking navmesh was canceled");
                }
                else
                {
                    //If an exception is thrown without the message: "Cancel" then it is an error.
                    Debug.LogError("Baking Navigation Mesh Failed");
                    Debug.LogError(e);
                }
            }

            #region UnLoad neighbor scenes

            //Unload the currently loaded neighbors (Scenes).
            if (loadedScenes.Length > 0)
            {
                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Unloading Neighbors", 0f);
                List<UniTask> unloadTasks = new List<UniTask>();

                foreach (Scene t in loadedScenes)
                    unloadTasks.Add(AsyncUnloadScene(t));

                await UniTask.WhenAll(unloadTasks);

                EditorUtility.DisplayProgressBar(editorProgressParTitle, "Unloading Neighbors", 1f);
            }

            //All editor only objects gets enabled.
            foreach (GameObject g in editorGameObjects)
                g.SetActive(true);

            #endregion

            //The progressbar is no longer needed.
            EditorUtility.ClearProgressBar();

            //Save the current scene
            EditorSceneManager.SaveScene(tileSubController.gameObject.scene);

            //Other process may start now.
            BakedEditorManager.SetRunning(false);

            return calculatedNavMesh;
        }

        /// <summary>
        ///     Loads the scene at the path async
        /// </summary>
        /// <param name="path">File path for the scene</param>
        /// <returns></returns>
        private static async UniTask<Scene> AsyncLoadScene(string path)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            await UniTask.WaitWhile(() => !scene.isLoaded);

            return scene;
        }

        /// <summary>
        ///     Unloads a currently loaded scene async
        /// </summary>
        /// <param name="scene">Name of the loaded scene to unload</param>
        private static async UniTask AsyncUnloadScene(Scene scene)
        {
            EditorSceneManager.CloseScene(scene, true);

            await UniTask.WaitWhile(() => scene.isLoaded);
        }

        /// <summary>
        ///     Builds a navmesh from the TileSubController and returns the navmesh triangulation from the build navmesh.
        ///     Then remove the build navmesh as it is no longer needed.
        /// </summary>
        /// <param name="surface">To build the navmesh from</param>
        /// <returns>Navmesh triangulation containing vertices, indices and areas in arrays</returns>
        private static NavMeshTriangulation BuildNavMeshTriangulation(
            NavMeshSurface surface)
        {
            surface.BuildNavMesh();
            NavMeshTriangulation navmesh = NavMesh.CalculateTriangulation();
            surface.RemoveData();
            surface.navMeshData = null;
            return navmesh;
        }

        /// <summary>
        ///     Creates new NavTriangles from the indices
        /// </summary>
        /// <param name="verts">3D vertices</param>
        /// <param name="indices">Each pair of threes indicate one triangle</param>
        /// <param name="areas">Area values for each triangle</param>
        /// <param name="triangles">List for the triangles to be added to</param>
        /// <param name="trianglesByVertexID">Each triangle will be assigned to each relevant vertex for optimization later</param>
        /// <param name="editorProgressParTitle">Editor loading bar title</param>
        /// <exception cref="Exception">Throws "Cancel" if the user cancels the progress</exception>
        private static void SetupNavTriangles(IReadOnlyList<Vector3> verts, IReadOnlyList<int> indices,
            IReadOnlyList<int> areas,
            ICollection<NavTriangle> triangles, IDictionary<int, List<int>> trianglesByVertexID,
            string editorProgressParTitle)
        {
            for (int i = 0; i < indices.Count; i += 3)
            {
                int a = indices[i], b = indices[i + 1], c = indices[i + 2];
                NavTriangle triangle = new NavTriangle(i / 3, a, b, c, areas[i / 3], verts[a], verts[b], verts[c]);

                triangles.Add(triangle);

                int tID = triangles.Count - 1;

                if (trianglesByVertexID.TryGetValue(a, out List<int> list))
                {
                    if (!list.Contains(tID))
                        list.Add(tID);
                }
                else
                {
                    trianglesByVertexID.Add(a, new List<int>() { tID });
                }

                if (trianglesByVertexID.TryGetValue(b, out list))
                {
                    if (!list.Contains(tID))
                        list.Add(tID);
                }
                else
                {
                    trianglesByVertexID.Add(b, new List<int>() { tID });
                }

                if (trianglesByVertexID.TryGetValue(c, out list))
                {
                    if (!list.Contains(tID))
                        list.Add(tID);
                }
                else
                {
                    trianglesByVertexID.Add(c, new List<int>() { tID });
                }

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Creating NavTriangles: {i / 3f} / {indices.Count / 3f}",
                        1f / (indices.Count / 3f) * (i / 3f)))
                    throw new Exception("Cancel");
            }
        }

        /// <summary>
        ///     Setup the connected neighbors for each NavTriangle
        /// </summary>
        /// <param name="triangles">The triangles to check</param>
        /// <param name="trianglesByVertexID">A list of triangle ids based on a vertex id</param>
        /// <param name="editorProgressParTitle">Editor loading bar title</param>
        /// <param name="iteration">This will run multiple times during baking. Will help the user know which step</param>
        /// <returns>The current triangle list now with neighbors set</returns>
        /// <exception cref="Exception">Throws "Cancel" if the user cancels the progress</exception>
        private static List<NavTriangle> SetupNeighbors(List<NavTriangle> triangles,
            IReadOnlyDictionary<int, List<int>> trianglesByVertexID, string editorProgressParTitle, string iteration)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                List<int> neighbors = new List<int>();
                List<int> possibleNeighbors = new List<int>();
                possibleNeighbors.AddRange(trianglesByVertexID[triangles[i].Vertices[0]]);
                possibleNeighbors.AddRange(trianglesByVertexID[triangles[i].Vertices[1]]);
                possibleNeighbors.AddRange(trianglesByVertexID[triangles[i].Vertices[2]]);

                possibleNeighbors = possibleNeighbors.Where(t => t != i).ToList();

                foreach (int t in possibleNeighbors)
                {
                    if (triangles[i].Vertices.SharedBetween(triangles[t].Vertices).Length == 2)
                        neighbors.Add(t);

                    if (triangles.Count == 3)
                        break;
                }

                triangles[i].SetNeighborIDs(neighbors.ToArray());

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Setting up NavTriangles neighbors. {iteration} iteration: {i} / {triangles.Count}",
                        1f / triangles.Count * i))
                    throw new Exception("Cancel");
            }

            return triangles;
        }

        /// <summary>
        ///     Setup what entry points each triangle contains
        /// </summary>
        /// <param name="navTriangles">Triangles to check if entry point is within</param>
        /// <param name="trianglesByVertexID">A list of triangle ids based on a vertex id</param>
        /// <param name="verts">3D vertices</param>
        /// <returns>List of entry point ids based on triangle id</returns>
        private static Dictionary<int, List<NavigationPointEntry>> SetupEntryPointsForTriangles(
            NavTriangle[] navTriangles,
            IReadOnlyDictionary<int, List<int>> trianglesByVertexID, IReadOnlyList<Vector3> verts)
        {
            NavigationPointEntry[] entries = UnityObject.FindObjectsOfType<NavigationPoint>()
                .SelectMany(p => p.GetEntryPoints()).ToArray();
            Dictionary<int, List<NavigationPointEntry>> result = new Dictionary<int, List<NavigationPointEntry>>();

            for (int e = 0; e < entries.Length; e++)
            {
                Vector3 pos = entries[e].Position;
                int closestVertIndex = 0;
                float dist = Vector3.Distance(verts[0], pos);

                for (int i = 1; i < verts.Count; i++)
                {
                    float d = Vector3.Distance(verts[i], pos);
                    if (d > dist)
                        continue;

                    dist = d;
                    closestVertIndex = i;
                }

                int[] tIDs = trianglesByVertexID[closestVertIndex].ToArray();

                foreach (int i in tIDs)
                {
                    Vector3[] tPos = navTriangles[i].Vertices.Select(v => verts[v]).ToArray();
                    if (!MathC.PointWithinTriangle2DWithTolerance(pos, tPos[0], tPos[1], tPos[2]))
                        continue;

                    navTriangles[i].SetNavPointIDs(navTriangles[i].NavPoints.Append(e).ToArray());

                    if (result.TryGetValue(i, out List<NavigationPointEntry> r))
                        r.Add(entries[e]);
                    else
                        result.Add(i, new List<NavigationPointEntry>() { entries[e] });

                    break;
                }
            }

            return result;
        }

        /// <summary>
        ///     Check each vertex and if its close to another then remove the other and replace any indices containing it with the
        ///     current one.
        ///     If the other vertex is in a triangle with the current then remove said triangle.
        /// </summary>
        /// <param name="verts">3D vertices</param>
        /// <param name="indices">Each pair of threes indicate one triangle</param>
        /// <param name="vertsByPos">List of vertex ids grouped based on the divided floor int of x and z value</param>
        /// <param name="groupSize">Division value for creating the vertex groupings</param>
        /// <param name="editorProgressParTitle">Editor progressbar title</param>
        /// <exception cref="Exception">Throws "Cancel" if the user cancels the progress</exception>
        private void CheckOverlap(List<Vector3> verts, List<int> indices, Dictionary<Vector2Int, List<int>> vertsByPos,
            float groupSize, string editorProgressParTitle)
        {
            //The ids of vertices to be removed, grouped index.
            Dictionary<int, List<int>> removed = new Dictionary<int, List<int>>();
            for (int currentVertIndex = 0; currentVertIndex < verts.Count; currentVertIndex++)
            {
                //Check if the current vertex id is part of the to be removed.
                if (removed.TryGetValue(Mathf.FloorToInt(currentVertIndex / groupSize), out List<int> removedList))
                    //If its to be removed then don't check this vertex.
                    if (removedList.Contains(currentVertIndex))
                        continue;

                //2D id of the vertex based on its x and z values and grouped by group size.
                Vector2Int id = new Vector2Int(Mathf.FloorToInt(verts[currentVertIndex].x / groupSize),
                    Mathf.FloorToInt(verts[currentVertIndex].z / groupSize));

                //Get the 
                List<int> toCheck = new List<int>();
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    if (vertsByPos.TryGetValue(id + new Vector2Int(x, y), out List<int> list))
                        toCheck.AddRange(list);

                toCheck = toCheck.Where(x => x != currentVertIndex).ToList();

                foreach (int other in toCheck)
                {
                    if (removed.TryGetValue(Mathf.FloorToInt(other / groupSize), out removedList))
                        if (removedList.Contains(other))
                            continue;

                    if (Vector3.Distance(verts[currentVertIndex], verts[other]) > OverlapCheckDistance)
                        continue;

                    if (removed.TryGetValue(Mathf.FloorToInt(other / groupSize), out removedList))
                        removedList.Add(other);
                    else
                        removed.Add(Mathf.FloorToInt(other / groupSize), new List<int> { other });

                    for (int indicesIndex = 0; indicesIndex < indices.Count; indicesIndex++)
                        if (indices[indicesIndex] == other)
                            indices[indicesIndex] = currentVertIndex;
                }

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Checking vertex overlap: {currentVertIndex} / {verts.Count}",
                        1f / verts.Count * currentVertIndex))
                    throw new Exception("Cancel");
            }

            List<int> toRemove = removed.Values.SelectMany(l => l).OrderBy(x => -x).ToList();
            for (int i = 0; i < toRemove.Count; i++)
            {
                int index = toRemove[i];
                vertsByPos[
                    new Vector2Int(Mathf.FloorToInt(verts[index].x / groupSize),
                        Mathf.FloorToInt(verts[index].z / groupSize))].Remove(index);
                verts.RemoveAt(index);

                for (int j = 0; j < indices.Count; j++)
                    if (indices[j] >= index)
                        indices[j] = indices[j] - 1;

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Removing overlapping vertices: {i} / {toRemove.Count}", 1f / toRemove.Count * i))
                    throw new Exception("Cancel");
            }

            for (int i = indices.Count - 1; i >= 0; i--)
            {
                if (i % 3 != 0)
                    continue;

                if (indices[i] == indices[i + 1] || indices[i] == indices[i + 2] || indices[i + 1] == indices[i + 2] ||
                    indices[i] >= verts.Count || indices[i + 1] >= verts.Count || indices[i + 2] >= verts.Count)
                {
                    indices.RemoveAt(i);
                    indices.RemoveAt(i);
                    indices.RemoveAt(i);
                }

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Correcting indices indexes: {indices.Count - i} / {verts.Count}",
                        1f / verts.Count * (indices.Count - i)))
                    throw new Exception("Cancel");
            }
        }

        /// <summary>
        ///     Fill any holes that might have appeared by checking overlap.
        ///     If any three vertexes are directly connected to each other without having a matching triangle then add one.
        /// </summary>
        /// <param name="verts">3D vertices</param>
        /// <param name="areas">When a new triangle is created then add an area value as well</param>
        /// <param name="indices">Each pair of threes indicate one triangle</param>
        /// <param name="editorProgressParTitle">Editor loading bar title</param>
        /// <exception cref="Exception">Throws "Cancel" if the user cancels the progress</exception>
        private static void FillHoles(IList<Vector3> verts, ICollection<int> areas, IList<int> indices,
            string editorProgressParTitle)
        {
            Dictionary<int, List<int>> connectionsByIndex = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> indicesByIndex = new Dictionary<int, List<int>>();
            for (int i = 0; i < verts.Count; i++)
            {
                connectionsByIndex.Add(i, new List<int>());
                indicesByIndex.Add(i, new List<int>());
            }

            for (int i = 0; i < indices.Count; i += 3)
            {
                if (!connectionsByIndex[indices[i]].Contains(indices[i + 1]))
                    connectionsByIndex[indices[i]].Add(indices[i + 1]);
                if (!connectionsByIndex[indices[i]].Contains(indices[i + 2]))
                    connectionsByIndex[indices[i]].Add(indices[i + 2]);

                if (!connectionsByIndex[indices[i + 1]].Contains(indices[i]))
                    connectionsByIndex[indices[i + 1]].Add(indices[i]);
                if (!connectionsByIndex[indices[i + 1]].Contains(indices[i + 2]))
                    connectionsByIndex[indices[i + 1]].Add(indices[i + 2]);

                if (!connectionsByIndex[indices[i + 2]].Contains(indices[i]))
                    connectionsByIndex[indices[i + 2]].Add(indices[i]);
                if (!connectionsByIndex[indices[i + 2]].Contains(indices[i + 1]))
                    connectionsByIndex[indices[i + 2]].Add(indices[i + 1]);

                int[] arr = { indices[i], indices[i + 1], indices[i + 2] };
                indicesByIndex[indices[i]].AddRange(arr);
                indicesByIndex[indices[i + 1]].AddRange(arr);
                indicesByIndex[indices[i + 2]].AddRange(arr);

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Collecting vertex connections and indices: {i} / {indices.Count}", 1f / indices.Count * i))
                    throw new Exception("Cancel");
            }

            for (int i = 0; i < verts.Count; i++)
            {
                Vector2 p = verts[i].XZ();

                for (int j = 0; j < indices.Count; j += 3)
                {
                    if (indices[j] == i || indices[j + 1] == i || indices[j + 2] == i)
                        continue;

                    Vector2 a = verts[indices[j]].XZ(), b = verts[indices[j + 1]].XZ(), c = verts[indices[j + 2]].XZ();

                    if (!MathC.PointWithinTriangle2DWithTolerance(p, a, b, c))
                        continue;

                    Vector2 close1 = MathC.ClosetPointOnLine(p, a, b),
                        close2 = MathC.ClosetPointOnLine(p, a, c),
                        close3 = MathC.ClosetPointOnLine(p, b, b);

                    Vector2 close;
                    if (Vector2.Distance(close1, p) < Vector2.Distance(close2, p) &&
                        Vector2.Distance(close1, p) < Vector2.Distance(close3, p))
                        close = close1;
                    else if (Vector2.Distance(close2, p) < Vector2.Distance(close3, p))
                        close = close2;
                    else
                        close = close3;

                    Vector2 offset = close - p;

                    verts[i] += (offset.normalized * (offset.magnitude + .01f)).ToV3(0);
                }

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Checking point overlap with triangles: {i} / {verts.Count}", 1f / verts.Count * i))
                    throw new Exception("Cancel");
            }

            for (int original = 0; original < verts.Count; original++)
            {
                List<int> originalConnections = connectionsByIndex[original];

                for (int otherIndex = 0; otherIndex < originalConnections.Count; otherIndex++)
                {
                    int other = originalConnections[otherIndex];

                    if (other <= original)
                        continue;

                    for (int finalIndex = otherIndex + 1; finalIndex < originalConnections.Count; finalIndex++)
                    {
                        int final = originalConnections[finalIndex];

                        if (final <= original || !connectionsByIndex[final].Contains(other))
                            continue;

                        bool denied = false;

                        Vector2 a = verts[original].XZ(), b = verts[other].XZ(), c = verts[final].XZ();
                        Vector2 center = Vector2.Lerp(Vector2.Lerp(a, b, .5f), c, .5f);

                        float minX = Mathf.Min(Mathf.Min(a.x, b.x), c.x),
                            minY = Mathf.Min(Mathf.Min(a.y, b.y), c.y),
                            maxX = Mathf.Max(Mathf.Max(a.x, b.x), c.x),
                            maxY = Mathf.Max(Mathf.Max(a.y, b.y), c.y);

                        for (int x = 0; x < indices.Count; x += 3)
                        {
                            List<int> checkArr = new List<int> { indices[x], indices[x + 1], indices[x + 2] };
                            if (checkArr.Contains(original) && checkArr.Contains(other) && checkArr.Contains(final))
                            {
                                //The triangle already exists
                                denied = true;
                                break;
                            }

                            Vector2 aP = verts[checkArr[0]].XZ(),
                                bP = verts[checkArr[1]].XZ(),
                                cP = verts[checkArr[2]].XZ();

                            //Bounding
                            if (maxX < Mathf.Min(Mathf.Min(aP.x, bP.x), cP.x) ||
                                maxY < Mathf.Min(Mathf.Min(aP.y, bP.y), cP.y) ||
                                minX > Mathf.Max(Mathf.Max(aP.x, bP.x), cP.x) ||
                                minY > Mathf.Max(Mathf.Max(aP.y, bP.y), cP.y))
                                continue;

                            //One of the new triangle points is within an already existing triangle
                            if (MathC.PointWithinTriangle2DWithTolerance(center, aP, bP, cP) ||
                                MathC.PointWithinTriangle2DWithTolerance(a, aP, bP, cP) ||
                                MathC.PointWithinTriangle2DWithTolerance(b, aP, bP, cP) ||
                                MathC.PointWithinTriangle2DWithTolerance(c, aP, bP, cP))
                            {
                                denied = true;
                                break;
                            }

                            if (!MathC.TriangleIntersect2D(a, b, c, aP, bP, cP))
                                continue;

                            denied = true;
                            break;
                        }

                        if (denied)
                            continue;

                        areas.Add(0);
                        indices.Add(original);
                        indices.Add(other);
                        indices.Add(final);
                    }
                }

                if (EditorUtility.DisplayCancelableProgressBar(editorProgressParTitle,
                        $"Finding and filling holes: {original} / {verts.Count}", 1f / verts.Count * original))
                    throw new Exception("Cancel");
            }
        }

        #endregion

        #endregion
    }
}