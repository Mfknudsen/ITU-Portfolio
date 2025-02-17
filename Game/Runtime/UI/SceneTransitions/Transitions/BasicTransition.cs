#region Packages

using System.Collections;
using UnityEngine;

// ReSharper disable Unity.PreferAddressByIdToGraphicsParams

#endregion

namespace Runtime.UI.SceneTransitions.Transitions
{
    [CreateAssetMenu(menuName = "UI/Transition/Basic")]
    public class BasicTransition : Transition
    {
        #region Values

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject gameObjectToAnimate;

        private GameObject instantiatedGameObject;

        #endregion

        public override IEnumerator Trigger(bool start)
        {
            if (start)
            {
                this.animator = this.transitionUI.InstantiateObject(this.gameObjectToAnimate).GetComponent<Animator>();
                this.instantiatedGameObject = this.animator.gameObject;
            }
            else
                this.onHide?.Invoke();

            SetAnimationBools(this.animator, start);

            //Let Unity Update so we get the clip we want
            yield return null;

            yield return new WaitForSeconds(GetTimeOfClipByName(this.animator));

            if (!start)
                Destroy(this.instantiatedGameObject);
        }
    }
}