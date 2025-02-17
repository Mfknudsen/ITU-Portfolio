using Runtime.AI.EntityComponents;
using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    public partial struct AgentUpdateIntendedDirectionJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref UnitAgentComponent agent)
        {
        }
    }
}