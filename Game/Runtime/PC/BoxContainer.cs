#region SDK

using System.Collections.Generic;
using Runtime.Pokémon;
using TMPro;
using UnityEngine;

//Custom

#endregion

namespace Runtime.PC
{
    public class BoxContainer : MonoBehaviour
    {
        #region Values
        [SerializeField] private string boxName = "";
        [SerializeField] private Pokemon[] list = new Pokemon[0];

        //GUI
        [SerializeField]
        private TextMeshProUGUI numberGUI;
        private List<BoxSpot> spots = new List<BoxSpot>();

        private void OnValidate()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                BoxSpot spot = this.transform.GetChild(i).GetComponent<BoxSpot>();

                if (spot != null) this.spots.Add(spot);
            }
        }
        #endregion

        public void Setup(int number, Pokemon[] list)
        {
            this.numberGUI.text = this.boxName;
            for (int i = 0; i < this.spots.Count; i++)
            {
                this.spots[i].SetPokemon(list[i]);
            }
        }
    }
}
