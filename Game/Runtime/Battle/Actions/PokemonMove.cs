#region Packages

using System.Collections;
using Runtime.AI.Battle.Evaluator;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Battle.Systems.Static_Operations;
using Runtime.Communication;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using Runtime.Systems;
using UnityEngine;

#endregion

// ReSharper disable InconsistentNaming
namespace Runtime.Battle.Actions
{
    #region Enums

    public enum Category
    {
        Physical,
        Special,
        Status,
        Extents
    }

    public enum Contest
    {
        Tough,
        Cute,
        Clever,
        Beautiful,
        Cool,
    }

    public enum HitType
    {
        One,
        AllAdjacentOneSide,
        AllAdjacent,
        AllOneSide,
        AllOneSideExceptUser,
        AllExceptUser,
        All,
    }

    public enum SpecialAddons
    {
        Sound,
        Bite
    }

    #endregion

    [CreateAssetMenu(fileName = "New Basic Pokemon Move", menuName = "Pokemon/Create new Basic Move", order = 1)]
    public class PokemonMove : BattleAction
    {
        #region Values

        [Header("Pokemon Move:")] [SerializeField]
        protected int index;

        [SerializeField] protected string moveName = "";
        [SerializeField] protected Type type;
        [SerializeField] protected Category category = 0;
        [SerializeField, Min(0)] protected int startPP, maxPP;
        private int currentPP;
        [SerializeField] protected int power, accuracy;

        // ReSharper disable once IdentifierTypo
        [SerializeField] private bool canCrit = true;

        [Header("Interaction:")] [SerializeField]
        protected bool makesContact;

        [SerializeField] protected bool affectByProtect,
            affectByMagicCoat,
            affectBySnatch,
            affectByMirrorMove,
            affectByKingsRock;

        [SerializeField] protected SpecialAddons[] specialAddons;

        [Header("Target:")] [SerializeField] protected HitType hitType = 0;

        // ReSharper disable once IdentifierTypo
        [SerializeField] protected bool selfTargetable;

        // ReSharper disable once IdentifierTypo
        [SerializeField] protected bool[] enemyTargetable = new bool[3];

        // ReSharper disable once IdentifierTypo
        [SerializeField] protected bool[] allyTargetable = new bool[2];

        [Header("Contests:")]
        //Standard
        [SerializeField]
        protected Contest normalCondition = 0;

        [SerializeField] protected int normalAppeal, normalJam;

        //Super
        [SerializeField] protected Contest superCondition = 0;

        [SerializeField] protected int superAppeal, superJam;

        //Spectacular
        [SerializeField] protected Contest spectacularCondition = 0;
        [SerializeField] protected int spectacularAppeal, spectacularJam;

        [Header("Status:")] [SerializeField] protected bool hasStatus;
        [SerializeField] protected Condition statusCondition;
        [SerializeField] protected int applyChance;
        [SerializeField] private Chat statusHitChat;

        private void OnValidate() =>
            this.currentPP = this.startPP;

        #endregion

        #region Setters

        public void SetType(Type set) =>
            this.type = set;

        public void SetCategory(Category set) =>
            this.category = set;

        // ReSharper disable once InconsistentNaming
        public void SetStartPP(int set) =>
            this.startPP = set;

        public void SetPower(int set) =>
            this.power = set;

        public void SetAccuracy(int set) =>
            this.accuracy = set;

        // ReSharper disable once IdentifierTypo
        public void SetTargetable(bool[] set)
        {
            this.enemyTargetable[0] = set[0];
            this.enemyTargetable[1] = set[1];
            this.enemyTargetable[2] = set[2];

            this.allyTargetable[0] = set[3];
            this.allyTargetable[1] = set[4];

            this.selfTargetable = set[5];
        }

        public void SetAffected(bool[] set)
        {
            this.makesContact = set[0];
            this.affectByProtect = set[1];
            this.affectByMagicCoat = set[2];
            this.affectBySnatch = set[3];
            this.affectByMirrorMove = set[4];
            this.affectByKingsRock = set[5];
        }

        public void SetNormalContests(int[] set)
        {
            this.normalCondition = (Contest)set[0];
            this.normalAppeal = set[1];
            this.normalJam = set[2];
        }

        public void SetSuperContests(int[] set)
        {
            this.superCondition = (Contest)set[0];
            this.superAppeal = set[1];
            this.superJam = set[2];
        }

        public void SetSpectacularContests(int[] set)
        {
            this.spectacularCondition = (Contest)set[0];
            this.spectacularAppeal = set[1];
            this.spectacularJam = set[2];
        }

        public void SetHitType(HitType set) =>
            this.hitType = set;

        public void SetHasStatus(bool set) =>
            this.hasStatus = set;

        public void SetStatusCondition(Condition set) =>
            this.statusCondition = set;

        public void SetApplyChance(int set) =>
            this.applyChance = set;

        // ReSharper disable once InconsistentNaming
        public void SetMaxPP(int set) =>
            this.maxPP = set;

        public void SetSpecialAddons(SpecialAddons[] set) =>
            this.specialAddons = set;

        #endregion

        #region Getters

        public override BattleAction GetAction()
        {
            BattleAction result = this;

            if (this.GetIsInstantiated()) return result;

            result = Instantiate(this);
            result.SetIsInstantiated(true);

            return result;
        }

        public string GetName() =>
            this.moveName;

        public bool GetActive() =>
            this.active;

        public Type GetMoveType() =>
            this.type;

        public Category GetCategory() =>
            this.category;

