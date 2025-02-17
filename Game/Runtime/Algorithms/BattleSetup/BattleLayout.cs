namespace Runtime.Algorithms.BattleSetup
{
    public readonly struct BattleLayout
    {
        #region Values

        public readonly bool Found;

        #endregion

        #region Build In States

        public BattleLayout(bool found)
        {
            this.Found = found;
        }

        #endregion
    }
}