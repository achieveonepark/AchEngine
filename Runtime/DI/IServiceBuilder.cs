using UnityEngine;

namespace AchEngine.DI
{
    /// <summary>
    /// VContainer를 직접 노출하지 않는 서비스 등록 인터페이스.
    /// AchEngineInstaller.Install() 에서 사용합니다.
    /// </summary>
    public interface IServiceBuilder
    {
        /// <summary>구체 타입을 등록합니다.</summary>
        IServiceBuilder Register<T>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where T : class;

        /// <summary>인터페이스와 구현체를 등록합니다.</summary>
        IServiceBuilder Register<TInterface, TImpl>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TImpl : class, TInterface;

        /// <summary>이미 생성된 인스턴스를 Singleton으로 등록합니다.</summary>
        IServiceBuilder RegisterInstance<T>(T instance)
            where T : class;

        /// <summary>씬 또는 계층 구조 안의 컴포넌트를 Singleton으로 등록합니다.</summary>
        IServiceBuilder RegisterComponent<T>(T component)
            where T : Component;
    }

    /// <summary>서비스 수명 주기</summary>
    public enum ServiceLifetime
    {
        Singleton,
        Transient,
        Scoped,
    }
}
