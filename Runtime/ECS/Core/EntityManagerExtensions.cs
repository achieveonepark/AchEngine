using Unity.Entities;

namespace AchEngine.ECS
{
    public static class EntityManagerExtensions
    {
        public static bool TryGetComponent<T>(this EntityManager manager, Entity entity, out T component)
            where T : unmanaged, IComponentData
        {
            if (!manager.Exists(entity) || !manager.HasComponent<T>(entity))
            {
                component = default;
                return false;
            }

            component = manager.GetComponentData<T>(entity);
            return true;
        }

        public static T GetOrAddComponent<T>(this EntityManager manager, Entity entity, T defaultValue = default)
            where T : unmanaged, IComponentData
        {
            if (!manager.HasComponent<T>(entity))
            {
                manager.AddComponentData(entity, defaultValue);
                return defaultValue;
            }

            return manager.GetComponentData<T>(entity);
        }

        public static void SetOrAddComponent<T>(this EntityManager manager, Entity entity, T component)
            where T : unmanaged, IComponentData
        {
            if (manager.HasComponent<T>(entity))
            {
                manager.SetComponentData(entity, component);
                return;
            }

            manager.AddComponentData(entity, component);
        }

        public static bool RemoveComponentIfExists<T>(this EntityManager manager, Entity entity)
            where T : unmanaged, IComponentData
        {
            if (!manager.Exists(entity) || !manager.HasComponent<T>(entity))
            {
                return false;
            }

            manager.RemoveComponent<T>(entity);
            return true;
        }

        public static DynamicBuffer<T> GetOrAddBuffer<T>(this EntityManager manager, Entity entity)
            where T : unmanaged, IBufferElementData
        {
            if (!manager.HasBuffer<T>(entity))
            {
                return manager.AddBuffer<T>(entity);
            }

            return manager.GetBuffer<T>(entity);
        }
    }
}
