#region Packages

using System.Collections; 

#endregion

namespace Runtime.Systems.PersistantRunner
{
    public interface IFrameStart
    {
        public IEnumerator FrameStart(PersistantRunner runner);
    }
}
