using System.Collections;

namespace Runtime.Battle.Systems.Interfaces
{
    public interface IOnTurnEnd
    {
        public IEnumerator Operation();
    }
}