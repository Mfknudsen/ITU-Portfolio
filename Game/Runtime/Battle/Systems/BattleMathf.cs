#region Packages

using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Battle.Systems.Spots;
using Runtime.Communication;
using Runtime.Items.Pokeballs;
using Runtime.Pokémon;
using Runtime.Pokémon.Conditions;
using Runtime.Pokémon.Conditions.Non_Volatiles;
using UnityEngine;
using Random = UnityEngine.Random;
using Type = Runtime.Pokémon.Type;

#endregion

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable once ParameterTypeCanBeEnumerable.Global
// ReSharper disable once MemberCanBePrivate.Global
namespace Runtime.Battle.Systems
{
    public static class BattleMathf
    {
        #region Values

        private static ChatManager chatManager;

        private static Chat _superEffective,
            _notEffective,
            _noEffect,
            _barelyEffective,
            _extremlyEffective;

        private static Chat _missChat;

        #endregion

        #region Setter

        public static void SetSuperEffective(Chat input)
        {
            _superEffective = input;
        }

        public static void SetNotEffective(Chat input)
        {
            _notEffective = input;
        }

        public static void SetNoEffect(Chat input)
        {
            _noEffect = input;
        }

        public static void SetBarelyEffective(Chat input)
        {
            _barelyEffective = input;
        }

        public static void SetExtremlyEffective(Chat input)
        {
            _extremlyEffective = input;
        }

        public static void SetMissChat(Chat input)
        {
            _missChat = input;
        }

        #endregion

        #region Getters

        public static Chat GetMissChat()
        {
            return _missChat;
        }

        #endregion

        #region Out

        public static float GetMultiplierValue(int index, bool baseStat)
        {
            if (index > 0)
                return 1 + index * (baseStat ? 0.5f : 0.33f);

            return index switch
            {
                -1 => baseStat ? 0.666f : 0.75f,
                -2 => baseStat ? 0.5f : 0.6f,
                -3 => baseStat ? 0.4f : 0.5f,
                -4 => baseStat ? 0.333f : 0.428f,
                -5 => baseStat ? 0.285f : 0.375f,
                -6 => baseStat ? 0.25f : 0.33f,
                _ => 1
            };
        }

        public static bool CanHit(Spot user, Spot target, PokemonMove pokemonMove)
        {
            if (user is null || target is null || pokemonMove is null)
                return false;

            HitType type = pokemonMove.GetHitType();

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ((type == HitType.AllAdjacent || type == HitType.AllAdjacentOneSide || type == HitType.One) &&
                user.GetAllAdjacentSpots().Contains(target))
                return true;

            if ((type == HitType.AllExceptUser || type == HitType.AllOneSideExceptUser) && target == user)
                return false;

            if (type == HitType.AllExceptUser || type == HitType.AllOneSideExceptUser || type == HitType.All ||
                type == HitType.AllOneSide)
                return true;

            return false;
        }

        public static bool MultiTargets(Spot target, Spot user, PokemonMove move)
        {
            HitType type = move.GetHitType();
            List<Spot> totals = new List<Spot>();

            switch (type)
            {
                case HitType.One:
                    return false;

                case HitType.AllAdjacent:
                    totals = user.GetAllAdjacentSpots();

                    return totals.Count > 1;

                case HitType.AllAdjacentOneSide:
                    totals.Add(target);
                    totals.Add(target.GetLeft());
                    totals.Add(target.GetRight());

                    List<Spot> checks = user.GetAllAdjacentOneSideSpots(target.GetBattleMember().GetTeamAffiliation() ==
                                                                        user.GetBattleMember().GetTeamAffiliation());

                    foreach (Spot total in totals.Where(total => total != target)
                                 .Where(total => !checks.Contains(total)))
                        totals.Remove(total);

                    return totals.Count > 1;

                case HitType.AllOneSideExceptUser:
                    totals = target.GetAllOneSide();

                    if (totals.Contains(user))
                        totals.Remove(user);

                    return totals.Count > 1;

                case HitType.AllOneSide:
                    totals = target.GetAllOneSide();

                    return totals.Count > 1;

                case HitType.AllExceptUser:
                    totals = BattleSystem.instance.GetSpotOversight().GetSpots();

                    if (totals.Contains(user))
                        totals.Remove(user);

                    return totals.Count > 1;

                case HitType.All:
                    totals = BattleSystem.instance.GetSpotOversight().GetSpots();

                    return totals.Count > 1;

                default:
                    return false;
            }
        }

        #region Calculations

