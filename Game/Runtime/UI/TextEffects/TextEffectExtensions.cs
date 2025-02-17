#region Packages

using TMPro;
using UnityEngine.Events;

#endregion

// ReSharper disable ObjectCreationAsStatement
namespace Runtime.UI.TextEffects
{
    public static class TextEffectExtensions
    {
        #region Out

        public static TextEffectBase ShowOverTime(this TextMeshProUGUI text, float timeBetweenCharacters)
        {
            TextEffectBase result = new TextEffectBase(text);
            new TextOverTime(result, timeBetweenCharacters);
            return result;
        }

        public static TextEffectBase ShowOverTime(this TextMeshProUGUI text, float timeBetweenCharacters,
            UnityAction onCompleteAction)
        {
            TextEffectBase result = new TextEffectBase(text);
            new TextOverTime(result, timeBetweenCharacters, onCompleteAction);
            return result;
        }

        public static TextEffectBase ShowOverTime(TextEffectBase effectBase, float timeBetweenCharacters)
        {
            new TextOverTime(effectBase, timeBetweenCharacters);
            return effectBase;
        }

        public static TextEffectBase ShowOverTime(TextEffectBase effectBase, float timeBetweenCharacters,
            UnityAction onCompleteAction)
        {
            new TextOverTime(effectBase, timeBetweenCharacters, onCompleteAction);
            return effectBase;
        }

        #endregion
    }
}