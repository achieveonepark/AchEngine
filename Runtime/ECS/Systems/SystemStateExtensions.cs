using Unity.Collections;
using Unity.Entities;

namespace AchEngine.ECS
{
    public static class SystemStateExtensions
    {
        public static EcsCommandBuffer CreateCommandBuffer(this ref SystemState state, Allocator allocator = Allocator.TempJob)
        {
            return new EcsCommandBuffer(allocator);
        }

        public static EntityQuery QueryAll(this ref SystemState state, params ComponentType[] all)
        {
            return state.EntityManager.CreateEntityQuery(all);
        }

        public static EntityQuery Query(this ref SystemState state, EcsQueryDescription description)
        {
            return state.EntityManager.CreateEntityQuery(description.ToEntityQueryDesc());
        }
    }
}
