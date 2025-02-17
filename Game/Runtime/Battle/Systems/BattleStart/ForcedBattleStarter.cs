namespace Runtime.Battle.Systems.BattleStart
{
    public class ForcedBattleStarter : BattleStarter
    {
        public override void InteractTrigger() =>
            this.TriggerBattle();
    }
}