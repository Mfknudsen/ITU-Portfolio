#region Libraries

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Navigation;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.Algorithms.PathFinding
{
    /// <summary>
    ///     Funnel algorithm use to after the A* algorithm.
    ///     Used to get the quickest path along the A* triangle path with the fewest turns.
    /// </summary>
    public static class Funnel
    {
        /// <summary>
        ///     Funnel algorithm to be used on a created path from the A* algorithm.
        /// </summary>
        /// <param name="start">Position of the agent</param>
        /// <param name="end">Target destination</param>
        /// <param name="triangleIDs">List of ids for the relevant triangles used in the a* path</param>
        /// <param name="triangles">List of triangles from the custom navmesh</param>
        /// <param name="verts">Array of 3D vertices from the custom navmesh</param>
        /// <param name="agent">The agent that requested the path</param>
        /// <returns></returns>
        public static List<Vector3> GetPath(Vector3 start, Vector3 end, int[] triangleIDs, NavTriangle[] triangles,
            Vector3[] verts, UnitAgent agent)
        {
            //Result list containing the different position for the agent to travel along.
            List<Vector3> result = new List<Vector3>(triangles.Length);
            if (triangleIDs.Length > 1)
            {
                //Apex is the newest point the agent will travel to.
                Vector2 apex = start.XZ();
                //List of portals to check.
                List<Portal> portals = GetPortals(apex, triangleIDs, triangles, verts, agent.Settings.Radius,
                    out Vector2[] remappedSimpleVerts, out Vector3[] remappedVerts);

                //Portal vert ids is the current funnel.
                Vector2 portalLeft = remappedSimpleVerts[portals[0].Left],
                    portalRight = remappedSimpleVerts[portals[0].Right];

                //Ids to be used when setting the new apex and adding to the result.
                int leftID = portals[0].Left,
                    rightID = portals[0].Right,
                    //Used when resetting the for loop to the portal which the newest apex originates from.
                    leftPortalID = 0,
                    rightPortalID = 0;

                //Checking the portals.
                for (int i = 1; i < portals.Count; i++)
                {
                    //The newest points to be checked against the current funnel.
                    Vector2 left = remappedSimpleVerts[portals[i].Left],
                        right = remappedSimpleVerts[portals[i].Right];


                    if (TriArea2(apex, portalRight, right) <= 0f)
                    {
                        //Update right if the new right point is within the funnel or the apex is the current right point
                        if (apex == portalRight ||
                            TriArea2(apex, portalLeft, right) > 0f)
                        {
                            //Tighten the funnel
                            portalRight = right;
                            rightPortalID = i;

                            if (i < portals.Count)
                                rightID = portals[i].Right;
                        }
                        else
                        {
                            //Right over left, insert left to path and restart scan from portal left point
                            result.Add(remappedVerts[leftID]);

                            //Make current left the new apex
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
                        if (apex == portalLeft ||
                            TriArea2(apex, portalRight, left) < 0f)
                        {
                            //Tighten the funnel
                            portalLeft = left;
                            leftPortalID = i;

                            if (i < portals.Count)
                                leftID = portals[i].Left;
                        }
                        else
                        {
                            //Left over right, insert right to path and restart scan from portal right point
                            result.Add(remappedVerts[rightID]);

                            //Make current right the new apex
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
                Vector2 e = end.XZ();
                bool apexFound = true;
                while (apexFound)
                {
                    apexFound = false;
                    if (TriArea2(apex, portalRight, e) > 0)
                    {
                        result.Add(remappedVerts[rightID]);
                        apex = portalRight;
                        portalLeft = apex;
                        if (rightPortalID + 1 < portals.Count)
                        {
                            rightPortalID++;
                            rightID = portals[rightPortalID].Right;
                            portalRight = remappedSimpleVerts[rightID];
                        }

                        apexFound = true;
                        continue;
                    }

                    if (TriArea2(apex, portalLeft, e) < 0)
                    {
                        result.Add(remappedVerts[leftID]);
                        apex = portalLeft;
                        portalRight = apex;
                        if (leftPortalID + 1 < portals.Count)
                        {
                            leftPortalID++;
                            leftID = portals[leftPortalID].Left;
                            portalLeft = remappedSimpleVerts[leftID];
                        }

                        apexFound = true;
                    }
                }
            }

            result.Add(end);

            return result;
        }

        /// <summary>
        ///     Creates the portals in order of the triangles.
        ///     Left and right of the portals is also properly set.
        /// </summary>
        /// <param name="start">The agents position. Used to determine left and right</param>
        /// <param name="triangleIDs">List of ids for the relevant triangles used in the path</param>
        /// <param name="triangles">List of triangles from the custom navmesh</param>
        /// <param name="verts">List of 3D vertices</param>
        /// <param name="agentRadius">The radius of the agent</param>
        /// <param name="remappedSimpleVerts">Out returns an array of remapped 2D vertices</param>
        /// <param name="remappedVerts">Out returns an array of remapped 3D vertices</param>
        /// <returns>List of portals in order from the agents start position to the target destination</returns>
        private static List<Portal> GetPortals(Vector2 start, IReadOnlyList<int> triangleIDs,
            IReadOnlyList<NavTriangle> triangles, IReadOnlyList<Vector3> verts, float agentRadius,
            out Vector2[] remappedSimpleVerts, out Vector3[] remappedVerts)
        {
            //RemappingVertices
            List<Vector3> remappedVertsResult = new List<Vector3>();
            List<Vector2> remappedSimpleVertsResult = new List<Vector2>();
            int[] shared;
            Dictionary<int, RemappedVert> remapped = new Dictionary<int, RemappedVert>();
            for (int i = 1; i < triangleIDs.Count; i++)
            {
                shared = triangles[triangleIDs[i]].Vertices.SharedBetween(triangles[triangleIDs[i - 1]].Vertices, 2);

                Vector2 ab = (verts[shared[0]] - verts[shared[1]]).XZ();

                if (remapped.TryGetValue(shared[0], out RemappedVert remappedVert))
                {
                    remappedVert.DirectionChange -= ab;
                    remapped[shared[0]] = remappedVert;
                }
                else
                {
                    remapped.Add(shared[0],
                        new RemappedVert(remapped.Count, verts[shared[0]], -ab));
                }

                if (remapped.TryGetValue(shared[1], out remappedVert))
                {
                    remappedVert.DirectionChange += ab;
                    remapped[shared[1]] = remappedVert;
                }
                else
                {
                    remapped.Add(shared[1],
                        new RemappedVert(remapped.Count, verts[shared[1]], ab));
                }
            }

            int[] key = remapped.Keys.ToArray();
            for (int i = 0; i < remapped.Count; i++)
            {
                RemappedVert remappedVert = remapped[key[i]];
                remappedVert.Set(agentRadius);
                remappedVertsResult.Add(remappedVert.Vert);
                remappedSimpleVertsResult.Add(remappedVert.SimpleVert);
                remapped[key[i]] = remappedVert;
            }

            remappedVerts = remappedVertsResult.ToArray();
            remappedSimpleVerts = remappedSimpleVertsResult.ToArray();

            //Creating portals
            shared = triangles[triangleIDs[0]].Vertices.SharedBetween(triangles[triangleIDs[1]].Vertices, 2);
            Vector2 forwardEnd = remappedSimpleVerts[remapped[shared[0]].NewID] +
                                 (remappedSimpleVerts[remapped[shared[1]].NewID] -
                                  remappedSimpleVerts[remapped[shared[0]].NewID]) * .5f;
            bool left = MathC.IsPointLeftToVector(start, forwardEnd, remappedSimpleVerts[0]);
            List<Portal> result = new List<Portal>
            {
                new Portal(remapped[shared[left ? 0 : 1]].NewID,
                    left ? 1 : 0,
                    remapped[shared[0]].NewID,
                    remapped[shared[1]].NewID)
            };

            for (int i = 1; i < triangleIDs.Count - 1; i++)
            {
                shared = triangles[triangleIDs[i]].Vertices.SharedBetween(triangles[triangleIDs[i + 1]].Vertices, 2);
                result.Add(new Portal(result[^1].Left, result[^1].Right,
                    remapped[shared[0]].NewID, remapped[shared[1]].NewID));
            }

            return result;
        }

        /// <summary>
        ///     Calculates if clockwise or counter clockwise
        /// </summary>
        /// <param name="a">Apex</param>
        /// <param name="b">Portal point</param>
        /// <param name="c">New point</param>
        /// <returns>Returns positive value if clockwise and negative value if counter clockwise</returns>
        private static float TriArea2(Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = b.x - a.x;
            float ay = b.y - a.y;
            float bx = c.x - a.x;
            float by = c.y - a.y;
            return bx * ay - ax * by;
        }
    }

    /// <summary>
    ///     Portal to be created between each triangle with the correct left and right compared to the position of the agent.
    /// </summary>
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
    internal struct RemappedVert
    {
        /// <summary>
        ///     The id of the vertices for the remapped vertices.
        ///     This struct will be placed in a dictionary with the previous id as the key.
        /// </summary>
        [NonSerialized] public readonly int NewID;

        [NonSerialized] public Vector3 Vert;
        [NonSerialized] public Vector2 SimpleVert;

        [NonSerialized] public Vector2 DirectionChange;

        public RemappedVert(int newID, Vector3 vert, Vector2 directionChange)
        {
            this.NewID = newID;
            this.Vert = vert;
            this.SimpleVert = Vector2.zero;
            this.DirectionChange = directionChange;
        }

        /// <summary>
        ///     After all the remapped vertices have been created then set the offset vert and a 2D version of it.
        /// </summary>
        /// <param name="agentRadius"></param>
        public void Set(float agentRadius)
        {
            this.SimpleVert = this.Vert.XZ() + this.DirectionChange.normalized * agentRadius * 1.25f;
            this.Vert = new Vector3(this.SimpleVert.x, this.Vert.y, this.SimpleVert.y);
        }
    }
}