        public static int CalculateCatch(Pokemon target, Pokeball pokeball)
        {
            if (pokeball is Masterball)
                return 4;

            Condition condition = target.GetConditionOversight().GetNonVolatileStatus();
            int maxHP = target.GetMaxHealth(), currentHP = (int)target.GetCurrentHealth();

            float a = 3 * maxHP;
            a -= 2 * currentHP;
            a *= target.GetCatchRate();
            a *= pokeball.GetCatchStat();
            a /= 3 * maxHP;
            a *= condition switch
            {
                SleepCondition _ => 2,
                FreezeCondition _ => 2,
                ParalysisCondition _ => 1.5f,
                PoisonCondition _ => 1.5f,
                BurnCondition _ => 1.5f,
                _ => 1
            };

            float b = 1048560 / Mathf.Sqrt(
                Mathf.Sqrt(
                    16711680 / a
                ));
            int shakeCheck = 0;

            for (int i = 0; i < 4; i++)
            {
                int random = Random.Range(0, 65535);

                if (random >= b)
                    break;

                shakeCheck++;
            }

            return shakeCheck;
        }

        public static int CalculateDamage(int level, float attack, float defense, float power, float[] modifiers)
        {
            float result = ((2 * level) / 5) + 2;
            result *= power * (attack / defense);
            result /= 50;
            result += 2;

            result = modifiers.Aggregate(result, (current, modifier) => current * modifier);

            return (int)result;
        }

        public static float[] CalculateModifiers(Pokemon attacker, Pokemon target, PokemonMove attackMove,
            bool multiTarget, bool isCritical)
        {
            TypeName attackType = attackMove.GetMoveType().GetTypeName();
            List<float> result = new List<float>();
            AbilityOversight abilityOversight = BattleSystem.instance.GetAbilityOversight();
            WeatherManager weatherManager = BattleSystem.instance.GetWeatherManager();

            #region Targets

            float targets = multiTarget ? 0.75f : 1;

            result.Add(targets);

            #endregion

            #region Badge

            // ReSharper disable once ConvertToConstant.Local
            float badge = 1;

            result.Add(badge);

            #endregion

            #region Critical

            float critical = isCritical ? 1.5f : 1;

            result.Add(critical);

            #endregion

            #region Random

            float random = Random.Range(0.75f, 1.0f);
            result.Add(random);

            #endregion

            #region Stab

            result.Add(attacker.IsSameType(attackType) ? 1.5f : 1);

            #endregion

            #region No Effect

            float type = 1;

            Type[] toCheck = target.GetTypes();
            List<IBypassImmune> bypassImmune = new List<IBypassImmune>();
            List<IImmuneAttackType> immuneAttackType = new List<IImmuneAttackType>();

            foreach (Pokemon pokemon in BattleSystem.instance.GetSpotOversight().GetSpots()
                         .Select(spot => spot.GetActivePokemon()).Where(pokemon => pokemon != null))
            {
                bypassImmune.AddRange(pokemon.GetAbilitiesOfType<IBypassImmune>());
                immuneAttackType.AddRange(pokemon.GetAbilitiesOfType<IImmuneAttackType>());
            }

            bypassImmune.AddRange(BattleSystem.instance.GetWeatherManager()
                .GetWeatherWithInterface<IBypassImmune>());
            immuneAttackType.AddRange(BattleSystem.instance.GetWeatherManager()
                .GetWeatherWithInterface<IImmuneAttackType>());

            if (target.GetTypes().Where(t => t != null)
                    .Any(t => t.GetNoEffect(attackMove.GetMoveType().GetTypeName())) ||
                immuneAttackType.Any(i => i.MatchType(attackMove.GetMoveType().GetTypeName())))
            {
                if (bypassImmune.Any(b =>
                        b.CanEffect(attackMove.GetMoveType().GetTypeName(), toCheck[0].GetTypeName())))
                    type = 0;

                if (toCheck.Length > 1)
                {
                    if (bypassImmune.Any(b =>
                            b.CanEffect(attackMove.GetMoveType().GetTypeName(), toCheck[1].GetTypeName())))
                        type = 0;
                }
            }

            if (type != 0)
            {
                type += toCheck[0].GetWeakness(attackType) - toCheck[0].GetResistance(attackType);

                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                if (toCheck[1] != null)
                    type += (toCheck[1].GetWeakness(attackType) - toCheck[1].GetResistance(attackType));

                switch (type)
                {
                    case 0:
                        type = 0.5f;
                        chatManager.Add(_notEffective.GetChatInstantiated());
                        break;
                    case -1:
                        type = 0.25f;
                        chatManager.Add(_barelyEffective.GetChatInstantiated());
                        break;
                    case 2:
                        type = 1.5f;
                        chatManager.Add(_superEffective.GetChatInstantiated());
                        break;
                    case 3:
                        type = 2;
                        chatManager.Add(_extremlyEffective.GetChatInstantiated());
                        break;
                    default:
                        type = 1;
                        break;
                }
            }
            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
            else if (_noEffect != null)
                chatManager.Add(_noEffect.GetChatInstantiated());

            result.Add(type);

            #endregion

            #region Burn

            float burn = 1;
            if (attackMove.GetCategory() == Category.Physical &&
                attacker.GetConditionOversight()?.GetNonVolatileStatus() is BurnCondition)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                burn = 0.5f;

                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (IBurnStop ability in BattleSystem.instance.GetAbilityOversight().ListOfSpecific<IBurnStop>())
                {
                    if (!ability.CanStopBurn(attacker)) continue;

                    burn = 1;
                    break;
                }
            }

