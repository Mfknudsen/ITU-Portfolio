#region Libraries

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Runtime.UI.TextEffects
{
    public sealed class TextOverTime : TextEffect
    {
        #region Values

        private readonly float timeBetweenCharacters;
        private readonly TMP_TextInfo textInfo;
        
        #endregion

        #region Build In States

        public TextOverTime(TextEffectBase effectBase, float timeBetweenCharacters) : base(effectBase)
        {
            this.timeBetweenCharacters = timeBetweenCharacters;
            this.textInfo = effectBase.GetText().textInfo;
        }

        public TextOverTime(TextEffectBase effectBase, float timeBetweenCharacters, UnityAction onCompleteAction) : base(effectBase, onCompleteAction)
        {
            this.timeBetweenCharacters = timeBetweenCharacters;
            this.textInfo = effectBase.GetText().textInfo;
        }

        #endregion

        #region In

        public override void Complete()
        {
            for (int i = 0; i < this.textInfo.characterCount - 1; i++)
            {
                TMP_CharacterInfo characterInfo = this.textInfo.characterInfo[i];
                TMP_MeshInfo meshInfo = this.textInfo.meshInfo[characterInfo.materialReferenceIndex];

                for (int j = 0; j < 4; j++)
                {
                    Color32 c = meshInfo.colors32[characterInfo.vertexIndex + i];
                    meshInfo.colors32[characterInfo.vertexIndex + i] = new Color32(c.a, c.g, c.b, 1);
                }
            }

            base.Complete();
        }

        #endregion

        #region Internal

        protected override IEnumerator Effect()
        {
            for (int i = 0; i < this.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo characterInfo = this.textInfo.characterInfo[i];
                TMP_MeshInfo meshInfo = this.textInfo.meshInfo[characterInfo.materialReferenceIndex];

                for (int j = 0; j < 4; j++)
                {
                    Color32 c = meshInfo.colors32[characterInfo.vertexIndex + i];
                    meshInfo.colors32[characterInfo.vertexIndex + i] = new Color32(c.a, c.g, c.b, 0);
                }
            }

            for (int i = 0; i < this.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo characterInfo = this.textInfo.characterInfo[i];
                TMP_MeshInfo meshInfo = this.textInfo.meshInfo[characterInfo.materialReferenceIndex];

                for (int j = 0; j < 4; j++)
                {
                    Color32 c = meshInfo.colors32[characterInfo.vertexIndex + i];
                    meshInfo.colors32[characterInfo.vertexIndex + i] = new Color32(c.a, c.g, c.b, 1);
                }

                yield return new WaitForSeconds(this.timeBetweenCharacters);
            }

            this.Complete();
        }

        #endregion
    }
}