#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Battle.Evaluator;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Items;
using Runtime.Pokémon;
using Runtime.Settings;
using Runtime.Trainer;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Battle.Systems
{
    public class BattleMember : SerializedMonoBehaviour
    {
        #region Values

        [SerializeField] [BoxGroup(" ")] private string memberName;

        [SerializeField] [BoxGroup(" ")] private bool isAlly;

        [SerializeField] [BoxGroup(" ")] private int spotsToOwn = 1;

        [BoxGroup(" /Affiliation")]
        [HorizontalGroup(" /Affiliation/Team Affiliation")]
        [HideIf("isWild")]
        [LabelWidth(60)]
        [SerializeField]
        private bool isPlayer;

        [HorizontalGroup(" /Affiliation/Team Affiliation")] [HideIf("isPlayer")] [LabelWidth(60)] [SerializeField]
        private bool isWild;

        [BoxGroup(" ")] [TableList] [SerializeField]
        private List<EvaluatorSetting> evaluatorSettings = new List<EvaluatorSetting>();

        [BoxGroup(" /Personality")] [SerializeField]
        private bool useDefaultPersonalitySetting = true;

        [BoxGroup(" /Personality")] [HideIf("useDefaultPersonalitySetting")] [SerializeField]
        private PersonalitySetting personalitySetting;

        [FoldoutGroup(" /On Defeat")] [SerializeField]
        private Chat onDefeatedChats;

        private Inventory inventory;
        private Team pokemonTeam;

        private bool hasAllSpots;
        private readonly List<Spot> ownedSpots = new List<Spot>();

        private readonly List<Evaluator> evaluators = new List<Evaluator>();

        #endregion

        #region Build In States

        private void OnValidate()
        {
            if (this.pokemonTeam == null) this.pokemonTeam = this.GetComponent<Team>();
            if (this.inventory == null) this.inventory = this.GetComponent<Inventory>();
        }

        #endregion

        #region Getters

        public string GetName() =>
            this.memberName;

        public bool IsWild() =>
            this.isWild;

        public Team GetTeam() =>
            this.pokemonTeam;

        public bool GetTeamAffiliation() =>
            this.isAlly || this.isPlayer;

        public List<Spot> GetOwnedSpots() =>
            this.ownedSpots;

        public bool OwnSpot(Spot spot) =>
            this.ownedSpots.Contains(spot);

        public bool IsPlayer() =>
            this.isPlayer;

        public bool HasAllSpots() =>
            this.hasAllSpots;

        public Inventory GetInventory() =>
            this.inventory;

        public int GetSpotsToOwn() =>
            this.spotsToOwn;

        public Chat GetOnDefeatedChats() =>
            this.onDefeatedChats;

        #endregion

        #region Setters

        public void SetTeamNumber(bool set)
        {
            this.isAlly = set;
        }

        public void SetOwnedSpot(Spot set)
        {
            if (set != null)
            {
                if (!this.ownedSpots.Contains(set)) this.ownedSpots.Add(set);
            }

            this.hasAllSpots = (this.ownedSpots.Count == this.spotsToOwn);
        }

        #endregion

        #region In

        public void Setup()
        {
            if (this.isPlayer)
                return;

            for (int i = 0; i < 6; i++)
            {
                Pokemon pokemon = this.pokemonTeam.GetPokemonByIndex(i);
                if (pokemon == null)
                    break;
                EvaluatorSetting evaluatorSetting;
                // ReSharper disable once LocalVariableHidesMember
                PersonalitySetting personalitySetting = this.useDefaultPersonalitySetting
                    ? new PersonalitySetting()
                    : this.personalitySetting;

                try
                {
                    evaluatorSetting = this.evaluatorSettings[i];
                }
                catch
                {
                    evaluatorSetting = GameplaySetting.GetDefaultEvaluatorSetting(Setting.Difficultly);
                }

                evaluatorSetting.SetPersonalitySetting(personalitySetting);

                Evaluator evaluator = new Evaluator(pokemon, evaluatorSetting);
                this.evaluators.Add(evaluator);
            }
        }

        public void ForceHasAllSpots()
        {
            this.hasAllSpots = true;
        }

        public void ActivateAIBrain(Pokemon toTick)
        {
            Evaluator evaluator = this.evaluators.FirstOrDefault(e => e.UsedForPokemon(toTick));

            if (evaluator == null)
            {
                Debug.LogWarning("Failed to Create Evaluation");
                evaluator = new Evaluator(toTick, GameplaySetting.GetDefaultEvaluatorSetting(Setting.Difficultly));
            }

            evaluator.EvaluateForPokemon();
        }

        #endregion
    }
}