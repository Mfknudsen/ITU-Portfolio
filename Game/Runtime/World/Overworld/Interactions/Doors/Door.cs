#region Libraries

using System;
using System.Collections;
using Runtime.Core;
using Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
 
#endregion

namespace Runtime.World.Overworld.Interactions.Doors
{
    public class Door : MonoBehaviour, IInteractable
    {
        #region Values

        [SerializeField, Required] private PlayerManager playerManager;

        [SerializeField] private StartPoint[] startPoints = new StartPoint[0];

        #endregion

        #region In

        public void InteractTrigger()
        {
            this.playerManager.DisablePlayerControl();

            this.StartCoroutine(this.GoThroughDoor());
        }

        #endregion

        #region Internal

        private IEnumerator GoThroughDoor()
        {
            NavMeshAgent agent = playerManager.GetAgent();

            Vector3 playerPos = agent.transform.position;
            StartPoint selectedPoint = startPoints[0];
            float distance = playerPos.QuickSquareDistance(selectedPoint.GetPosition);

            for (int i = 1; i < startPoints.Length; i++)
            {
                float d = playerPos.QuickSquareDistance(startPoints[i].GetPosition);

                if (d < distance)
                {
                    distance = d;
                    selectedPoint = startPoints[i];
                }
            }

            agent.SetDestination(selectedPoint.GetPosition);

            while (!agent.isStopped)
            {
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, selectedPoint.GetRotation, Time.deltaTime);
                yield return null;
            }

            //Play animation
            

            // Release Player
        }

        #endregion
    }

    [Serializable]
    internal struct StartPoint
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private AnimationClip animationClip;

        public Vector3 GetPosition => position;
        public Quaternion GetRotation => rotation;
        public AnimationClip GetAnimationClip => animationClip;
    }
}