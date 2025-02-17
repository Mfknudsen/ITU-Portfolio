#region Packages

using System;
using Runtime.Files;
using Sirenix.OdinInspector;

#endregion

namespace Runtime.Player
{
    [Serializable]
    public class CharacterSheet
    {
        public int badgeCount;

        [FoldoutGroup("Pronouns")]
        [HorizontalGroup("Pronouns/Horizontal")]
        [HideLabel]
        [BoxGroup("Pronouns/Horizontal/1", false)]
        public string pronoun1;

        [HideLabel] [BoxGroup("Pronouns/Horizontal/2", false)]
        public string pronoun2;

        [HideLabel] [BoxGroup("Pronouns/Horizontal/3", false)]
        public string pronoun3;

        public CharacterSheet(PlayerData data)
        {
            try
            {
                this.badgeCount = data.badgeCount;

                string[] pronouns = data.pronouns;
                this.pronoun1 = pronouns[0];
                this.pronoun2 = pronouns[1];
                this.pronoun3 = pronouns[2];
            }
            catch (Exception)
            {
                this.pronoun1 = "They";
                this.pronoun2 = "Them";
                this.pronoun3 = "Theirs";
            }
        }
    }
}