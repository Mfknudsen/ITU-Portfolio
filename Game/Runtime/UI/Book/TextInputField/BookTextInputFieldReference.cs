using Runtime.UI_Book;
using Runtime.UI.Book.Interfaces;
using UnityEngine;

namespace Runtime.UI.Book.TextInputField
{
    public class BookTextInputFieldReference : MonoBehaviour, ICustomGUIElementReference
    {
        private UIBook uiBook;
        private BookTextInputField bookField;

        public void Setup(UIBook uiBook, ICustomGUIElement element)
        {
            this.uiBook = uiBook;
            if (element is BookTextInputField field) this.bookField = field;
        }
    }
}