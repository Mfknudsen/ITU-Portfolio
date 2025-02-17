#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Pokémon;
using UnityEngine;

#endregion

namespace Runtime.AI.Battle.Evaluator
{
    public class Evaluator
    {
        private readonly Pokemon pokemon;
        private readonly List<BattleAction> actions;

        private readonly EvaluatorSetting setting;
        private string preMoveName;
        private float preValue;

        public bool UsedForPokemon(Pokemon comparor)
        {
            return this.pokemon == comparor;
        }

        public Evaluator(Pokemon pokemon, EvaluatorSetting setting)
        {
            this.pokemon = pokemon;
            this.setting = setting;

            this.preMoveName = "";
            this.preValue = 0;
            this.actions = new List<BattleAction>();

            this.actions.AddRange(pokemon.GetMoves().Where(move => move != null));

            if (setting.canSwitchOut) this.actions.Add(BattleSystem.instance.InstantiateSwitchAction());
            if (setting.canUseItems) this.actions.Add(BattleSystem.instance.InstantiateItemAction());
        }

        public void EvaluateForPokemon()
        {
            VirtualBattle virtualBattle = new VirtualBattle();

            List<VirtualMove> virtualMoves = new List<VirtualMove>();
            VirtualSpotOversight spotOversight = virtualBattle.spotOversight;
            VirtualSpot userSpot = spotOversight.GetPokemonSpot(this.pokemon);
            VirtualPokemon user = userSpot.virtualPokemon;

            foreach (BattleAction battleAction in this.actions.Where(action => action != null))
            {
                foreach (VirtualSpot spot in spotOversight.spots.Where(s =>
                             VirtualMathf.MoveCanHit(battleAction, userSpot, s)))
                {
                    BattleAction rootAction = SetupRootAction(battleAction, user.GetActualPokemon(),
                        spot.virtualPokemon.GetActualPokemon());

                    virtualMoves.Add(new VirtualMove(
                        rootAction,
                        0,
                        battleAction, this.pokemon,
                        spot.virtualPokemon.GetFakePokemon(),
                        virtualBattle,
                        this.setting.personalitySetting)
                    );
                }
            }

            this.setting.personalitySetting.Tick();
            this.Evaluate(user, this.setting.depth, virtualMoves.ToArray());
        }

        private void Evaluate(VirtualPokemon user, int depth, IReadOnlyList<VirtualMove> toCheck)
        {
            if (toCheck.Count == 0) return;

            if (depth == 0)
            {
                VirtualMove highest = toCheck[0];
                for (int i = 1; i < toCheck.Count; i++)
                {
                    if (toCheck[i].value <= highest.value)
                        continue;

                    highest = toCheck[i];
                }

                BattleAction rootAction = highest.rootAction;

                if (rootAction.name == this.preMoveName)
                {
                    this.preValue += this.setting.continuesIncrease;

                    if (Random.Range(20, 100) < this.preValue)
                    {
                        for (int i = 1; i < toCheck.Count; i++)
                        {
                            if (toCheck[i].value <= highest.value ||
                                toCheck[i].rootAction.name == this.preMoveName)
                                continue;

                            highest = toCheck[i];
                        }
                    }
                }
                else
                {
                    this.preMoveName = rootAction.name;
                    this.preValue = 0;
                }

                user.GetActualPokemon().SetBattleAction(highest.rootAction);
                return;
            }

            List<VirtualMove> nextMoves = new List<VirtualMove>();
            foreach (VirtualMove virtualMove in toCheck)
            {
                if (virtualMove.rootAction is SwitchAction)
                    continue;

                foreach (VirtualSpot spot in virtualMove.virtualBattle.spotOversight.spots)
                {
                    foreach (BattleAction battleAction in this.actions)
                    {
                        nextMoves.Add(new VirtualMove(
                            virtualMove.rootAction,
                            virtualMove.value,
                            battleAction,
                            user.GetFakePokemon(),
                            spot.virtualPokemon.GetFakePokemon(),
                            virtualMove.virtualBattle, this.setting.personalitySetting
                        ));
                    }
                }
            }

            // ReSharper disable once TailRecursiveCall
            this.Evaluate(user, depth - 1, nextMoves.ToArray());
        }

        private static BattleAction SetupRootAction(BattleAction battleAction, Pokemon user, Pokemon target)
        {
            BattleAction result = Object.Instantiate(battleAction);
            result.SetCurrentPokemon(user);
            result.SetTargets(target);

            return result;
        }
    }
}