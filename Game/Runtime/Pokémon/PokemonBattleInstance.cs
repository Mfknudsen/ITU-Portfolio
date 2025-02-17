using System.Collections.Generic;
using Runtime.Battle.Actions;
using UnityEngine;

namespace Runtime.Pokémon
{
    public struct PokemonBattleInstance 
    {
         public int spotIndex;
         public GameObject prefab;
         public GameObject spawnedObject;
         public Animator anim;
         public bool gettingSwitched, inBattle;
         public BattleAction battleAction;
         public bool gettingRevived;
         public int[] multipliers;
         public List<Ability> instantiatedAbilities;
        
        public PokemonBattleInstance(Pokemon pokemon)
        {
            this.multipliers = new int[6];
            this.spotIndex = 0;
            this.prefab = null;
            this.spawnedObject = null;
            this.anim = null;
            this.gettingSwitched = false;
            this.inBattle = false;
            this.battleAction = null;
            this.gettingRevived = false;
            this.instantiatedAbilities = null;
        }
    }
}
