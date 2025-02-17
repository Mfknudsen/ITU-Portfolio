namespace Runtime.Pokémon.Evolutions
{
    public abstract class EvolutionMethod
    {
        #region Out

        public abstract bool Check(object[] inputObjects = null);

        #endregion
    }
}