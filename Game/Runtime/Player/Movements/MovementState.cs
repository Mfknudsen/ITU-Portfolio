using UnityEngine;

namespace Runtime.Player.Movements
{
    public abstract class MovementState
    {
        protected readonly Controller controller;
        
        protected MovementState(Controller controller)
        {
            this.controller = controller;
        }

        public abstract void ReceiveInputEvent(Event e);

        public abstract void TickMovement();
    }
}
