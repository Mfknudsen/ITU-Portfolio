#region SDK

using Runtime.Battle.Actions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

// ReSharper disable ParameterHidesMember
namespace Runtime.UI.Battle.Selection
{
    public class TurnMoveSlot : MonoBehaviour
    {
        [SerializeField] private TurnSelection turnSelection;
        [SerializeField] private TextMeshProUGUI gui;
        [SerializeField] private Image background;

        private PokemonMove move;

        public void SetMove(PokemonMove move)
        {
            this.move = move;

            this.background.color = Color.white;
            this.gui.text = "";

            if (move is null) return;

            this.gui.text = move.GetName();

            this.background.color = move.GetMoveType().GetTypeColor();
        }

        public void Trigger()
        {
            this.turnSelection.ReceiveAction(this.move);
        }
    }
}