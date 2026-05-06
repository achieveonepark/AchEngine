using System;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class AchSceneManager : IManager
    {
        public IScene Current { get; private set; }
        public string CurrentSceneName => UnitySceneManager.GetActiveScene().name;

        private bool _isLoading;

        public event Action OnSceneLoadStarted;
        public static event Action OnSceneLoadCompleted;

#if ACHENGINE_UNITASK
        public UniTask Initialize() => UniTask.CompletedTask;

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;

            if (Current != null)
                await Current.OnSceneEnd();

            OnSceneLoadStarted?.Invoke();
            await UnitySceneManager.LoadSceneAsync(sceneName).ToUniTask();
            OnSceneLoadCompleted?.Invoke();

            Current = FindSceneComponent();
            _isLoading = false;

            if (Current != null)
                await Current.OnSceneStart();
        }

        public async UniTask ReloadSceneAsync()
        {
            if (Current == null) throw new NullReferenceException("No active IScene.");
            await LoadSceneAsync(CurrentSceneName);
        }

        public async UniTask UnloadSceneAsync(string sceneName)
        {
            if (_isLoading || Current == null) return;
            _isLoading = true;
            await Current.OnSceneEnd();
            await UnitySceneManager.UnloadSceneAsync(sceneName).ToUniTask();
            Current = null;
            _isLoading = false;
        }
#else
        public Task Initialize() => Task.CompletedTask;

        public async Task LoadSceneAsync(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;

            if (Current != null)
                await Current.OnSceneEnd();

            OnSceneLoadStarted?.Invoke();

            var op = UnitySceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) await Task.Yield();

            OnSceneLoadCompleted?.Invoke();
            Current = FindSceneComponent();
            _isLoading = false;

            if (Current != null)
                await Current.OnSceneStart();
        }

        public async Task ReloadSceneAsync()
        {
            if (Current == null) throw new NullReferenceException("No active IScene.");
            await LoadSceneAsync(CurrentSceneName);
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            if (_isLoading || Current == null) return;
            _isLoading = true;
            await Current.OnSceneEnd();
            var op = UnitySceneManager.UnloadSceneAsync(sceneName);
            while (!op.isDone) await Task.Yield();
            Current = null;
            _isLoading = false;
        }
#endif

        private IScene FindSceneComponent()
        {
            var scene = UnitySceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent<IScene>(out var s))
                    return s;
            }
            Debug.LogWarning("[AchSceneManager] No IScene component found in scene root objects.");
            return null;
        }
    }
}
