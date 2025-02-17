using System.Collections;
using Runtime.Systems;

namespace Runtime.Pokémon.Conditions.Volatiles
{
    public class Wrap : VolatileCondition, IOperation
    {
        public override bool CanIncrease()
        {
            return false;
        }

        public override void Increase(Condition condition)
        {
            throw new System.NotImplementedException();
        }

        public override void Reset()
        {
            throw new System.NotImplementedException();
        }

        public bool IsOperationDone => throw new System.NotImplementedException();

        public IEnumerator Operation()
        {
            throw new System.NotImplementedException();
        }

        public void OperationEnd()
        {
        }
    }
}
