using Runtime.AI.EntityAspects;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.AI.Navigation;
using Unity.Entities;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    public partial struct DebugTrianglesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Entity singletonEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();
            NavigationMeshSingletonComponent navmesh =
                SystemAPI.GetComponent<NavigationMeshSingletonComponent>(singletonEntity);

            Vector3 startPoint = new Vector3(navmesh.MinFloorX, 0,
                navmesh.MinFloorZ);
            Vector3 xDir = Vector3.right * NavigationMesh.GroupDivision,
                zDir = Vector3.forward * NavigationMesh.GroupDivision;

            DynamicBuffer<VertXZBufferElement> vertXZBufferElements =
                SystemAPI.GetBuffer<VertXZBufferElement>(singletonEntity);
            DynamicBuffer<VertYBufferElement> vertYBufferElements =
                SystemAPI.GetBuffer<VertYBufferElement>(singletonEntity);
            DynamicBuffer<NavTriangleBufferElement> navTriangleBufferElements =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(singletonEntity);

            foreach (NavTriangleBufferElement triangle in navTriangleBufferElements)
            {
                Vector3 a = vertXZBufferElements[triangle.A].ToV3(vertYBufferElements[triangle.A]),
                    b = vertXZBufferElements[triangle.B].ToV3(vertYBufferElements[triangle.B]),
                    c = vertXZBufferElements[triangle.C].ToV3(vertYBufferElements[triangle.C]);

                Debug.DrawLine(a, b);
                Debug.DrawLine(a, c);
                Debug.DrawLine(c, b);
            }

            int i = 0;
            foreach (UpdateCellAspect aspect in
                     SystemAPI
                         .Query<UpdateCellAspect>())
            {
                i += 10;

                Vector3 p = startPoint + xDir * aspect.CellComponent.ValueRO.X + zDir * aspect.CellComponent.ValueRO.Z +
                            Vector3.up * i;
                Debug.DrawRay(p, xDir);
                Debug.DrawRay(p, zDir);
                Debug.DrawRay(p + xDir + zDir, -xDir);
                Debug.DrawRay(p + xDir + zDir, -zDir);

                foreach (NavMeshCellTriangleIndexBufferElement triangleIndex in aspect.NavTriangleBufferElements)
                {
                    NavTriangleBufferElement triangle = navTriangleBufferElements[triangleIndex.Index];
                    Vector3 a = vertXZBufferElements[triangle.A].ToV3(vertYBufferElements[triangle.A]),
                        b = vertXZBufferElements[triangle.B].ToV3(vertYBufferElements[triangle.B]),
                        c = vertXZBufferElements[triangle.C].ToV3(vertYBufferElements[triangle.C]);

                    a += Vector3.up * i;
                    b += Vector3.up * i;
                    c += Vector3.up * i;

                    Debug.DrawLine(a, b, Color.magenta);
                    Debug.DrawLine(a, c, Color.magenta);
                    Debug.DrawLine(c, b, Color.magenta);
                }
            }
        }
    }
}