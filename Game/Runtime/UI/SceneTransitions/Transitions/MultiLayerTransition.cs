#region Packages

using System;
using System.Collections;
using UnityEngine;

#endregion

namespace Runtime.UI.SceneTransitions.Transitions
{
    [CreateAssetMenu(menuName = "UI/Transition/Multi-Layer")]
    public class MultiLayerTransition : Transition
    {
        #region Values

        [SerializeField] private TransitionLayer[] transitionLayers;

        #endregion

        public override IEnumerator Trigger(bool start)
        {
            foreach (TransitionLayer layer in this.transitionLayers) this.transitionUI.StartCoroutine(layer.Trigger(start, this.transitionUI));

            foreach (TransitionLayer layer in this.transitionLayers)
            {
                while (!layer.Done())
                    yield return null;
            }
        }
    }

    [Serializable]
    public class TransitionLayer : MonoBehaviour
    {
        #region Values

        [SerializeField] private GameObject gameObjectToAnimate;

        [SerializeField] private float timeFromStart;
        [SerializeField] private Animator animator;

        private bool done;

        #endregion

        #region In

        public IEnumerator Trigger(bool start, SceneTransitionUI transitionUI)
        {
            this.done = false;

            yield return new WaitForSeconds(this.timeFromStart);

            if (start) this.animator = transitionUI.InstantiateObject(this.gameObjectToAnimate).GetComponent<Animator>();

            Transition.SetAnimationBools(this.animator, start);

            //Let Unity Update so we get the clip we want
            yield return null;

            yield return new WaitForSeconds(Transition.GetTimeOfClipByName(this.animator));

            this.done = true;
        }

        public bool Done()
        {
            return this.done;
        }

        public void End()
        {
            Destroy(this.animator.gameObject);
        }

        #endregion
    }
}