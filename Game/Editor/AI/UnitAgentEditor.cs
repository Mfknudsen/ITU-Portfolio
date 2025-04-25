#region Libraries

using System.Collections.Generic;
using Editor.Common;
using Runtime.AI.Navigation;
using Runtime.AI.Navigation.RayCast;
using Runtime.Core;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

#endregion

namespace Editor.AI
{
#if UNITY_EDITOR

    [CustomEditor(typeof(NavigationAgent))]
    public sealed class UnitAgentEditor : OdinEditor
    {
        #region Values

        private NavigationAgent agent;

        #endregion

        #region Build In States

        protected override void OnEnable()
        {
            base.OnEnable();

            this.agent = (NavigationAgent)this.target;
        }

        private void OnSceneGUI()
        {
            if (this.agent == null)
                return;

            UnitAgentSettings settings = this.agent.Settings;
            if (settings != null)
            {
                Draw.DrawCylinder(this.agent.transform.position, settings.Height, settings.Radius,
                    Color.yellow);

                Draw.DrawCircle(this.agent.transform.position + Vector3.up, this.agent.Settings.AvoidanceRadius,
                    Color.blue);
            }

            List<NavigationRayCastObject> objects = this.agent.GetRayCastObjects();
            if (objects == null || objects.Count == 0)
                return;

            foreach (NavigationRayCastObject navigationRayCastObject in objects)
            {
                float y = this.agent.transform.position.y + settings.Height;
                if (navigationRayCastObject is CircleObject circleObject)
                {
                    Draw.DrawCircle(circleObject.GetPosition().ToV3(y), circleObject.GetRadius(), Color.blue);
                }
                else if (navigationRayCastObject is LineObject lineObject)
                {
                    Debug.DrawLine(lineObject.GetA().ToV3(y), lineObject.GetB().ToV3(y), Color.blue);
                }
            }

            bool hit = objects.DoRayCast(this.agent, this.agent.GetCurrentNavMeshDirection(),
                out RayHitResult hitResult);

            Vector3 agentCenterA = this.agent.transform.position + Vector3.up * 1.15f;
            Vector2 directionXZ = this.agent.GetCurrentNavMeshDirection();
            if (hit)
            {
                Debug.DrawLine(agentCenterA,
                    hitResult.point.ToV3(agentCenterA.y), Color.red);
                Draw.DrawCircle(hitResult.point.ToV3(agentCenterA.y), this.agent.Settings.Radius, Color.red);

                int i = 0;
                const float angle = 80f / 6;
                while (i < 6)
                {
                    i++;
                    Vector2 rightDir = MathC.RotateBy(directionXZ, i * angle);
                    if (!objects.DoRayCast(this.agent, rightDir, out hitResult))
                    {
                        directionXZ = rightDir;
                        break;
                    }

                    Debug.DrawLine(agentCenterA,
                        hitResult.point.ToV3(agentCenterA.y), Color.red);
                    Draw.DrawCircle(hitResult.point.ToV3(agentCenterA.y), this.agent.Settings.Radius, Color.red);

                    Vector2 leftDir = MathC.RotateBy(directionXZ, -i * angle);

                    if (!objects.DoRayCast(this.agent, leftDir, out hitResult))
                    {
                        directionXZ = leftDir;
                        break;
                    }

                    Debug.DrawLine(this.agent.transform.position + Vector3.up * 1.15f,
                        hitResult.point.ToV3(this.agent.transform.position.y + 1.15f), Color.red);
                    Draw.DrawCircle(hitResult.point.ToV3(agentCenterA.y), this.agent.Settings.Radius, Color.red);
                }
            }

            Debug.DrawRay(this.agent.transform.position + Vector3.up * .5f, directionXZ.ToV3(0), Color.green);
        }

        #endregion
    }
#endif
}