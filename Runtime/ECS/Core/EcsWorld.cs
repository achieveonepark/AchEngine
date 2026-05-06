using System;
using Unity.Collections;
using Unity.Entities;

namespace AchEngine.ECS
{
    public sealed class EcsWorld : IEcsWorld
    {
        public EcsWorld(World world)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        public static EcsWorld Default
        {
            get
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world == null)
                {
                    throw new InvalidOperationException("Default ECS world is not available.");
                }

                return new EcsWorld(world);
            }
        }

        public World World { get; }
        public EntityManager EntityManager => World.EntityManager;
        public bool IsCreated => World != null && World.IsCreated;

        public Entity CreateEntity(params ComponentType[] componentTypes)
        {
            return EntityManager.CreateEntity(componentTypes);
        }

        public Entity Instantiate(Entity prefab)
        {
            return EntityManager.Instantiate(prefab);
        }

        public void Destroy(Entity entity)
        {
            if (EntityManager.Exists(entity))
            {
                EntityManager.DestroyEntity(entity);
            }
        }

        public bool Exists(Entity entity)
        {
            return EntityManager.Exists(entity);
        }

        public IEcsCommandBuffer CreateCommandBuffer(Allocator allocator = Allocator.TempJob)
        {
            return new EcsCommandBuffer(allocator);
        }

        public bool HasComponent<T>(Entity entity)
            where T : unmanaged, IComponentData
        {
            return EntityManager.Exists(entity) && EntityManager.HasComponent<T>(entity);
        }

        public T GetComponent<T>(Entity entity)
            where T : unmanaged, IComponentData
        {
            return EntityManager.GetComponentData<T>(entity);
        }

        public void SetComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData
        {
            EntityManager.SetComponentData(entity, component);
        }

        public void AddComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData
        {
            EntityManager.AddComponentData(entity, component);
        }

        public void RemoveComponent<T>(Entity entity)
            where T : unmanaged, IComponentData
        {
            if (EntityManager.Exists(entity) && EntityManager.HasComponent<T>(entity))
            {
                EntityManager.RemoveComponent<T>(entity);
            }
        }

        public bool HasBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return EntityManager.Exists(entity) && EntityManager.HasBuffer<T>(entity);
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return EntityManager.GetBuffer<T>(entity);
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return EntityManager.AddBuffer<T>(entity);
        }

        public EntityQuery CreateQuery(params ComponentType[] all)
        {
            return EntityManager.CreateEntityQuery(all);
        }

        public EntityQuery CreateQuery(EcsQueryDescription description)
        {
            return EntityManager.CreateEntityQuery(description.ToEntityQueryDesc());
        }
    }
}
