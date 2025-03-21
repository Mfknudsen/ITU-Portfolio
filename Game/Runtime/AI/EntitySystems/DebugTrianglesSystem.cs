using Runtime.AI.EntityAspects;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.AI.Navigation;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(NavigationSystemGroup))]
    [BurstCompile]
    public partial struct DebugTrianglesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            state.RequireForUpdate<NavigationMeshSingletonComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity singletonEntity = SystemAPI.GetSingletonEntity<NavigationMeshSingletonComponent>();
            NavigationMeshSingletonComponent navmesh =
                SystemAPI.GetComponent<NavigationMeshSingletonComponent>(singletonEntity);

            Vector3 startPoint = new Vector3(navmesh.MinFloorX, 0,
                navmesh.MinFloorZ);
            Vector3 xDir = Vector3.right * NavigationMesh.GroupDivision,
                zDir = Vector3.forward * NavigationMesh.GroupDivision;

            DynamicBuffer<VertBufferElement> vertXZBufferElements =
                SystemAPI.GetBuffer<VertBufferElement>(singletonEntity);
            DynamicBuffer<NavTriangleBufferElement> navTriangleBufferElements =
                SystemAPI.GetBuffer<NavTriangleBufferElement>(singletonEntity);

            foreach (NavTriangleBufferElement triangle in navTriangleBufferElements)
            {
                Vector3 a, b;
                if (triangle.NeighborOneId > triangle.ID)
                {
                    a = vertXZBufferElements[triangle.NeighborOneA].Position;
                    b = vertXZBufferElements[triangle.NeighborOneB].Position;
                    Debug.DrawLine(a, b);
                }

                if (triangle.NeighborTwoId > triangle.ID)
                {
                    a = vertXZBufferElements[triangle.NeighborTwoA].Position;
                    b = vertXZBufferElements[triangle.NeighborTwoB].Position;
                    Debug.DrawLine(a, b);
                }

                if (triangle.NeighborThreeId <= triangle.ID) continue;

                a = vertXZBufferElements[triangle.NeighborThreeA].Position;
                b = vertXZBufferElements[triangle.NeighborThreeB].Position;
                Debug.DrawLine(a, b);
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

                int size = aspect.CellComponent.ValueRO.Size;
                DynamicBuffer<NavMeshCellTriangleIndexBufferElement> triangleIds = aspect.NavTriangleBufferElements;

                for (int j = 0; j < size; j++)
                {
                    NavTriangleBufferElement triangle = navTriangleBufferElements[triangleIds[j].Index];
                    Vector3 a = vertXZBufferElements[triangle.A].Position,
                        b = vertXZBufferElements[triangle.B].Position,
                        c = vertXZBufferElements[triangle.C].Position;

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