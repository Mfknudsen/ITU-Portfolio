using Unity.Entities;

namespace Runtime.AI.EntityComponents
{
    public struct AgentSettingsComponent : IComponentData
    {
        public int ID;
        public float Radius;
    }
}