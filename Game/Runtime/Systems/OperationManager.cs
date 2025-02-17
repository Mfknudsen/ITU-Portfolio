#region Packages

using System.Collections;
using System.Collections.Generic;
using Runtime.Systems.PersistantRunner;
using UnityEngine;

#endregion

namespace Runtime.Systems
{
    [CreateAssetMenu(menuName = "Manager/Operation")]
    public class OperationManager : Manager, IFrameStart, IFrameUpdate
    {
        #region Values

        private PersistantRunner.PersistantRunner operationController;

        private bool done;

        private readonly Queue<OperationsContainer> operationsContainers = new Queue<OperationsContainer>();
        private OperationsContainer currentContainer;

        private readonly Dictionary<OperationsContainer, List<Coroutine>> activeAsyncCoroutines =
            new Dictionary<OperationsContainer, List<Coroutine>>();

        #endregion

        #region Build In States

        public IEnumerator FrameStart(PersistantRunner.PersistantRunner runner)
        {
            while (this.operationController == null)
            {
                this.operationController = FindObjectOfType<PersistantRunner.PersistantRunner>();
                yield return null;
            }

            this.ready = true;
        }

        public void FrameUpdate()
        {
            if (this.currentContainer == null)
            {
                if (this.operationsContainers.Count == 0) return;

                this.currentContainer = this.operationsContainers.Dequeue();

                foreach (IOperation i in this.currentContainer.GetInterfaces())
                    this.operationController.StartCoroutine(i.Operation());
            }
            else
            {
                this.done = true;

                foreach (IOperation i in this.currentContainer.GetInterfaces())
                {
                    if (i.IsOperationDone) continue;

                    this.done = false;
                }

                if (!this.done) return;

                foreach (IOperation i in this.currentContainer.GetInterfaces())
                    i.OperationEnd();

                this.currentContainer = null;
            }
        }

        #endregion

        #region Getters

        public bool GetDone()
        {
            return this.done;
        }

        #endregion

        #region In

        public void AddOperationsContainer(OperationsContainer set)
        {
            this.done = false;

            this.operationsContainers.Enqueue(set);
        }

        public void AddOperationsContainer(IEnumerable<OperationsContainer> sets)
        {
            this.done = false;

            foreach (OperationsContainer container in sets)
                this.operationsContainers.Enqueue(container);
        }

        public void AddAsyncOperationsContainer(OperationsContainer container)
        {
            this.activeAsyncCoroutines.Add(container, new List<Coroutine>());

            foreach (IOperation i in container.GetInterfaces())
                this.activeAsyncCoroutines[container].Add(this.operationController.StartCoroutine(i.Operation()));
        }

        public void InsertFront(OperationsContainer set)
        {
            this.done = false;

            List<OperationsContainer> containerHolder = new List<OperationsContainer>();
            while (this.operationsContainers.Count > 0)
                containerHolder.Add(this.operationsContainers.Dequeue());

            this.operationsContainers.Enqueue(set);
            foreach (OperationsContainer operationsContainer in containerHolder)
                this.operationsContainers.Enqueue(operationsContainer);
        }

        public void StopAsyncContainer(OperationsContainer toStop, bool triggerEnd = false)
        {
            foreach (Coroutine coroutine in this.activeAsyncCoroutines[toStop])
                this.operationController.StopCoroutine(coroutine);

            if (!triggerEnd) return;

            foreach (IOperation operation in toStop.GetInterfaces()) operation.OperationEnd();
        }

        #endregion
    }

    public class OperationsContainer
    {
        private readonly List<IOperation> operationInterfaces = new List<IOperation>();

        public OperationsContainer()
        {
        }

        public OperationsContainer(IOperation set) => 
            this.operationInterfaces.Add(set);

        public OperationsContainer(IEnumerable<IOperation> set) => 
            this.operationInterfaces.AddRange(set);

        public void Add(IOperation operationInterface)
        {
            if (operationInterface == null)
                return;

            this.operationInterfaces.Add(operationInterface);
        }

        public void Add(IOperation[] interfaces)
        {
            if (interfaces == null)
                return;

            foreach (IOperation i in interfaces) this.operationInterfaces.Add(i);
        }

        public IEnumerable<IOperation> GetInterfaces() => this.operationInterfaces.ToArray();
    }

    public interface IOperation
    {
        public bool IsOperationDone { get; }

        public IEnumerator Operation();

        public void OperationEnd();
    }
}