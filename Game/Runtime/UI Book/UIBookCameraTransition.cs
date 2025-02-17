#region Libraries

using System;
using System.Collections;
using Runtime.Core;
using Runtime.Systems;
using Unity.Cinemachine;
using UnityEngine;

#endregion

namespace Runtime.UI_Book
{
    [ExecuteInEditMode]
    public class UIBookCameraTransition : MonoBehaviour, IOperation
    {
        #region Values

        // ReSharper disable once InconsistentNaming
        public bool DEBUG;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private Transform toMove, endFollow;
        [SerializeField] private Transform middleLeft, middleRight;
        [SerializeField] private Transform start, end;
        [SerializeField] private float moveSpeed = 1;
        [SerializeField] private CinemachineBlenderSettings settings;

        private Transform middle;
        private float t;
        private CinemachineVirtualCamera cinemachineVirtualCamera;
        private bool done;

        #endregion

        #region Build In States

        private void OnValidate()
        {
            if (this.settings == null || this.settings.CustomBlends == null)
                return;

            this.settings.CustomBlends[0].Blend.Time = 1 - Math.Abs(this.moveSpeed);
            this.settings.CustomBlends[1].Blend.Time = 1 - Math.Abs(this.moveSpeed);
        }

        private void Awake()
        {
            this.cinemachineVirtualCamera = this.toMove.gameObject.GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (this.end != null && this.endFollow != null) this.end.position = this.endFollow.position;

#if UNITY_EDITOR
            if (!this.DEBUG) return;

            Vector3 startPosition = this.start.position,
                endPosition = this.end.position;

            Vector3 oldRightPos = startPosition,
                oldLeftPos = startPosition;

            float floatTime = 0;

            while (floatTime <= 1)
            {
                Vector3 rightPos = MathC.CurveLerpPosition(this.curve,
                    floatTime,
                    startPosition, this.middleRight.position,
                    endPosition);

                Vector3 leftPos = MathC.CurveLerpPosition(this.curve,
                    floatTime,
                    startPosition, this.middleLeft.position,
                    endPosition);

                Debug.DrawLine(
                    oldRightPos,
                    rightPos);

                Debug.DrawLine(
                    oldLeftPos,
                    leftPos);

                oldRightPos = rightPos;
                oldLeftPos = leftPos;
                floatTime += 0.025f;
            }
#endif
        }

        #endregion

        #region Getters

        public float GetSpeed()
        {
            return this.moveSpeed;
        }

        #endregion

        #region In

        public void Direction(bool awayFromBook)
        {
            if ((awayFromBook && this.moveSpeed < 0) ||
                (!awayFromBook && this.moveSpeed > 0))
                this.moveSpeed *= -1;

            this.t = this.moveSpeed > 0 ? 0 : 1;
        }

        public void OperationEnd()
        {
            this.cinemachineVirtualCamera.enabled = this.moveSpeed < 0;
        }

        public void CheckMiddle()
        {
            Vector3 endPosition = this.end.position;
            this.middle =
                Vector3.Distance(endPosition, this.middleRight.position) <=
                Vector3.Distance(endPosition, this.middleLeft.position)
                    ? this.middleRight
                    : this.middleLeft;
        }

        #endregion

        #region Out

        public IEnumerator Operation()
        {
            this.done = false;

            while (this.t is <= 1 and >= 0)
            {
                this.toMove.position = MathC.CurveLerpPosition(this.curve, this.t, this.start.position, this.middle.position,
                    this.end.position);
                this.t += this.moveSpeed * Time.deltaTime;
                yield return null;
            }

            this.done = true;
        }

        public bool IsOperationDone => this.done;

        public float GetTimeToComplete()
        {
            return 1 / this.moveSpeed;
        }

        #endregion
    }
}