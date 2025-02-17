#region Packages

using System.Collections;
using System.Linq;
using Runtime.Battle.Actions;
using Runtime.Battle.Systems;
using Runtime.Battle.Systems.Interfaces;
using Runtime.Pokémon;
using Runtime.Systems;
using UnityEngine;

#endregion

namespace Runtime.Weathers.Energy
{
    public class DragonForceWeather : EnergyWeather, IOnSuperEffective, IOnMoveHit, IOperation
    {
        #region Values

        private bool hasHit;
        private Pokemon affected;

        #endregion

        public float Effect(PokemonMove pokemonMove)
        {
            if (!this.amplified)
                return 0.8f;

            return pokemonMove.GetMoveType().GetTypeName() == TypeName.Dragon
                ? 1.5f
                : 1.3f;
        }

        public bool IsOperationDone => throw new System.NotImplementedException();

        public bool MultiHit(PokemonMove pokemonMove)
        {
            if (pokemonMove.GetCurrentPokemon() == this.affected)
                this.hasHit = true;
            else
            {
                this.affected = pokemonMove.GetCurrentPokemon();
                this.hasHit = false;
            }

            return this.hasHit;
        }

        public IEnumerator Operation()
        {
            float secPerPokeMove = 200 * BattleSystem.instance.GetSecPerPokeMove();
            
            if (this.affected == null || this.affected.GetTypes()
                .Any(type => type.GetTypeName() == TypeName.Dragon)) yield break;

            int damagePerTarget = this.affected.GetCalculatedStat(Stat.HP) / 10;
            float damageApplied = 0, damageOverTime = damagePerTarget / secPerPokeMove;

            while (damageApplied < damagePerTarget)
            {
                if (damageApplied + damageOverTime >= damagePerTarget)
                    damageOverTime = damagePerTarget - damageApplied;

                damageApplied += damageOverTime;

                this.affected.ReceiveDamage(damageOverTime);

                yield return new WaitForSeconds(BattleSystem.instance.GetSecPerPokeMove() / secPerPokeMove);
            }
        }

        public void OperationEnd()
        {
        }
    }
}