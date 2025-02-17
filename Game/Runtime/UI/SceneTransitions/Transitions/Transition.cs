#region Packages

using System.Collections;
using UnityEngine;

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams
// ReSharper disable ParameterHidesMember

#endregion

namespace Runtime.UI.SceneTransitions.Transitions
{
    public abstract class Transition : ScriptableObject
    {
        #region Values

        protected SceneTransitionUI transitionUI;

        public delegate void OnHide();

        public OnHide onHide;
        
        #endregion

        #region Setters

        public void SetTransitionParent(SceneTransitionUI transitionUI)
        {
            this.transitionUI = transitionUI;
        }

        #endregion

        #region In

        // ReSharper disable once IdentifierTypo
        public static void SetAnimationBools(Animator animator, bool start)
        {
            animator.SetBool("Start", start);
            animator.SetBool("End", !start);
        }

        #endregion

        #region Out

        public abstract IEnumerator Trigger(bool start);

        public static float GetTimeOfClipByName(Animator anim)
        {
            return anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        }

        #endregion

        #region Internal

        protected IEnumerator TriggerOnHide(float timeToHide)
        {
            yield return new WaitForSeconds(timeToHide);

            this.onHide?.Invoke();
        }

        #endregion
    }
}