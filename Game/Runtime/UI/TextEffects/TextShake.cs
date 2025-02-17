using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.UI.TextEffects
{
    /// <summary>
    /// Must be manually stopped
    /// </summary>
    public sealed class TextShake : TextEffect
    {
        #region Values

        private readonly float shakeOffsetAmount, shakeTime;
        private readonly bool perCharacter;
        private readonly int startCharacterIndex, endCharacterIndex;

        private List<float> randomOffset, originalPosition;

        #endregion

        #region Build In States

        public TextShake(TextEffectBase effectBase, float shakeOffsetAmount, float shakeTime,
            bool perCharacter, int startCharacterIndex,
            int endCharacterIndex) :
            base(effectBase)
        {
            this.shakeOffsetAmount = shakeOffsetAmount;
            this.shakeTime = shakeTime;
            this.perCharacter = perCharacter;
            this.startCharacterIndex = startCharacterIndex;
            this.endCharacterIndex = endCharacterIndex;

            this.randomOffset = new List<float>(this.effectBase.GetText().textInfo.characterCount);
            this.originalPosition = new List<float>(this.effectBase.GetText().textInfo.characterCount);
            for (int i = 0; i < effectBase.GetText().textInfo.characterCount; i++)
            {
                this.randomOffset[i] = Random.Range(0, shakeTime);
                this.originalPosition[i] = this.effectBase.GetText().textInfo.meshInfo[i]
                    .vertices[this.effectBase.GetText().textInfo.characterInfo[i].vertexIndex].y;
            }
        }

        public TextShake(TextEffectBase effectBase, float shakeOffsetAmount, float shakeTime,
            bool perCharacter, int startCharacterIndex,
            int endCharacterIndex, float originalPosition) :
            base(effectBase)
        {
            this.shakeOffsetAmount = shakeOffsetAmount;
            this.shakeTime = shakeTime;
            this.perCharacter = perCharacter;
            this.startCharacterIndex = startCharacterIndex;
            this.endCharacterIndex = endCharacterIndex;

            this.randomOffset = new List<float>(this.effectBase.GetText().textInfo.characterCount);
            this.originalPosition = new List<float>(this.effectBase.GetText().textInfo.characterCount);
            for (int i = 0; i < effectBase.GetText().textInfo.characterCount; i++)
            {
                this.randomOffset[i] = Random.Range(0, shakeTime);
                this.originalPosition[i] = originalPosition;
            }
        }

        #endregion

        #region Internal

        protected override IEnumerator Effect()
        {
            float time = 0;
            while (true)
            {
                time += Time.deltaTime;
                int count = 0;
                for (int i = this.startCharacterIndex; i < this.endCharacterIndex + 1; i++)
                {
                    if (this.perCharacter)
                        count++;
                }

                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        #endregion
    }
}