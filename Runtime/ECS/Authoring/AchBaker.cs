using Unity.Entities;
using UnityEngine;

namespace AchEngine.ECS
{
    public abstract class AchBaker<TAuthoring> : Baker<TAuthoring>
        where TAuthoring : Component
    {
        protected Entity PrimaryEntity(TransformUsageFlags flags = TransformUsageFlags.Dynamic)
        {
            return GetEntity(flags);
        }

        protected void AddComponentToPrimary<T>(T component, TransformUsageFlags flags = TransformUsageFlags.Dynamic)
            where T : unmanaged, IComponentData
        {
            AddComponent(PrimaryEntity(flags), component);
        }

        protected DynamicBuffer<T> AddBufferToPrimary<T>(TransformUsageFlags flags = TransformUsageFlags.Dynamic)
            where T : unmanaged, IBufferElementData
        {
            return AddBuffer<T>(PrimaryEntity(flags));
        }
    }
}
