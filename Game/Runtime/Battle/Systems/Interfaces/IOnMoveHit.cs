using System.Collections;
using Runtime.Battle.Actions;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IOnMoveHit
    {
        public bool MultiHit(PokemonMove pokemonMove);

        public IEnumerator Operation();
    }
}
