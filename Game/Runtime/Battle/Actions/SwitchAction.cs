#region Packages

using System.Collections;
using System.Collections.Generic;
using Runtime.AI.Battle.Evaluator;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.Battle.Actions
{
    [CreateAssetMenu(fileName = "SwitchAction", menuName = "Action/Create new Switch Action")]
    public sealed class SwitchAction : BattleAction
    {
        #region Values

        [Header("Switch Specific Values:")] [SerializeField]
        private Trainer.Team team;

        [SerializeField] private Pokemon nextPokemon;
        [SerializeField] private Chat[] nextChat;
        [SerializeField] private Spot spot;

        #endregion

        #region Getters

        public Pokemon GetNextPokemon()
        {
            return this.nextPokemon;
        }

        public Spot GetSpot()
        {
            return this.spot;
        }

        #endregion

        #region Setters

        public void SetNextPokemon(Pokemon next)
        {
            if (next == null)
                return;

            this.nextPokemon = next;

            next.SetGettingSwitched(true);
        }

        public void SetTeam(Trainer.Team set)
        {
            this.team = set;
        }

        public void SetSpot(Spot set)
        {
            if (set == null)
                return;

            this.spot = set;
        }

        #endregion

        #region In

        public override IEnumerator Operation()
        {
            this.done = false;
            List<Chat> toSend = new List<Chat>();

            //Start of match there will be no current pokemon
            if (this.currentPokemon != null)
            {
                //Switch team member places
                this.team.SwitchTeamPlaces(this.currentPokemon, this.nextPokemon);
                
                if (this.currentPokemon.GetSpawnedObject() != null)
                {
                    foreach (Chat t in this.chatOnActivation)
                    {
                        Chat c = Instantiate(t);
                        c.AddToOverride("<POKEMON_NAME>", this.currentPokemon.GetName());
                        toSend.Add(c);
                    }

                    this.chatManager.Add(toSend.ToArray());

                    GameObject obj = this.currentPokemon.GetSpawnedObject();

                    while (!this.chatManager.GetIsClear() && obj.transform.localScale.magnitude > 0.01f)
                    {
                        obj.transform.localScale += -Vector3.one * Time.deltaTime;
                        yield return null;
                    }

                    this.currentPokemon.DespawnPokemon();
                }
            }

            this.nextPokemon.Setup();

            //Out text + Spawn new Pokemon
            toSend.Clear();
            foreach (Chat t in this.nextChat)
            {
                Chat c = Instantiate(t);
                c.AddToOverride("<NEXT_POKEMON>", this.nextPokemon.GetName());
                toSend.Add(c);
            }

            this.chatManager.Add(toSend.ToArray());

            BattleSystem.instance.SpawnPokemon(this.nextPokemon, this.spot);
            Transform inTrans = this.nextPokemon.GetSpawnedObject().transform;
            inTrans.localScale = Vector3.one * 0.1f;

            while (inTrans.localScale.y < 1)
            {
                inTrans.transform.localScale += Vector3.one * Time.deltaTime;
                if (inTrans.localScale.y > 1)
                    inTrans.localScale = Vector3.one;
                yield return null;
            }

            yield return new WaitForSeconds(1);

            this.done = true;
        }

        #endregion

        #region Out

        public override float Evaluate(Pokemon user, Pokemon target, VirtualBattle virtualBattle,
            PersonalitySetting personalitySetting)
        {
            Debug.LogError("Evaluate Switch");

            return 0;
        }

        #endregion
    }
}