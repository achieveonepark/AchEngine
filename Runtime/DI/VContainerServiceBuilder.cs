#if ACHENGINE_VCONTAINER
using VContainer;
using UnityEngine;

namespace AchEngine.DI
{
    /// <summary>
    /// IServiceBuilder의 VContainer 구현체.
    /// AchEngineScope 내부에서만 사용됩니다.
    /// </summary>
    internal sealed class VContainerServiceBuilder : IServiceBuilder
    {
        private readonly IContainerBuilder _builder;

        public VContainerServiceBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }

        public IServiceBuilder Register<T>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : class
        {
            _builder.Register<T>(ToVContainer(lifetime));
            return this;
        }

        public IServiceBuilder Register<TInterface, TImpl>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TImpl : class, TInterface
        {
            _builder.Register<TInterface, TImpl>(ToVContainer(lifetime));
            return this;
        }

        public IServiceBuilder RegisterInstance<T>(T instance)
            where T : class
        {
            _builder.RegisterInstance(instance);
            return this;
        }

        public IServiceBuilder RegisterComponent<T>(T component)
            where T : Component
        {
            _builder.RegisterInstance(component);
            return this;
        }

        private static Lifetime ToVContainer(ServiceLifetime lifetime) => lifetime switch
        {
            ServiceLifetime.Singleton => Lifetime.Singleton,
            ServiceLifetime.Scoped    => Lifetime.Scoped,
            _                         => Lifetime.Transient,
        };
    }
}
#endif
