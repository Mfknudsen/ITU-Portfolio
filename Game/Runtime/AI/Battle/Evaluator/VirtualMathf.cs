#region Packages

using System.Collections.Generic;
using System.Linq;
using Runtime.AI.Battle.Evaluator.Virtual;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using Runtime.Weathers;
using UnityEngine;

#endregion

// ReSharper disable SuspiciousTypeConversion.Global
namespace Runtime.AI.Battle.Evaluator
{
    public static class VirtualMathf
    {
        public static bool MoveCanHit(BattleAction move, VirtualSpot user, VirtualSpot target)
        {
            if (move is not PokemonMove pokemonMove) return false;

            HitType hitType = pokemonMove.GetHitType();
            
            switch (hitType)
            {
                case HitType.All:
                    return true;
                case HitType.One or HitType.AllAdjacent:
                {
                    bool[] targets = pokemonMove.GetTargetable();

                    Pokemon actual = target.virtualPokemon.GetActualPokemon();

                    if (targets[0] && user.front == actual)
                        return true;

                    if (targets[1] && (user.strafeLeft == actual || user.strafeRight == actual))
                        return true;
                    
                    break;
                }
            }

            return hitType == HitType.AllExceptUser && user != target;
        }

        public static int CalculateVirtualDamage(PokemonMove move, Pokemon user, Pokemon target,
            VirtualBattle virtualBattle)
        {
            Category category = move.GetCategory();
            int attack = category == Category.Physical
                    ? user.GetCalculatedStat(Stat.Attack)
                    : user.GetCalculatedStat(Stat.SpAtk),
                defense = category == Category.Physical
                    ? user.GetCalculatedStat(Stat.Defence)
                    : user.GetCalculatedStat(Stat.SpDef);

            float result = ((2 * user.GetLevel()) / 5) + 2;
            result *= move.GetPower() * (attack / defense);
            result /= 50;
            result += 2;

            result = CalculateVirtualModifiers(user, target, move, virtualBattle)
                .Aggregate(result, (current, modifier) => current * modifier);

            return (int)result;
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static float[] CalculateVirtualModifiers(Pokemon user, Pokemon target, PokemonMove move,
            VirtualBattle virtualBattle)
        {
            List<float> result = new List<float>();

            Type[] defTypes = target.GetTypes();

            List<IBypassImmune> bypassImmune = new List<IBypassImmune>();
            List<IImmuneAttackType> immuneAttackType = new List<IImmuneAttackType>();
            List<IBurnStop> burnStop = new List<IBurnStop>();
            List<IFinalModifier> finalModifier = new List<IFinalModifier>();

            foreach (Pokemon pokemon in virtualBattle.spotOversight.spots.Select(spot =>
                         spot.virtualPokemon.GetFakePokemon()))
            {
                bypassImmune.AddRange(pokemon.GetAbilitiesOfType<IBypassImmune>());
                immuneAttackType.AddRange(pokemon.GetAbilitiesOfType<IImmuneAttackType>());
                burnStop.AddRange(pokemon.GetAbilitiesOfType<IBurnStop>());
                finalModifier.AddRange(pokemon.GetAbilitiesOfType<IFinalModifier>());
            }

            foreach (Weather virtualBattleWeather in virtualBattle.weathers.Where(virtualBattleWeather =>
                         virtualBattleWeather != null))
            {
                bypassImmune.Add(virtualBattleWeather as IBypassImmune);
                immuneAttackType.Add(virtualBattleWeather as IImmuneAttackType);
                burnStop.Add(virtualBattleWeather as IBurnStop);
                finalModifier.Add(virtualBattleWeather as IFinalModifier);
            }


            #region Immune

            if (target.GetTypes().Where(t => t != null).Any(t => t.GetNoEffect(move.GetMoveType().GetTypeName())) ||
                immuneAttackType.Any(i => i.MatchType(move.GetMoveType().GetTypeName())))
            {
                if (bypassImmune.Any(b => b.CanEffect(move.GetMoveType().GetTypeName(), defTypes[0].GetTypeName())))
                    return new[] { 0f };

                if (defTypes.Length > 1)
                {
                    if (bypassImmune.Any(b => b.CanEffect(move.GetMoveType().GetTypeName(), defTypes[1].GetTypeName())))
                        return new[] { 0f };
                }
            }

            #endregion

            #region Stab

            result.Add(user.IsSameType(move.GetMoveType().GetTypeName()) ? 1.5f : 1);

            #endregion

            #region Burn

            if (move.GetCategory() == Category.Physical &&
                user.GetConditionOversight().GetNonVolatileStatus() is BurnCondition &&
                !burnStop.Any(b => b.CanStopBurn(user)))
            {
                result.Add(0.5f);
            }

            #endregion

            #region Other

            result.AddRange(finalModifier.Select(f => f.Modify(move)));

            #endregion

            return result.ToArray();
        }
    }
}