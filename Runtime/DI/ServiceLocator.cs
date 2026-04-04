using System;

namespace AchEngine.DI
{
    /// <summary>
    /// [Inject] 없이 서비스를 접근할 수 있는 정적 서비스 로케이터.
    /// AchEngineScope 초기화 후 자동으로 설정됩니다.
    ///
    /// 주의: [Inject] 를 사용할 수 있는 경우 해당 방식을 우선합니다.
    ///       이 클래스는 MonoBehaviour 등 [Inject]가 불가능한 환경을 위한 보조 수단입니다.
    /// </summary>
    public static class ServiceLocator
    {
        private static Func<Type, object> _resolver;

        /// <summary>ServiceLocator가 사용 가능한 상태인지 확인합니다.</summary>
        public static bool IsReady => _resolver != null;

        internal static void Setup(Func<Type, object> resolver) => _resolver = resolver;

        internal static void Reset() => _resolver = null;

        /// <summary>
        /// 등록된 서비스를 반환합니다.
        /// AchEngineScope가 초기화되지 않은 경우 예외를 던집니다.
        /// </summary>
        public static T Resolve<T>()
        {
            if (_resolver == null)
                throw new InvalidOperationException(
                    "[ServiceLocator] AchEngineScope가 초기화되지 않았습니다.");
            return (T)_resolver(typeof(T));
        }

        /// <summary>
        /// 등록된 서비스를 안전하게 가져옵니다.
        /// 실패 시 false를 반환하며 예외를 던지지 않습니다.
        /// </summary>
        public static bool TryResolve<T>(out T result)
        {
            if (_resolver == null)
            {
                result = default;
                return false;
            }

            try
            {
                result = (T)_resolver(typeof(T));
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
