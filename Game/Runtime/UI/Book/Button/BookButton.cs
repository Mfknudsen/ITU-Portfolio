#region Packages

using System.Linq;
using Runtime.UI_Book;
using Runtime.UI.Book.Interfaces;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Runtime.UI.Book.Button
{
    [AddComponentMenu("Mfknudsen/BookUI/Book Button")]
    public class BookButton : UnityEngine.UI.Button, ICustomGUIElement
    {
        #region Values

        [SerializeField, HideInInspector] private BookTurn bookTurn;

        private static readonly SelectionState[] States =
        {
            SelectionState.Normal,
            SelectionState.Disabled,
            SelectionState.Highlighted,
            SelectionState.Pressed,
            SelectionState.Selected
        };

        #endregion

        protected override void Awake()
        {
            base.Awake();

            Navigation nav = this.navigation;
            nav.mode = Navigation.Mode.None;
            this.navigation = nav;
        }

        #region Getters

        public BookTurn GetBookTurn()
        {
            return this.bookTurn;
        }

        #endregion

        #region Setters

        public void SetBookTurn(BookTurn set)
        {
            this.bookTurn = set;
        }

        #endregion

        #region In

        public void SetPressedState(string buttonState)
        {
            SelectionState state =
                States.FirstOrDefault(selectionState => selectionState.ToString().Equals(buttonState));

            base.DoStateTransition(state, false);

            if (state == SelectionState.Pressed) this.Invoke(nameof(this.TriggerOnClick), 0.1f);
        }

        #endregion

        #region Internal

        private void TriggerOnClick() => 
            this.onClick.Invoke();

        #endregion
    }
}