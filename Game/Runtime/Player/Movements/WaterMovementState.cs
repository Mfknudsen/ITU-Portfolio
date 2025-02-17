using UnityEngine;

namespace Runtime.Player.Movements
{
    public class WaterMovementState : MovementState
    {
        public WaterMovementState(Controller controller) : base(controller)
        {
        }

        public override void ReceiveInputEvent(Event e)
        {
            throw new System.NotImplementedException();
        }

        public override void TickMovement()
        {
            throw new System.NotImplementedException();
        }
    }
}
