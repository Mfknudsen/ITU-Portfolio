#region Libraries

using System;
using System.Collections;
using Runtime.Battle.Systems;
using Runtime.Core;
using Runtime.Files;
using Runtime.ScriptableVariables.Events;
using Runtime.ScriptableVariables.Objects.Cinemachine;
using Runtime.Systems;
using Runtime.Systems.PersistantRunner;
using Runtime.Trainer;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

#endregion

namespace Runtime.Player
{
    [CreateAssetMenu(menuName = "Manager/Player")]
    public sealed class PlayerManager : Manager, IFrameStart
    {
        #region Values

        [ShowInInspector] [ReadOnly] [NonSerialized]
        private PlayerState playerState = PlayerState.Paused;

        [SerializeField] [FoldoutGroup("References")]
        private Team team;

        [SerializeField] [FoldoutGroup("References")]
        private Controller controller;

        [SerializeField] [FoldoutGroup("References")]
        private NavMeshAgent agent;

        [SerializeField] [FoldoutGroup("References")]
        private BattleMember battleMember;

        [SerializeField] [FoldoutGroup("References")]
        private PlayerInteraction playerInteraction;

        [SerializeField] [FoldoutGroup("References")]
        private CinemachineFreeLook overworldCameraRig;

        [SerializeField] [FoldoutGroup("Character Sheet")] [HideLabel]
        private CharacterSheet characterSheet;

        [SerializeField] [BoxGroup("Variables/Camera")] [Required]
        private CinemachineBrainVariable cameraBrain;

        [SerializeField] [BoxGroup("Events")] [Required]
        private PlayerStateEvent playerStateEvent;

        private GameObject playerGameObject;

        private const string FileName = "PlayerData";

        #endregion

        #region Build In State

        public IEnumerator FrameStart(PersistantRunner runner)
        {
            this.playerGameObject = GameObject.FindWithTag("Player");

            DontDestroyOnLoad(this.playerGameObject);

            GameObject overworld = this.playerGameObject.GetChildByName("Overworld");

            this.overworldCameraRig = overworld.GetFirstComponentByRoot<CinemachineFreeLook>();
            this.agent = overworld.GetComponent<NavMeshAgent>();
            this.team = this.playerGameObject.GetComponent<Team>();
            this.controller = overworld.GetComponent<Controller>();
            this.playerInteraction = overworld.GetComponent<PlayerInteraction>();
            this.cameraBrain.Value = this.playerGameObject.GetComponentInChildren<CinemachineBrain>();

            this.characterSheet = new CharacterSheet(FileManager.LoadData<PlayerData>(FileName));
            this.overworldCameraRig.enabled = false;

            this.battleMember = this.playerGameObject.GetFirstComponentByRoot<BattleMember>();

            this.ready = true;

            yield break;
        }

        #endregion

        #region Getters

        public CharacterSheet GetCharacterSheet()
        {
            return this.characterSheet;
        }

        public string[] GetPronouns()
        {
            return new[]
                { this.characterSheet.pronoun1, this.characterSheet.pronoun2, this.characterSheet.pronoun3 };
        }

        public Team GetTeam()
        {
            return this.team;
        }

        public BattleMember GetBattleMember()
        {
            return this.battleMember;
        }

        public PlayerInteraction GetInteractions()
        {
            return this.playerInteraction;
        }

        public NavMeshAgent GetAgent()
        {
            return this.agent;
        }

        public CinemachineFreeLook GetOverworldCameraRig()
        {
            return this.overworldCameraRig;
        }

        public Controller GetController()
        {
            return this.controller;
        }

        public PlayerState GetPlayerState()
        {
            return this.playerState;
        }

        #endregion

        #region Setters

        public void SetState(PlayerState set)
        {
            this.playerState = set;
            this.playerStateEvent.Trigger(set);
        }

        #endregion

        #region In

        public void EnablePlayerControl()
        {
            this.playerInteraction.enabled = true;
            this.controller.Enable();
        }

        public void DisablePlayerControl()
        {
            this.playerInteraction.enabled = false;
            this.controller.Disable();
        }

        public void DisableOverworld()
        {
            this.playerGameObject.SetActive(false);
        }

        public void EnableOverworld()
        {
            this.playerGameObject.SetActive(true);
        }

        public void PlayAnimationClip(AnimationClip clip)
        {
        }

        #endregion
    }
}