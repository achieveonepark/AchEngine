using Unity.Collections;
using Unity.Entities;

namespace AchEngine.ECS
{
    public interface IEcsWorld
    {
        World World { get; }
        EntityManager EntityManager { get; }
        bool IsCreated { get; }

        Entity CreateEntity(params ComponentType[] componentTypes);
        Entity Instantiate(Entity prefab);
        void Destroy(Entity entity);
        bool Exists(Entity entity);
        IEcsCommandBuffer CreateCommandBuffer(Allocator allocator = Allocator.TempJob);

        bool HasComponent<T>(Entity entity)
            where T : unmanaged, IComponentData;

        T GetComponent<T>(Entity entity)
            where T : unmanaged, IComponentData;

        void SetComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData;

        void AddComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData;

        void RemoveComponent<T>(Entity entity)
            where T : unmanaged, IComponentData;

        bool HasBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData;

        DynamicBuffer<T> GetBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData;

        DynamicBuffer<T> AddBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData;

        EntityQuery CreateQuery(params ComponentType[] all);
        EntityQuery CreateQuery(EcsQueryDescription description);
    }
}
