#region Libraries

using Runtime.AI;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Items;
using Runtime.Pokémon.Conditions;
using Runtime.Systems.Pooling;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Runtime.Pokémon
{
    #region Enums

    public enum Stat
    {
        // ReSharper disable once InconsistentNaming
        HP,
        Attack,
        Defence,
        SpAtk,
        SpDef,
        Speed,
        Accuracy,
        Evasion,
        Critical
    }

    public enum EvolutionMethod
    {
        None,
        Level,
        Item,
        Trade,
        LearnedMove
    }

    public enum EggGroup
    {
        Monster,
        Water1
    }

    public enum LevelRate
    {
        Slow,
        MediumSlow
    }

    public enum Shape
    {
    }

    public enum Footprint
    {
    }

    #endregion

    [CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon", order = 0)]
    public sealed class Pokemon : ScriptableObject
    {
        #region Values

        #region Pokemon

        [SerializeField] private bool isInstantiated;

        [SerializeField, BoxGroup("Basic")] private string pokemonName;

        [SerializeField, BoxGroup("Basic")] private int pokedexIndex;

        [SerializeField, BoxGroup("Basic")] private Type[] types = new Type[1];

        [SerializeField, BoxGroup("Basic"), TextArea]
        private string description;

        [SerializeField, HorizontalGroup("Basic/H1"), VerticalGroup("Basic/H1/V1")]
        private float height;

        [SerializeField, VerticalGroup("Basic/H1/V1")]
        private float weight;

        [SerializeField, VerticalGroup("Basic/H1/V2")]
        private Shape shape;

        [SerializeField, VerticalGroup("Basic/H1/V2")]
        private Footprint footprint;

        [BoxGroup("Basic")][SerializeField] private Color pokedexColor = Color.green;

        [SerializeField, LabelWidth(75), BoxGroup("Basic/H2", showLabel: false), AssetSelector(FlattenTreeView = true, Paths = "Assets/Prefabs"), Required]
        private PokemonUnit unitPrefab;

        [SerializeField, BoxGroup("Basic/H2"), LabelWidth(90)]
        private Ability firstAbility, secondAbility, hiddenAbility;

        [SerializeField, BoxGroup("Moves")] private string pokemonCategory;

        [SerializeField, BoxGroup("Stats")] private Stats stats;
        [SerializeField, BoxGroup("Stats")] private int[] iv = new int[6];
        [SerializeField, BoxGroup("Stats")] private int[] ev = new int[6];

        [SerializeField, BoxGroup("Evolution")]
        private EvolutionMethod[] method;

        [SerializeField, BoxGroup("Moves")] private MoveStats moveStats;

        [SerializeField, BoxGroup("Breeding")] private BreedingStats breedingStats;

        private int maxHealth;

        private int level, maxExp;
        private int currentExp;

        private float currentHealth;

        private ConditionOversight oversight;

        private IHoldableItem itemInHand;

        [SerializeField] private float genderRate, catchRate;

        [SerializeField] private int expYield;

        [SerializeField] private LevelRate levelRate;
        [SerializeField] private int[] evYield = new int[6];

        [SerializeField] private int baseFriendship;

        #endregion

        #region In Battle

        [Header("Battle:"), SerializeField] private int spotIndex;
        [SerializeField] private GameObject spawnedObject;
        [SerializeField] private Animator anim;
        [SerializeField] private bool gettingSwitched, inBattle;
        [SerializeField] private BattleAction battleAction;
        [SerializeField] private bool gettingRevived;
        [SerializeField] private int[] multipliers = new int[6];
        [SerializeField] private List<Ability> instantiatedAbilities;

        private int accuracy, evasion, critical;

        #endregion

        #endregion

        #region Getters

        #region Pokemon

        public int GetMaxHealth() =>
            this.maxHealth;

        public bool GetIsInstantiated() =>
            this.isInstantiated;

        public string GetName() =>
            this.pokemonName;

        public int GetStatRaw(Stat target) =>
            this.stats[target];

        public int GetIV(Stat target) =>
            this.iv[(int)target];

        public int GetEV(Stat target) =>
            this.ev[(int)target];

        public int GetLevel() =>
            this.level;

        public Type[] GetTypes()
        {
            if (this.types.Length == 1)
                return (Type[])this.types.Concat(new Type[] { null });

            return this.types;
        }

        public float GetCatchRate() =>
            this.catchRate;

        public Ability[] GetAbilities() =>
            new[] { this.firstAbility, this.secondAbility, this.hiddenAbility };

        #endregion

        #region - In Battle

        public int GetCalculatedStat(Stat stat)
        {
            int baseStat = this.stats[stat];

            int iv = this.iv[(int)stat];

            int ev = this.ev[(int)stat];

            float result = stat == Stat.HP
                ? BattleMathf.CalculateHPStat(
                    baseStat,
                    iv,
                    ev,
                    this.level)
                : BattleMathf.CalculateBaseStat(
                    baseStat,
                    iv,
                    ev,
                    this.level);

            result *= BattleMathf.GetMultiplierValue(this.multipliers[(int)stat],
                stat is not (Stat.Accuracy or Stat.Evasion));

            if (stat == Stat.HP) return Mathf.FloorToInt(result);

            result = BattleSystem.instance.GetAbilityOversight()
                .ListOfSpecific<IStatModifier>()
                .Aggregate(result, (current, statModifier) => current * statModifier.Modify(this, stat));

            return Mathf.FloorToInt(result);
        }

        public ConditionOversight GetConditionOversight() =>
            this.oversight ? this.oversight : this.oversight = CreateInstance<ConditionOversight>();

        public float GetCurrentHealth() => this.currentHealth;

        public IEnumerable<PokemonMove> GetMoves() => this.moveStats.learnedMoves;

        public PokemonMove GetMoveByIndex(int index)
        {
            if (this.moveStats.learnedMoves.Length <= 0 || index < 0 || index >= this.moveStats.learnedMoves.Length)
                return null;

            if (this.moveStats.learnedMoves[index] != null)
                return this.moveStats.learnedMoves[index].GetAction() as PokemonMove;

            return null;
        }

        public GameObject GetSpawnedObject() => this.spawnedObject;

        public BattleAction GetBattleAction() => this.battleAction;

        public bool GetGettingSwitched() => this.gettingSwitched;

        public bool GetInBattle() => this.inBattle;

        public bool GetRevived() => this.gettingRevived;

        public int GetAccuracy() => this.accuracy;

        public int GetEvasion() => this.evasion;

        public int GetCritical() => this.critical;

        #endregion

        #endregion

        #region Setters

        public void SetIsInstantiated(bool set) => this.isInstantiated = set;

        public void SetSpawnedObject(GameObject set) => this.spawnedObject = set;

        public void SetBattleAction(BattleAction set) => this.battleAction = set;

        public void SetGettingSwitched(bool set) => this.gettingSwitched = set;

        public void SetInBattle(bool set) => this.inBattle = set;

        public void SetRevived(bool set) => this.gettingRevived = set;

        public void SetFirstAbility(Ability set) => this.firstAbility = set;

        public void SetSecondAbility(Ability set) => this.secondAbility = set;

        public void SetHiddenAbility(Ability set) => this.hiddenAbility = set;

        #region Setting NonSerialized for Virtual Pokemon

        public void VirtualSetConditionOversight(ConditionOversight conditionOversight) =>
            this.oversight = conditionOversight;

        #endregion

        #endregion

        #region Out

        public bool IsSameType(TypeName typeName)
        {
            if (this.types[0].GetTypeName() == typeName)
                return true;

            if (this.types[1] == null) return false;

            return this.types[1].GetTypeName() == typeName;
        }

        public T[] GetAbilitiesOfType<T>() => new[] { this.firstAbility, this.secondAbility, this.hiddenAbility }.OfType<T>().ToArray();

        public PokemonUnit InstantiateUnitPrefab(PokemonState state, Vector3 position, Quaternion rotation, Transform parent = null, bool active = true)
        {
            PokemonUnit unit = Pool.CreateAtPositionAndRotation(this.unitPrefab, position, rotation, parent, active).GetComponent<PokemonUnit>();
            unit.SetState(state);
            return unit;
        }

        #endregion

        #region In

        public void Setup()
        {
            this.oversight ??= CreateInstance<ConditionOversight>();
            this.oversight.Setup(this);

            this.maxHealth = this.GetCalculatedStat(Stat.HP);
            this.currentHealth = this.maxHealth;

            AbilityOversight abilityOversight = BattleSystem.instance.GetAbilityOversight();

            this.instantiatedAbilities = new List<Ability>
                { this.firstAbility, this.secondAbility, this.hiddenAbility };

            for (int i = 0; i < this.instantiatedAbilities.Count; i++)
            {
                if (this.instantiatedAbilities[i] == null) continue;

                Ability ability = Instantiate(this.instantiatedAbilities[i]);

                ability.SetAffectedPokemon(this);
                abilityOversight.AddAbility(ability);

                this.instantiatedAbilities[i] = ability;
            }
        }

        // ReSharper disable once IdentifierTypo
        public void DespawnPokemon()
        {
            Destroy(this.spawnedObject);
            this.inBattle = false;
            this.gettingSwitched = false;
            this.spawnedObject = null;
            this.oversight.ResetConditionList();
        }

        public void ReceiveDamage(float damage) =>
            this.currentHealth = Mathf.Clamp(this.currentHealth - damage, 0, this.maxHealth);

        public void ReceiveExp(int points)
        {
            if (this.level == 100) return;

            int expNeeded = this.maxExp - this.currentExp;

            if (expNeeded < points)
            {
            }
            else
            {
                points -= expNeeded;
                this.LevelUp();
                this.currentExp = points;
            }
        }

        public void EffectMultiplierStage(int stages, Stat affectedStat)
        {
            if (affectedStat is Stat.Accuracy or Stat.Evasion or Stat.Critical)
                return;

            this.multipliers[(int)affectedStat] += stages;
        }

        public void ResetForAIMemory()
        {
            for (int i = 0; i < 6; i++)
            {
                this.iv[i] = 0;
                this.ev[i] = 0;
            }

            this.firstAbility = null;
            this.secondAbility = null;
            this.hiddenAbility = null;

            this.baseFriendship = 0;

            this.oversight = Instantiate(this.oversight);
            this.oversight.Setup(this);
        }

        #endregion

        #region Internal

        private void LevelUp() => this.level++;

        #endregion
    }
}