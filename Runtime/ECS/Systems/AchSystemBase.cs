using Unity.Collections;
using Unity.Entities;

namespace AchEngine.ECS
{
    public abstract partial class AchSystemBase : SystemBase
    {
        protected IEcsWorld Ecs => new EcsWorld(World);

        protected EcsCommandBuffer CreateCommandBuffer(Allocator allocator = Allocator.TempJob)
        {
            return new EcsCommandBuffer(allocator);
        }

        protected EntityQuery QueryAll(params ComponentType[] all)
        {
            return EntityManager.CreateEntityQuery(all);
        }

        protected EntityQuery Query(EcsQueryDescription description)
        {
            return EntityManager.CreateEntityQuery(description.ToEntityQueryDesc());
        }
    }
}
