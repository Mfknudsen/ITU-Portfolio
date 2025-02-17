#region Packages

using Runtime.Battle.Actions;
using Runtime.Pokémon;

#endregion

namespace Runtime.AI.Battle.Evaluator.Virtual
{
    public class VirtualMove
    {
        public readonly float value;
        public readonly VirtualBattle virtualBattle;
        public readonly BattleAction rootAction;

        public VirtualMove(BattleAction rootAction, float preValue,
            BattleAction action, Pokemon user, Pokemon target,
            VirtualBattle virtualBattle, PersonalitySetting personalitySetting)
        {
            this.rootAction = rootAction;
            this.virtualBattle = new VirtualBattle(virtualBattle);
            this.value = preValue + action.Evaluate(user, target, this.virtualBattle, personalitySetting);
        }
    }
}