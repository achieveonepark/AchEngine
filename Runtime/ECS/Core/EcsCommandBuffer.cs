using Unity.Collections;
using Unity.Entities;

namespace AchEngine.ECS
{
    public sealed class EcsCommandBuffer : IEcsCommandBuffer
    {
        private EntityCommandBuffer _buffer;

        public EcsCommandBuffer(Allocator allocator = Allocator.TempJob)
        {
            _buffer = new EntityCommandBuffer(allocator);
        }

        public EntityCommandBuffer Buffer => _buffer;
        public bool IsCreated => _buffer.IsCreated;

        public Entity CreateEntity()
        {
            return _buffer.CreateEntity();
        }

        public Entity CreateEntity<T>(T component)
            where T : unmanaged, IComponentData
        {
            var entity = _buffer.CreateEntity();
            _buffer.AddComponent(entity, component);
            return entity;
        }

        public Entity Instantiate(Entity prefab)
        {
            return _buffer.Instantiate(prefab);
        }

        public IEcsCommandBuffer AddComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData
        {
            _buffer.AddComponent(entity, component);
            return this;
        }

        public IEcsCommandBuffer SetComponent<T>(Entity entity, T component)
            where T : unmanaged, IComponentData
        {
            _buffer.SetComponent(entity, component);
            return this;
        }

        public IEcsCommandBuffer RemoveComponent<T>(Entity entity)
            where T : unmanaged, IComponentData
        {
            _buffer.RemoveComponent<T>(entity);
            return this;
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return _buffer.AddBuffer<T>(entity);
        }

        public IEcsCommandBuffer Destroy(Entity entity)
        {
            _buffer.DestroyEntity(entity);
            return this;
        }

        public void Playback(EntityManager manager)
        {
            _buffer.Playback(manager);
        }

        public void Dispose()
        {
            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
            }
        }
    }
}
