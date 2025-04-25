#region Packages

using Runtime.UI_Book;
using Runtime.UI.Book.Button;
using UnityEditor;

#endregion

namespace Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BookButton))]
    public class EditorBookButton : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BookButton bookButton = this.target as BookButton;

            if (bookButton != null)
            {
                BookTurn turn = (BookTurn)EditorGUILayout.EnumPopup("Book Turn Enums", bookButton.GetBookTurn());
                if (turn != bookButton.GetBookTurn())
                {
                    bookButton.SetBookTurn(turn);
                    EditorUtility.SetDirty(bookButton);
                }

                EditorGUILayout.Space(10);
            }

            base.OnInspectorGUI();
        }
    }
#endif
}