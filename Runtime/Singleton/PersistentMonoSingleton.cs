using UnityEngine;

namespace AchEngine
{
    public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
    }
}
