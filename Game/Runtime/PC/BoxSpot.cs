#region SDK

using Runtime.Pokémon;
using UnityEngine;
using UnityEngine.UI;

//Custom

#endregion

namespace Runtime.PC
{
    public class BoxSpot : MonoBehaviour
    {
        private Pokemon onSpot;
        private Image visual = null;

        public void SetPokemon(Pokemon set)
        {
            this.onSpot = set;

            this.visual.sprite = null;
        }
    }
}