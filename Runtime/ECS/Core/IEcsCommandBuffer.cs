using System;
using Unity.Entities;

namespace AchEngine.ECS
{
    public interface IEcsCommandBuffer : IDisposable
    {
        EntityCommandBuffer Buffer { get; }
        bool IsCreated { get; }

        Entity CreateEntity();
        Entity CreateEntity<T>(T component)
            where T : unmanaged, IComponentData;

        Entity Instantiate(Entity prefab);

        IEcsCommandBuffer AddComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData;

        IEcsCommandBuffer SetComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData;

        IEcsCommandBuffer RemoveComponent<T>(Entity entity)
            where T : unmanaged, IComponentData;

        DynamicBuffer<T> AddBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData;

        IEcsCommandBuffer Destroy(Entity entity);
        void Playback(EntityManager manager);
    }
}
