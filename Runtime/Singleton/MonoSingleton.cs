using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// MonoBehaviour 기반 싱글턴 베이스 클래스.
    /// 씬에 인스턴스가 없으면 자동으로 새 GameObject를 생성합니다.
    /// </summary>
    /// <typeparam name="T">싱글턴으로 사용할 구체 MonoBehaviour 타입.</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {
        private static T instance;
        private SingletonInitializationStatus initializationStatus = SingletonInitializationStatus.None;

        /// <summary>
        /// 싱글턴 인스턴스를 반환합니다.
        /// 씬에 없으면 새 GameObject를 생성하여 컴포넌트를 추가합니다.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        var obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                        instance.OnMonoSingletonCreated();
                    }
                }
                return instance;
            }
        }

        /// <summary>싱글턴 초기화가 완료되었는지 여부.</summary>
        public virtual bool IsInitialized => initializationStatus == SingletonInitializationStatus.Initialized;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                InitializeSingleton();
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        /// <summary>Instance 프로퍼티에서 새 GameObject를 생성했을 때 호출됩니다. 서브클래스에서 재정의 가능합니다.</summary>
        protected virtual void OnMonoSingletonCreated() { }

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
