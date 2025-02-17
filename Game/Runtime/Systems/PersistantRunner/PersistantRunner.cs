#region Packages

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#endregion

namespace Runtime.Systems.PersistantRunner
{
    [DisallowMultipleComponent]
    public sealed class PersistantRunner : MonoBehaviour
    {
        #region Values

        [SerializeField, AssetsOnly] private List<Manager> managers = new List<Manager>();

        private List<IFrameStart> frameStarts;
        private List<IFrameUpdate> frameUpdates;
        private List<IFrameLateUpdate> frameLateUpdates;

        #endregion

        #region Build In States

        private void Start()
        {
            if (this.managers.Count == 0)
                return;

            DontDestroyOnLoad(this.gameObject);

            this.frameStarts = this.managers
                .Where(m => m != null && m.GetType().GetInterfaces().Contains(typeof(IFrameStart)))
                .Select(m => m as IFrameStart)
                .ToList();
            this.frameUpdates = this.managers
                .Where(m => m != null && m.GetType().GetInterfaces().Contains(typeof(IFrameUpdate)))
                .Select(m => m as IFrameUpdate)
                .ToList();
            this.frameLateUpdates = this.managers
                .Where(m => m != null && m.GetType().GetInterfaces().Contains(typeof(IFrameLateUpdate)))
                .Select(m => m as IFrameLateUpdate)
                .ToList();

            foreach (IFrameStart frameStart in this.frameStarts)
                this.StartCoroutine(frameStart.FrameStart(this));
        }

        private void Update()
        {
            foreach (IFrameUpdate frameUpdate in this.frameUpdates)
                frameUpdate.FrameUpdate();
        }

        private void LateUpdate()
        {
            foreach (IFrameLateUpdate frameLateUpdate in this.frameLateUpdates)
                frameLateUpdate.FrameLateUpdate();
        }

        #endregion

        #region In

#if UNITY_EDITOR

        public void AddManagers(IEnumerable<Manager> toAdd)
        {
            this.managers = toAdd.ToList();
            
            this.Start();
        }
#endif

        #endregion
    }
}