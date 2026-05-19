using UnityEngine;

namespace AchEngine
{
    /// <summary>
    /// 씬 전환 시에도 파괴되지 않는 영속 MonoBehaviour 싱글턴 베이스 클래스.
    /// 초기화 완료 시 자동으로 DontDestroyOnLoad를 적용합니다.
    /// </summary>
    /// <typeparam name="T">싱글턴으로 사용할 구체 MonoBehaviour 타입.</typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
    {
        /// <summary>초기화 완료 시 DontDestroyOnLoad를 적용하여 씬 전환 후에도 인스턴스를 유지합니다.</summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
    }
}
