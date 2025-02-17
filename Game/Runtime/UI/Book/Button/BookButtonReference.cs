#region Packages

using Runtime.UI_Book;
using Runtime.UI.Book.Interfaces;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Runtime.UI.Book.Button
{
    public class BookButtonReference : UnityEngine.UI.Button, ICustomGUIElementReference
    {
        #region Values

        private UIBook uiBook;
        private BookButton bookButton;
        private readonly Color color = new Color(0, 0, 0, 0);

        #endregion

        protected override void Awake()
        {
            Navigation nav = this.navigation;
            nav.mode = Navigation.Mode.None;
            this.navigation = nav;

            ColorBlock colorBlock = this.colors;

            colorBlock.normalColor = this.color;
            colorBlock.highlightedColor = this.color;
            colorBlock.pressedColor = this.color;
            colorBlock.disabledColor = this.color;
            colorBlock.selectedColor = this.color;

            this.colors = colorBlock;

            base.Awake();
        }

        #region In

        public void Setup(UIBook uiBook, ICustomGUIElement element)
        {
            this.uiBook = uiBook;

            if (element is BookButton button) this.bookButton = button;
        }

        #endregion

        #region Internal

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (this.bookButton == null) return;

            if (state == SelectionState.Pressed)
                this.uiBook.Effect(this.bookButton.GetBookTurn());

            this.bookButton.SetPressedState(state.ToString());
        }

        #endregion
    }
}