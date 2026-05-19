namespace AchEngine
{
    /// <summary>싱글턴 인스턴스의 초기화 상태를 나타내는 열거형.</summary>
    public enum SingletonInitializationStatus
    {
        /// <summary>초기화되지 않은 상태.</summary>
        None,
        /// <summary>초기화가 완료된 상태.</summary>
        Initialized
    }

    /// <summary>
    /// 순수 C# 클래스용 스레드 안전 싱글턴 베이스 클래스.
    /// double-checked locking 패턴으로 인스턴스를 생성합니다.
    /// </summary>
    /// <typeparam name="T">싱글턴으로 사용할 구체 클래스 타입.</typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        private static T instance;
        private SingletonInitializationStatus initializationStatus = SingletonInitializationStatus.None;

        /// <summary>
        /// 싱글턴 인스턴스를 반환합니다.
        /// 인스턴스가 없으면 스레드 안전하게 새 인스턴스를 생성합니다.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(T))
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            instance.InitializeSingleton();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>싱글턴 초기화가 완료되었는지 여부.</summary>
        public virtual bool IsInitialized => initializationStatus == SingletonInitializationStatus.Initialized;

        /// <summary>초기화 완료 시 호출됩니다. 서브클래스에서 재정의하여 추가 초기화를 수행합니다.</summary>
        protected virtual void OnInitialized() { }

        /// <summary>
        /// 싱글턴 초기화를 수행합니다. 중복 호출을 방지하며 OnInitialized를 실행합니다.
        /// </summary>
        public virtual void InitializeSingleton()
        {
            if (initializationStatus != SingletonInitializationStatus.None)
                return;
            initializationStatus = SingletonInitializationStatus.Initialized;
            OnInitialized();
        }

        /// <summary>싱글턴 정리 작업을 수행합니다. 서브클래스에서 재정의하여 리소스를 해제합니다.</summary>
        public virtual void ClearSingleton() { }

        /// <summary>기존 인스턴스를 파괴하고 새 인스턴스를 생성합니다.</summary>
        public static void CreateInstance()
        {
            DestroyInstance();
            instance = Instance;
        }

        /// <summary>현재 인스턴스를 정리하고 참조를 해제합니다.</summary>
        public static void DestroyInstance()
        {
            if (instance == null)
                return;
            instance.ClearSingleton();
            instance = default;
        }
    }
}
