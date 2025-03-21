using Unity.Entities;

namespace Runtime.AI.EntitySystems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class NavigationSystemGroup : ComponentSystemGroup
    {
    }
}