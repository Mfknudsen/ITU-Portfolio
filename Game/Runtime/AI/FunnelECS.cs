using System;
using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Runtime.AI
{
    [BurstCompile]
    public static class FunnelEcs
    {
        [BurstCompile]
        public static void GetPath(in float3 start, in float3 end, in NativeArray<int> triangleIDs,
            in NativeArray<NavTriangleBufferElement> triangles,
            in NativeArray<VertBufferElement> verts,
            ref DynamicBuffer<WayPointBufferElement> agentPath,
            in AgentSettingsComponent agentSettings)
        {
            //Result list containing the different position for the agent to travel along.
            if (triangleIDs.Length > 1)
            {
                //Apex is the newest point the agent will travel to.
                float2 apex = start.XZ();
                //List of portals to check.
                GetPortals(apex,
                    triangleIDs,
                    triangles, verts,
                    agentSettings.Radius,
                    out NativeArray<float2> remappedSimpleVerts,
                    out NativeArray<float3> remappedVerts,
                    out NativeArray<int> rightArray,
                    out NativeArray<int> leftArray);

                int portalCount = rightArray.Length;

                //Portal vert ids is the current funnel.
                float2 portalLeft = remappedSimpleVerts[leftArray[0]],
                    portalRight = remappedSimpleVerts[rightArray[0]];

                //Ids to be used when setting the new apex and adding to the result.
                int leftID = leftArray[0],
                    rightID = rightArray[0],
                    //Used when resetting the for loop to the portal which the newest apex originates from.
                    leftPortalID = 0,
                    rightPortalID = 0;

                //Checking the portals.
                for (int i = 1; i < leftArray.Length; i++)
                {
                    //The newest points to be checked against the current funnel.
                    float2 left = remappedSimpleVerts[leftArray[i]],
                        right = remappedSimpleVerts[rightArray[i]];


                    if (TriArea2(apex, portalRight, right) <= 0f)
                    {
                        //Update right if the new right point is within the funnel or the apex is the current right point
                        if (apex.Equals(portalRight) ||
                            TriArea2(apex, portalLeft, right) > 0f)
                        {
                            //Tighten the funnel
                            portalRight = right;
                            rightPortalID = i;

                            if (i < portalCount)
                                rightID = rightArray[i];
                        }
                        else
                        {
                            //Right over left, insert left to path and restart scan from portal left point
                            agentPath.Add(new WayPointBufferElement()
                            {
                                Point = remappedVerts[leftID],
                                IsWalk = true
                            }); //Make current left the new apex
                            apex = portalLeft;
                            portalRight = portalLeft;

                            //Reset
                            i = leftPortalID;
                            continue;
                        }
                    }

                    if (TriArea2(apex, portalLeft, left) >= 0f)
                    {
                        //Update left if the new left point is within the funnel or the apex is the current left point
                        if (apex.Equals(portalLeft) ||
                            TriArea2(apex, portalRight, left) < 0f)
                        {
                            //Tighten the funnel
                            portalLeft = left;
                            leftPortalID = i;

                            if (i < portalCount)
                                leftID = leftArray[i];
                        }
                        else
                        {
                            //Left over right, insert right to path and restart scan from portal right point
                            agentPath.Add(new WayPointBufferElement()
                            {
                                Point = remappedVerts[rightID],
                                IsWalk = true
                            }); //Make current right the new apex
                            apex = portalRight;
                            portalLeft = portalRight;
                            leftPortalID = rightPortalID;

                            //Reset
                            i = rightPortalID;
                        }
                    }
                }


                //The end point might be outside the current funnel after the original algorithm.
                //This catches those. 
                float2 e = end.XZ();
                bool apexFound = true;
                while (apexFound)
                {
                    apexFound = false;
                    if (TriArea2(apex, portalRight, e) > 0)
                    {
                        agentPath.Add(new WayPointBufferElement()
                        {
                            Point = remappedVerts[rightID],
                            IsWalk = true
                        });
                        apex = portalRight;
                        portalLeft = apex;
                        if (rightPortalID + 1 < portalCount)
                        {
                            rightPortalID++;
                            rightID = rightArray[rightPortalID];
                            portalRight = remappedSimpleVerts[rightID];
                        }

                        apexFound = true;
                        continue;
                    }

                    if (TriArea2(apex, portalLeft, e) < 0)
                    {
                        agentPath.Add(new WayPointBufferElement()
                        {
                            Point = remappedVerts[leftID],
                            IsWalk = true
                        });
                        apex = portalLeft;
                        portalRight = apex;
                        if (leftPortalID + 1 < portalCount)
                        {
                            leftPortalID++;
                            leftID = leftArray[leftPortalID];
                            portalLeft = remappedSimpleVerts[leftID];
                        }

                        apexFound = true;
                    }
                }
            }

            agentPath.Add(new WayPointBufferElement()
            {
                Point = end,
                IsWalk = true
            });
        }

        [BurstCompile]
        private static void GetPortals(in float2 start, in NativeArray<int> triangleIDs,
            in NativeArray<NavTriangleBufferElement> triangles, in NativeArray<VertBufferElement> verts,
            float agentRadius,
            out NativeArray<float2> remappedSimpleVerts, out NativeArray<float3> remappedVerts,
            out NativeArray<int> rightArray, out NativeArray<int> leftArray)
        {
            //RemappingVertices
            NativeList<float3> remappedVertsResult = new NativeList<float3>(verts.Length, Allocator.Temp);
            NativeList<float2> remappedSimpleVertsResult = new NativeList<float2>(verts.Length, Allocator.Temp);
            NativeList<int> shared = new NativeList<int>(2, Allocator.Temp);

            NativeList<int> rightArrayResult = new NativeList<int>(verts.Length / 2, Allocator.Temp),
                leftArrayResult = new NativeList<int>(verts.Length / 2, Allocator.Temp);

            NativeList<int> oldIndex = new NativeList<int>(triangleIDs.Length / 3, Allocator.Temp);
            NativeList<float3> directions = new NativeList<float3>(oldIndex.Length, Allocator.Temp);

            for (int i = 1; i < triangleIDs.Length; i++)
            {
                Shared(triangles[triangleIDs[i]], triangles[triangleIDs[i - 1]], ref shared);

                float3 ab = verts[shared[0]].Position - verts[shared[1]].Position;

                int index = Contains(oldIndex, shared[0]);
                if (index == -1)
                {
                    oldIndex.Add(shared[0]);
                    directions.Add(-ab);
                }
                else
                {
                    directions[index] -= ab;
                }

                index = Contains(oldIndex, shared[1]);
                if (index == -1)
                {
                    oldIndex.Add(shared[1]);
                    directions.Add(ab);
                }
                else
                {
                    directions[index] += ab;
                }

                /*
                JobLogger.Log(remapped.Count);
                if (remapped.TryGetValue(shared[0], out RemappedVert remappedVert))
                {
                    remappedVert.DirectionChange -= ab;
                    remapped[shared[0]] = remappedVert;
                }
                else
                {
                    remapped.Add(shared[0],
                        new RemappedVert(remapped.Count, simpleVerts[shared[0]].To3(vertsY[shared[0]]), -ab));
                }

                if (remapped.TryGetValue(shared[1], out remappedVert))
                {
                    remappedVert.DirectionChange += ab;
                    remapped[shared[1]] = remappedVert;
                }
                else
                {
                    remapped.Add(shared[1],
                        new RemappedVert(remapped.Count, simpleVerts[shared[1]].To3(vertsY[shared[1]]), ab));
                }
                */
            }

            for (int i = 0; i < oldIndex.Length; i++)
            {
                float3 vert = verts[oldIndex[i]].Position +
                              directions[i].Normalize() * agentRadius * 1.25f;
                remappedVertsResult.Add(vert);
                remappedSimpleVertsResult.Add(vert.XZ());
            }

            remappedVerts = remappedVertsResult.AsArray();
            remappedSimpleVerts = remappedSimpleVertsResult.AsArray();

            //Creating portals
            Shared(triangles[triangleIDs[0]], triangles[triangleIDs[1]], ref shared);
            int a = Contains(oldIndex, shared[0]), b = Contains(oldIndex, shared[1]);
            float2 forwardEnd = remappedSimpleVerts[a] +
                                (remappedSimpleVerts[b] -
                                 remappedSimpleVerts[a]) * .5f;
            bool left = MathC.IsPointLeftToVector(start, forwardEnd, remappedSimpleVerts[0]);

            if (left)
            {
                leftArrayResult.Add(0);
                rightArrayResult.Add(1);
            }
            else
            {
                leftArrayResult.Add(1);
                rightArrayResult.Add(0);
            }

            for (int i = 1; i < triangleIDs.Length - 1; i++)
            {
                Shared(triangles[triangleIDs[i]], triangles[triangleIDs[i + 1]], ref shared);
                a = Contains(oldIndex, shared[0]);
                b = Contains(oldIndex, shared[1]);
                if (leftArrayResult[^1] == a || rightArrayResult[^1] == b)
                {
                    leftArrayResult.Add(a);
                    rightArrayResult.Add(b);
                }
                else
                {
                    leftArrayResult.Add(b);
                    rightArrayResult.Add(a);
                }
            }

            /*
            NativeList<Portal> result = new NativeList<Portal>
            {
                new Portal(remapped[shared[left ? 0 : 1]].NewID,
                    left ? 1 : 0,
                    remapped[shared[0]].NewID,
                    remapped[shared[1]].NewID)
            };

            for (int i = 1; i < triangleIDs.Length - 1; i++)
            {
                Shared(triangles[triangleIDs[i]], triangles[triangleIDs[i + 1]], ref shared);
                result.Add(new Portal(result[^1].Left, result[^1].Right,
                    remapped[shared[0]].NewID, remapped[shared[1]].NewID));
            }
            */

            rightArray = rightArrayResult;
            leftArray = leftArrayResult;
        }

        /// <summary>
        ///     Calculates if clockwise or counter clockwise
        /// </summary>
        /// <param name="a">Apex</param>
        /// <param name="b">Portal point</param>
        /// <param name="c">New point</param>
        /// <returns>Returns positive value if clockwise and negative value if counter clockwise</returns>
        [BurstCompile]
        private static float TriArea2(in float2 a, in float2 b, in float2 c)
        {
            float ax = b.x - a.x;
            float ay = b.y - a.y;
            float bx = c.x - a.x;
            float by = c.y - a.y;
            return bx * ay - ax * by;
        }

        [BurstCompile]
        private static void Shared(in NavTriangleBufferElement a, in NavTriangleBufferElement b,
            ref NativeList<int> result)
        {
            if (a.A == b.A)
                result.Add(a.A);
            if (a.A == b.B)
                result.Add(a.A);
            if (a.A == b.C)
                result.Add(a.A);
            if (a.B == b.A)
                result.Add(a.B);
            if (a.B == b.B)
                result.Add(a.B);
            if (a.B == b.C)
                result.Add(a.B);
            if (a.C == b.A)
                result.Add(a.C);
            if (a.C == b.B)
                result.Add(a.C);
            if (a.C == b.C)
                result.Add(a.C);
        }

        [BurstCompile]
        private static int Contains(in NativeList<int> array, in int index)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == index)
                    return i;
            }

            return -1;
        }
    }

    /// <summary>
    ///     Portal to be created between each triangle with the correct left and right compared to the position of the agent.
    /// </summary>
    [BurstCompile]
    public readonly struct Portal
    {
        /// <summary>
        ///     Vertices id to be used with the remapped vertices list.
        /// </summary>
        [NonSerialized] public readonly int Left, Right;

        public Portal(int previousLeft, int previousRight, int a, int b)
        {
            if (previousLeft == a || previousRight == b)
            {
                this.Left = a;
                this.Right = b;
            }
            else
            {
                this.Left = b;
                this.Right = a;
            }
        }
    }

    /// <summary>
    ///     Used to remap the vertices from the custom navmesh to match the agents' radius.
    ///     Remapping will insure the agent don't hit things like buildings while traveling the path.
    /// </summary>
    [BurstCompile]
    internal struct RemappedVert
    {
        /// <summary>
        ///     The id of the vertices for the remapped vertices.
        ///     This struct will be placed in a dictionary with the previous id as the key.
        /// </summary>
        public readonly int NewID;

        public float3 Vert;
        public float2 SimpleVert;

        public float2 DirectionChange;

        public RemappedVert(int newID, float3 vert, float2 directionChange)
        {
            this.NewID = newID;
            this.Vert = vert;
            this.SimpleVert = float2.zero;
            this.DirectionChange = directionChange;
        }

        /// <summary>
        ///     After all the remapped vertices have been created then set the offset vert and a 2D version of it.
        /// </summary>
        /// <param name="agentRadius"></param>
        public void Set(float agentRadius)
        {
            this.SimpleVert = this.Vert.XZ() + this.DirectionChange.Normalize() * agentRadius * 1.25f;
            this.Vert = new float3(this.SimpleVert.x, this.Vert.y, this.SimpleVert.y);
        }
    }
}