        // ReSharper disable once InconsistentNaming
        public int GetStartPP() =>
            this.startPP;

        public int GetPower() =>
            this.power;

        public int GetAccuracy() =>
            this.accuracy;

        // ReSharper disable once IdentifierTypo
        public bool[] GetTargetable()
        {
            bool[] result = new bool[6];

            result[0] = this.enemyTargetable[0];
            result[1] = this.enemyTargetable[1];
            result[2] = this.enemyTargetable[2];

            result[3] = this.allyTargetable[0];
            result[4] = this.allyTargetable[1];

            result[5] = this.selfTargetable;

            return result;
        }

        public bool[] GetAffected()
        {
            bool[] result = new bool[6];

            result[0] = this.makesContact;
            result[1] = this.affectByProtect;
            result[2] = this.affectByMagicCoat;
            result[3] = this.affectBySnatch;
            result[4] = this.affectByMirrorMove;
            result[5] = this.affectByKingsRock;

            return result;
        }

        public int[] GetNormalContests() =>
            new[] { (int)this.normalCondition, this.normalAppeal, this.normalJam };

        public int[] GetSuperContests() =>
            new[] { (int)this.superCondition, this.superAppeal, this.superJam };

        public int[] GetSpectacularContests() =>
            new[] { (int)this.spectacularCondition, this.spectacularAppeal, this.spectacularJam };

        public HitType GetHitType() =>
            this.hitType;

        public bool GetHasStatus() =>
            this.hasStatus;

        public Condition GetStatusCondition() =>
            this.statusCondition;

        public int GetApplyChance() =>
            this.applyChance;

        // ReSharper disable once InconsistentNaming
        public int GetMaxPP() =>
            this.maxPP;

        public SpecialAddons[] GetSpecialAddons() =>
            this.specialAddons;

        #endregion

        #region Out

        public override float Evaluate(Pokemon user, Pokemon target, VirtualBattle virtualBattle,
            PersonalitySetting personalitySetting)
        {
            return VirtualMathf.CalculateVirtualDamage(this, user, target, virtualBattle) *
                   personalitySetting.aggressionLevel;
        }

        public override IEnumerator Operation()
        {
            this.done = false;

            float secPerPokeMove = BattleSystem.instance.GetSecPerPokeMove();

            foreach (Spot target in this.targets)
            {
                Pokemon pokemon = target.GetActivePokemon();
                OperationsContainer container = new OperationsContainer();

                #region Calculate Hit

                float accuracyUser = BattleMathf.CalculateOtherStat(
                    Stat.Accuracy, this.currentPokemon.GetAccuracy());
                float evasionTarget = BattleMathf.CalculateOtherStat(
                    Stat.Evasion, this.currentPokemon.GetEvasion());

                bool hit = BattleMathf.CalculateHit(this.accuracy, accuracyUser, evasionTarget);

                if (!hit)
                {
                    MissHit missHit = new MissHit(pokemon, this.chatManager);
                    container.Add(missHit);
                    this.operationManager.AddOperationsContainer(container);

                    this.done = true;
                    yield return 1;
                    yield break;
                }

                #endregion

                foreach (Chat chat in this.chatOnActivation)
                {
                    Chat instance = chat.GetChatInstantiated();
                    instance.AddToOverride("<POKEMON_NAME>", this.currentPokemon.GetName());
                    instance.AddToOverride("<POKEMON_MOVE>", this.moveName);
                    this.chatManager.Add(instance);
                }

                if (this.category != Category.Status)
                {
                    bool isCritical = this.canCrit && BattleMathf.CalculateCriticalRoll(this.currentPokemon, pokemon);
                    float damagePerTarget = this.GetDamageForTarget(this.currentPokemon, pokemon, isCritical);

                    DamagePokemon damagePokemon = new DamagePokemon(pokemon, damagePerTarget, secPerPokeMove);
                    container.Add(damagePokemon);
                }
                else if (this.statusCondition != null)
                {
                    if (!BattleMathf.CalculateStatusHit(this.applyChance)) continue;

                    ApplyStatus applyStatus = new ApplyStatus(this.statusHitChat, pokemon, this.statusCondition);
                    container.Add(applyStatus);
                }

                this.operationManager.AddOperationsContainer(container);
            }

            this.currentPokemon.SetBattleAction(null);
            this.done = true;
        }

        #endregion

        #region Internal

        protected override Chat[] TransferInformationToChat()
        {
            Chat[] result = new Chat[this.chatOnActivation.Length];

            for (int i = 0; i < this.chatOnActivation.Length; i++)
            {
                if (this.chatOnActivation[i] == null) continue;

                result[i] = this.chatOnActivation[i].GetChatInstantiated();

                result[i].AddToOverride("<POKEMON_NAME>", this.currentPokemon.GetName());
                result[i].AddToOverride("<POKEMON_MOVE>", this.moveName);
            }

            return result;
        }

        private float GetDamageForTarget(Pokemon user, Pokemon target, bool isCritical)
        {
            float attackPower =
                this.currentPokemon.GetCalculatedStat(this.category == Category.Physical ? Stat.Attack : Stat.SpAtk);

            float defencePower =
                this.currentPokemon.GetCalculatedStat(this.category == Category.Physical ? Stat.Defence : Stat.SpDef);

            return BattleMathf.CalculateDamage(user.GetLevel(),
                attackPower,
                defencePower, this.power,
                BattleMathf.CalculateModifiers(
                    user,
                    target,
                    this, this.targets.Count == 1,
                    isCritical));
        }

        #endregion
    }
}