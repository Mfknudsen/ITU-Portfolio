#region Libraries

using System.Collections.Generic;
using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.AI.Navigation.RayCast
{
    public static class NavigationRayCast
    {
        public static bool DoRayCast(this List<NavigationRayCastObject> objects, NavigationAgent agent, Vector2 direction,
            out RayHitResult result)
        {
            Vector2 hit = Vector2.zero;
            Vector2 agentPosition = agent.transform.position.XZ();
            float dist = 0;
            bool hasHit = false;
            foreach (NavigationRayCastObject obj in objects)
            {
                if (!obj.Check(agentPosition, direction * agent.Settings.AvoidanceRadius, agent.Settings.Radius * .9f,
                        out Vector2 rayHit))
                    continue;

                if (dist != 0 && agentPosition.QuickSquareDistance(rayHit) >= dist) continue;

                dist = agentPosition.QuickSquareDistance(rayHit);
                hit = rayHit;
                hasHit = true;
            }


            result = new RayHitResult(hit, dist);
            return hasHit;
        }
    }

    public readonly struct RayHitResult
    {
        #region Values

        public readonly Vector2 point;
        public readonly float squaredDist;

        #endregion

        #region Build In States

        public RayHitResult(Vector2 point, float squaredDist)
        {
            this.point = point;
            this.squaredDist = squaredDist;
        }

        #endregion
    }
}