#region Libraries

using Runtime.Pokémon;
using System;
using Runtime.Pokémon.Pokédex;
using UnityEngine;

#endregion

namespace Runtime.World.Overworld.Spawner
{
    [CreateAssetMenu(menuName = "Pokemon/Spawn List", fileName = "New Spawn List")]
    public class PokemonSpawnList : ScriptableObject
    {
        #region Values

        [SerializeField]
        private PokémonSpawnListEntity[] list;

        #endregion

        #region Out

        public Pokemon GetPokemonPrefab()
        {
            float total = 0;
            foreach (PokémonSpawnListEntity pokemon in this.list)
                total += pokemon.encounterRate;

            float encounter = UnityEngine.Random.Range(0, total);
            float current = 0;

            for (int i = 0; i < this.list.Length; i++)
            {
                PokémonSpawnListEntity entity = this.list[i];
                if (i == 0 && encounter <= entity.encounterRate)
                    return entity.pokemon.Get;
                else if (i == this.list.Length - 1)
                    return entity.pokemon.Get;
                else if (encounter < current + entity.encounterRate)
                    return entity.pokemon.Get;

                current += entity.encounterRate;
            }

            return null;
        }

        #endregion
    }

    [Serializable]
    public struct PokémonSpawnListEntity
    {
        #region Values

        public PokemonGetter pokemon;
        public float encounterRate;
        public int maxActiveEntities;

        #endregion
    }
}