#region Packages

using System;
using System.Collections;
using Runtime.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.UI_Book
{
    [Serializable]
    internal sealed class BookTurnAction : IOperation
    {
        #region Values

        [SerializeField, Required] private UIBook uiBook;
        [SerializeField, Required] private GameObject turnLeftPaper, turnRightPaper, openLeft, openRight;

        private bool fromLeftToRight;
        private bool done;
        private static readonly int InvertPageID = Shader.PropertyToID("InvertPage");

        #endregion

        public void SetDirection(bool set)
        {
            this.fromLeftToRight = set;
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            this.SetOpens(false);
            this.SetTurns(true);

            this.turnLeftPaper.GetComponent<Renderer>().material.SetInt(InvertPageID, this.fromLeftToRight ? 0 : 1);
            this.turnRightPaper.GetComponent<Renderer>().material.SetInt(InvertPageID, this.fromLeftToRight ? 1 : 0);

            const float animationTime = 0.5f;

            yield return new WaitForSeconds(animationTime * 0.1f);

            if (!this.fromLeftToRight)
                this.openRight.SetActive(true);
            else
                this.openLeft.SetActive(true);

            yield return new WaitForSeconds(animationTime * 0.9f);

            this.SetTurns(false);
            this.SetOpens(true);

            this.uiBook.ConstructUI();
            this.done = true;
        }

        public void OperationEnd()
        {
        }

        #region Internal

        private void SetOpens(bool set)
        {
            this.openLeft.SetActive(set);
            this.openRight.SetActive(set);
        }

        private void SetTurns(bool set)
        {
            this.turnLeftPaper.SetActive(set);
            this.turnRightPaper.SetActive(set);
        }

        #endregion
    }
}