using Runtime.AI.EntityComponents;
using Unity.Entities;
using Unity.Transforms;

namespace Runtime.AI.EntityAspects
{
    internal readonly partial struct UpdateDestinationAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRO<UnitAgentComponent> AgentComponent;
        public readonly RefRO<LocalTransform> Transform;
        public readonly RefRW<DestinationComponent> DestinationComponent;
    }
}