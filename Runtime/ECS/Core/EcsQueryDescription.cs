using Unity.Entities;

namespace AchEngine.ECS
{
    public readonly struct EcsQueryDescription
    {
        public EcsQueryDescription(ComponentType[] all = null, ComponentType[] any = null, ComponentType[] none = null)
        {
            All = all ?? new ComponentType[0];
            Any = any ?? new ComponentType[0];
            None = none ?? new ComponentType[0];
        }

        public ComponentType[] All { get; }
        public ComponentType[] Any { get; }
        public ComponentType[] None { get; }

        public static EcsQueryDescription AllOf(params ComponentType[] all)
        {
            return new EcsQueryDescription(all);
        }

        public static EcsQueryDescription AnyOf(params ComponentType[] any)
        {
            return new EcsQueryDescription(any: any);
        }

        public static EcsQueryDescription NoneOf(params ComponentType[] none)
        {
            return new EcsQueryDescription(none: none);
        }

        public EcsQueryDescription WithAny(params ComponentType[] any)
        {
            return new EcsQueryDescription(All, any, None);
        }

        public EcsQueryDescription WithNone(params ComponentType[] none)
        {
            return new EcsQueryDescription(All, Any, none);
        }

        public EntityQueryDesc ToEntityQueryDesc()
        {
            return new EntityQueryDesc
            {
                All = All,
                Any = Any,
                None = None
            };
        }
    }
}
