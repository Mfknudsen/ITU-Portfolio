#region Libraries

using Runtime.AI;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.BattleStart;
using Runtime.Battle.Systems.Spots;
using Runtime.Battle.Systems.States;
using Runtime.Communication;
using Runtime.Player;
using Runtime.Player.Camera;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using Runtime.Systems;
using Runtime.Systems.UI;
using Runtime.Testing;
using Runtime.UI.Battle;
using Runtime.UI.Battle.Information_Display;
using Runtime.UI.Battle.Selection;
using Runtime.UI.Communication;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using Logger = Runtime.Testing.Logger;

#endregion

namespace Runtime.Battle.Systems
{
    public class BattleSystem : MonoBehaviour
    {
        #region Values

        public static BattleSystem instance;

        [SerializeField, Required] private OperationManager operationManager;
        [SerializeField, Required] private ChatManager chatManager;
        [SerializeField, Required] private PlayerManager playerManager;
        [SerializeField, Required] private UIManager uiManager;

        [FoldoutGroup("Conditions")]
        [SerializeField]
        private Condition faintCondition;

        [FoldoutGroup("Actions")]
        [SerializeField]
        private BattleAction switchAction, itemAction;

        [FoldoutGroup("Actions")]
        [SerializeField]
        private float secondsPerPokemonMove = 1;

        [FoldoutGroup("Battlefield")]
        [SerializeField]
        private GameObject spotPrefab;

        [SerializeField, FoldoutGroup("UI"), Required]
        private BattleUI battleUI;

        [SerializeField, FoldoutGroup("UI"), Required]
        private SelectionMenu selectionMenu;

        [SerializeField, FoldoutGroup("UI"), Required]
        private InformationDisplay informationDisplay;

        [SerializeField, FoldoutGroup("Chat"), Required]
        private TextField textField;

        [FoldoutGroup("Chat")]
        [SerializeField]
        private Chat superEffective;

        [FoldoutGroup("Chat")]
        [SerializeField]
        private Chat notEffective, noEffect, barelyEffective, extremelyEffective, miss;

        [SerializeField, FoldoutGroup("Camera")]
        private CameraEvent cameraEvent;

        private BattleStarter starter;

        private WeatherManager weatherManager;

        private SpotOversight spotOversight;

        private AbilityOversight abilityOversight;

        private Coroutine stateManager;

        #endregion

        #region Build In States

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Start()
        {
            instance = this;

            BattleMathf.SetSuperEffective(this.superEffective);
            BattleMathf.SetNotEffective(this.notEffective);
            BattleMathf.SetNoEffect(this.noEffect);
            BattleMathf.SetBarelyEffective(this.barelyEffective);
            BattleMathf.SetExtremlyEffective(this.extremelyEffective);
            BattleMathf.SetMissChat(this.miss);
        }

        #endregion

        #region Getters

        public SelectionMenu GetSelectionMenu() =>
            this.selectionMenu;

        public float GetSecPerPokeMove() =>
            this.secondsPerPokemonMove;

        public BattleStarter GetStarter() =>
            this.starter;

        public WeatherManager GetWeatherManager() =>
            this.weatherManager ??= new WeatherManager();

        public SpotOversight GetSpotOversight() =>
            this.spotOversight ??= new SpotOversight();

        public AbilityOversight GetAbilityOversight() =>
            this.abilityOversight;

        public InformationDisplay GetDisplayManager() =>
            this.informationDisplay;

        #endregion

        #region In

        public void StartBattle(BattleStarter battleStarter)
        {
            this.starter = battleStarter;

            this.textField.MakeCurrent();

            this.SetState(new BeginState(
                this,
                this.operationManager,
                this.chatManager,
                this.uiManager,
                this.playerManager,
                battleStarter.GetBattleInitializer));

            this.operationManager.AddAsyncOperationsContainer(
                new OperationsContainer(this.cameraEvent));
        }

        public void SpawnPokemon(Pokemon pokemon, Spot spot)
        {
            Logger.AddLog(this.name, "Spawning: " + pokemon.GetName());

            Transform sTransform = spot.GetTransform();
            GameObject obj = pokemon.InstantiateUnitPrefab(PokemonState.Batte, sTransform.position, sTransform.rotation, sTransform).gameObject;

            pokemon.SetSpawnedObject(obj);
            pokemon.SetInBattle(true);
            pokemon.SetGettingSwitched(false);
            pokemon.SetRevived(false);

            spot.SetActivePokemon(pokemon);
            spot.SetNeedNew(false);

            this.informationDisplay.UpdateSlots(this.spotOversight);

            //Check if spawned object is placeholder;
            obj.GetComponent<PokemonPlaceholder>().SetText(pokemon.GetName());
        }

        public void DespawnPokemon(Pokemon pokemon)
        {
            if (!pokemon) return;

            foreach (Spot s in this.spotOversight.GetSpots()
                         // ReSharper disable once Unity.NoNullPropagation
                         .Select(s => new { s, p = s?.GetActivePokemon() })
                         .Where(t => t.p == pokemon)
                         .Select(t => t.s))
            {
                pokemon.DespawnPokemon();

                s.SetActivePokemon(null);

                break;
            }

            this.informationDisplay.UpdateSlots(this.spotOversight);
        }

        public void SetupAbilityOversight()
        {
            this.abilityOversight = new AbilityOversight();

            this.abilityOversight.Setup();
        }

        public void SetState(State set)
        {
            if (this.stateManager != null) this.StopCoroutine(this.stateManager);

            this.stateManager = this.StartCoroutine(set.Tick());
        }

        public void EndBattle(bool playerVictory)
        {
            Destroy(this.battleUI.gameObject);

            this.starter.EndBattle(playerVictory);
        }

        public void SetPokemonFainted(Pokemon pokemon)
        {
            FaintedCondition condition = Instantiate(this.faintCondition) as FaintedCondition;

            if (condition == null) return;

            condition.SetAffectedPokemon(pokemon);

            pokemon.GetConditionOversight().TryApplyNonVolatileCondition(condition);
        }

        #endregion

        #region Out

        public Spot CreateSpot() =>
            Instantiate(this.spotPrefab, this.transform).GetComponent<Spot>();

        public SwitchAction InstantiateSwitchAction() =>
            (SwitchAction)Instantiate(this.switchAction);

        public ItemAction InstantiateItemAction() =>
            Instantiate(this.itemAction) as ItemAction;

        public bool CheckTeamDefeated(bool isAlly) =>
            this.spotOversight.GetSpots()
                .Select(s => s.GetBattleMember())
                .Where(bm => bm.GetTeamAffiliation() == isAlly)
                .Any(bm => !bm.GetTeam().HasMorePokemon());

        #endregion
    }
}