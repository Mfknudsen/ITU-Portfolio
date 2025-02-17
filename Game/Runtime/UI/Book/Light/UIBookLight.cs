#region Packages

using Runtime.Core;
using UnityEngine;

#endregion

namespace Runtime.UI.Book.Light
{
    public class UIBookLight : MonoBehaviour
    {
        #region Values

        [SerializeField] private int pixelSkip = 10;
        [SerializeField] private float levelToTurnOn;
        [SerializeField] private UnityEngine.Light bookLight;
        [SerializeField] private RenderTexture renderTexture;

        #endregion

        #region In

        public void TurnOff()
        {
            this.bookLight.enabled = false;
        }

        public void Calculate()
        {
            this.bookLight.enabled = true;
            
            Texture2D tex = this.renderTexture.RenderTextureToTexture2D();
            Color[] pixels = tex.GetPixels();

            float brightColor = 0;
            int count = 0;

            for (int i = 0; i < pixels.Length; i += this.pixelSkip)
            {
                count++;
                brightColor += pixels[i].grayscale;
            }

            this.bookLight.enabled = brightColor / count * 100f <= this.levelToTurnOn;
        }

        #endregion
    }
}