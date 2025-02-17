#region Packages

using System.Collections;
using Runtime.Communication;
using Runtime.Systems;

#endregion

namespace Runtime.Battle.Systems.Static_Operations
{
    public class ChatOperation : IOperation
    {
        private bool done;
        private readonly Chat[] toPlay;
        private readonly ChatManager chatManager;

        public ChatOperation(Chat[] toPlay)
        {
            this.toPlay = toPlay;
        }

        public ChatOperation(Chat toPlay)
        {
            this.toPlay = new[] { toPlay };
        }

        public bool IsOperationDone => this.done;

        public IEnumerator Operation()
        {
            this.done = false;

            this.chatManager.Add(this.toPlay);

            while (!this.chatManager.GetIsClear())
                yield return null;

            this.done = true;
        }

        public void OperationEnd()
        {
        }
    }
}