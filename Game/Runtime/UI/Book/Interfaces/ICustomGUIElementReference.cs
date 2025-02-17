#region Packages

using Runtime.UI_Book;

#endregion

namespace Runtime.UI.Book.Interfaces
{
    public interface ICustomGUIElementReference
    {
        public void Setup(UIBook uiBook, ICustomGUIElement element);
    }
}
