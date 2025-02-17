using UnityEngine;

namespace Runtime.Player.Movements
{
    public class GroundMovementState : MovementState
    {
        public GroundMovementState(Controller controller) : base(controller)
        {
        }

        public override void ReceiveInputEvent(Event e)
        {
            Debug.Log(this.controller);
        }

        public override void TickMovement()
        {
            throw new System.NotImplementedException();
        }
    }
}
