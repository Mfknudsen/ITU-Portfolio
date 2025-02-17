#region Libraries

using System.Collections;
using TMPro;
using UnityEngine;

#endregion

namespace Runtime.UI.TextEffects
{
    public sealed class TextFade : TextEffect
    {
        #region Values

        public enum FadeType
        {
            All,
            LeftToRight,
            RightToLeft,
            TopToButton,
            ButtonToTop
        }

        private float highestX, highestY;
        private float lowestX, lowestY;

        private float diffX, diffY;

        private FadeType fadeType;

        private readonly float totalTime, fadeTime;

        private readonly TMP_TextInfo textInfo;

        #endregion

        #region Build In States

        public TextFade(TextEffectBase effectBase, float totalTime, float fadeTime,
            FadeType fadeType) : base(effectBase)
        {
            this.textInfo = effectBase.GetText().textInfo;

            this.fadeType = fadeType;
            this.totalTime = totalTime;
            this.fadeTime = fadeTime;

            this.highestX = 0;
            this.highestY = 0;
            this.lowestX = 0;
            this.lowestY = 0;
            this.diffX = 0;
            this.diffY = 0;

            this.SetLowsAndHighs();
        }

        #endregion

        #region Getters

        public float LowestX() => this.lowestX;

        public float LowestY() => this.lowestY;

        public float HighestX() => this.highestX;

        public float HighestY() => this.highestY;

        #endregion

        #region Setters

        public void SetFadeType(FadeType set)
        {
            this.fadeType = set;
            this.SetLowsAndHighs();
        }

        #endregion

        #region Internal

        protected override IEnumerator Effect()
        {
            yield break;
        }

        private void SetLowsAndHighs()
        {
            switch (this.fadeType)
            {
                case FadeType.All:
                    return;

                case FadeType.LeftToRight or FadeType.RightToLeft:
                {
                    this.lowestX = this.textInfo.meshInfo[this.textInfo.characterInfo[0].materialReferenceIndex]
                        .vertices[0].x;
                    this.highestX = this.textInfo.meshInfo[this.textInfo.characterInfo[1].materialReferenceIndex]
                        .vertices[4].x;

                    for (int i = 1; i < this.textInfo.characterCount; i++)
                    {
                        TMP_CharacterInfo charInfo = this.textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                            continue;
                        float newLow = this.textInfo.meshInfo[charInfo.materialReferenceIndex]
                            .vertices[charInfo.vertexIndex].x;

                        if (this.lowestX > newLow) this.lowestX = newLow;
                    }

                    for (int i = 1; i < this.textInfo.characterCount; i++)
                    {
                        TMP_CharacterInfo charInfo = this.textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                            continue;
                        float newHigh = this.textInfo.meshInfo[charInfo.materialReferenceIndex]
                            .vertices[charInfo.vertexIndex].x;

                        if (this.highestX < newHigh) this.highestX = newHigh;
                    }

                    this.diffX = Mathf.Abs(this.highestX - this.lowestX);

                    break;
                }

                default:
                {
                    this.lowestY = this.textInfo.meshInfo[this.textInfo.characterInfo[0].materialReferenceIndex]
                        .vertices[0].y;
                    this.highestY = this.textInfo.meshInfo[this.textInfo.characterInfo[1].materialReferenceIndex]
                        .vertices[4].y;

                    for (int i = 1; i < this.textInfo.characterCount; i++)
                    {
                        TMP_CharacterInfo charInfo = this.textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                            continue;
                        float newLow = this.textInfo.meshInfo[charInfo.materialReferenceIndex]
                            .vertices[charInfo.vertexIndex].y;

                        if (this.lowestY > newLow) this.lowestY = newLow;
                    }

                    for (int i = 1; i < this.textInfo.characterCount; i++)
                    {
                        TMP_CharacterInfo charInfo = this.textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                            continue;
                        float newHigh = this.textInfo.meshInfo[charInfo.materialReferenceIndex]
                            .vertices[charInfo.vertexIndex].y;

                        if (this.highestY < newHigh) this.highestY = newHigh;
                    }

                    this.diffY = Mathf.Abs(this.highestY - this.lowestY);

                    break;
                }
            }
        }

        #endregion

        public override void Complete()
        {
        }
    }
}