            result.Add(burn);

            #endregion

            #region Other

            result.AddRange(abilityOversight.ListOfSpecific<IFinalModifier>()
                .Concat(weatherManager.GetWeatherWithInterface<IFinalModifier>())
                .Select(finalModifier => finalModifier.Modify(attackMove)));

            #endregion

            return result.ToArray();
        }

        public static bool CalculateCriticalRoll(Pokemon user, Pokemon target)
        {
            //Critical Chance base on Generation 3 - 5

            if (target.GetAbilitiesOfType<ICritImmune>().Any(i => i.CanEffect(target)))
                return false;

            float chance = user.GetCritical() switch
            {
                0 => 6.25f,
                1 => 12.5f,
                2 => 25,
                3 => 33.3f,
                _ => 50
            };

            chance = BattleSystem.instance.GetAbilityOversight().ListOfSpecific<ICritModifier>()
                .Aggregate(chance, (current, critModifier) => current * critModifier.Modify(user));

            int random = Random.Range(0, 100);

            return random <= chance;
        }

        public static int CalculateConfusionDamage(int level, float attack, float defense)
        {
            const float power = 40;
            float result = ((2 * level) / 5) + 2;
            result *= power * (attack / defense);
            result /= 50;
            result += 2;
            result *= Random.Range(0.75f, 1.0f);

            return (int)result;
        }

        public static float CalculateOtherStat(Stat stat, int stages)
        {
            if (stat != Stat.Accuracy && stat != Stat.Evasion)
                throw new Exception("Stat Must Be Accuracy or Evasion");

            if (stat == Stat.Accuracy)
            {
                return stages switch
                {
                    -6 => 0.33f,
                    -5 => 0.375f,
                    -4 => 0.428f,
                    -3 => 0.5f,
                    -2 => 0.6f,
                    -1 => 0.75f,
                    1 => 1.33f,
                    2 => 1.66f,
                    3 => 2,
                    4 => 2.33f,
                    5 => 2.66f,
                    6 => 3,
                    _ => 1,
                };
            }

            return stages switch
            {
                6 => 0.33f,
                5 => 0.375f,
                4 => 0.428f,
                3 => 0.5f,
                2 => 0.6f,
                1 => 0.75f,
                -1 => 1.33f,
                -2 => 1.66f,
                -3 => 2,
                -4 => 2.33f,
                -5 => 2.66f,
                -6 => 3,
                _ => 1,
            };
        }

        public static int CalculateBaseStat(int baseStat, int iv, int ev, int level)
        {
            float result = (baseStat + iv) * 2;
            result += (float)ev / 4;
            result *= level;
            result /= 100;
            result += 5;

            return (int)Mathf.Floor(result);
        }

        public static int CalculateHPStat(int baseStat, int iv, int ev, int level)
        {
            float result = 2 * baseStat + iv;
            result += (float)ev / 4;
            result *= level;
            result /= 100;
            result += level + 10;

            return (int)Mathf.Floor(result);
        }

        public static bool CalculateHit(int accuracyMove, float accuracyUser, float evaTarget)
        {
            float t = accuracyMove * accuracyUser * evaTarget;
            t *= 2.55f;
            int r = Random.Range(0, 255);
            return r <= t;
        }

        public static bool CalculateStatusHit(int chance)
        {
            int random = Random.Range(0, 100);

            return random <= chance;
        }

        #endregion

        #endregion
    }
}