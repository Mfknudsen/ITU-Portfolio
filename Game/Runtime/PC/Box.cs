#region Packages

using System.Collections.Generic;
using Runtime.Files;
using Runtime.Pokémon;
using Runtime.World.Overworld.Interactions;
using UnityEngine;

#endregion

namespace Runtime.PC
{
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Overworld/Interactions")]
    public class Box : MonoBehaviour, IInteractable
    {
        #region Values

        private static List<Pokemon> pokemonsInBox = new List<Pokemon>();
        private string fileName = "PokemonBox";

        #endregion

        #region In

        #region Load/Save

        public void Load()
        {
            pokemonsInBox.Clear();
            pokemonsInBox.AddRange(FileManager.LoadData<Pokemon[]>(this.fileName));
        }

        public void Save()
        {
            FileManager.SaveData(this.fileName, pokemonsInBox);
        }

        #endregion

        public void ShowNextBox()
        {
        }

        public void ShowPreviousBox()
        {
        }

        #endregion

        public void InteractTrigger()
        {
            Debug.Log("Trigger Box");
            //UIManager.instance.SwitchUI(UISelection.Box);
        }
    }
}