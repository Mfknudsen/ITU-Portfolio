using Runtime.AI.EntityBuffers;
using Runtime.AI.EntityComponents;
using Runtime.AI.Navigation;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Runtime.AI.EntityAuthoring
{
    public class AgentComponentAuthoring : MonoBehaviour
    {
        [SerializeField] [Required] private UnitAgentSettings settings;

        private class Baker : Baker<AgentComponentAuthoring>
        {
            public override void Bake(AgentComponentAuthoring authoring)
            {
                Entity entity = this.GetEntity(TransformUsageFlags.Dynamic);

                this.AddBuffer<WayPointBufferElement>(entity);

                this.AddComponent(entity, new UnitAgentComponent()
                {
                    CurrentTriangleID = -1
                });
                this.AddComponent(entity, new AgentSettingsComponent
                {
                    ID = authoring.settings.ID,
                    Radius = authoring.settings.Radius
                });
            }
        }
    }
}