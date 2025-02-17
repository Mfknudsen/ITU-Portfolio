#region Libraries

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.Core
{
    public sealed class Timer
    {
        #region Values

        private readonly UnityEvent timerEvent = new UnityEvent();
        private readonly float durationInSeconds;
        private float current;
        private bool done;

        #endregion

        #region Build In States

        public Timer(float durationInSeconds, UnityAction action = null)
        {
            this.durationInSeconds = durationInSeconds;
            this.timerEvent.AddListener(action);
            TimerUpdater.AddToActives(this);
        }

        #endregion

        #region Getters

        public bool IsDone => this.done;

        #endregion

        #region In

        public void Update()
        {
            if (this.done) return;

            this.current += Time.deltaTime;

            if (this.current < this.durationInSeconds) return;

            this.timerEvent.Invoke();
            this.done = true;
        }

        public void Stop() =>
            this.done = true;

        public void Reset()
        {
            this.done = false;
            this.current = 0;
            TimerUpdater.AddToActives(this);
        }

        #endregion
    }
}