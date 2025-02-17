namespace Runtime.VFX.Scene
{
    public abstract class SceneEffect : EffectBase
    {
        #region Values

        protected bool isSwitching, active;

        #endregion

        #region Getters

        public bool IsActive => this.active;

        #endregion

        #region In

        public virtual void UpdateEffect()
        {
        }

        public void CheckRules()
        {
            if (this.isSwitching) return;

            bool check = this.Rules();

            if (check == this.active) return;

            this.isSwitching = true;
            this.active = true;

            if (this.active)
                this.Play();
            else
                this.Stop();
        }

        #endregion

        #region Internal

        protected abstract bool Rules();

        protected abstract void Play();

        protected abstract void Stop();

        #endregion
    }
}