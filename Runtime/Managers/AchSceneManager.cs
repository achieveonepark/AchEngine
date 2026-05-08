using System;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using AchEngine.Managers.Internal;

namespace AchEngine.Managers
{
    public class AchSceneManager : IManager
    {
        public IScene Current { get; private set; }
        public string CurrentSceneName => UnitySceneManager.GetActiveScene().name;

        private bool _isLoading;

        public event Action OnSceneLoadStarted;
        public static event Action OnSceneLoadCompleted;

        public AchTask Initialize() => AchTask.CompletedTask;

        public async AchTask LoadSceneAsync(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;

            if (Current != null)
                await Current.OnSceneEnd();

            OnSceneLoadStarted?.Invoke();
            await UnitySceneManager.LoadSceneAsync(sceneName).ToAchTask();
            OnSceneLoadCompleted?.Invoke();

            Current = FindSceneComponent();
            _isLoading = false;

            if (Current != null)
                await Current.OnSceneStart();
        }

        public async AchTask ReloadSceneAsync()
        {
            if (Current == null) throw new NullReferenceException("No active IScene.");
            await LoadSceneAsync(CurrentSceneName);
        }

        public async AchTask UnloadSceneAsync(string sceneName)
        {
            if (_isLoading || Current == null) return;
            _isLoading = true;
            await Current.OnSceneEnd();
            await UnitySceneManager.UnloadSceneAsync(sceneName).ToAchTask();
            Current = null;
            _isLoading = false;
        }

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
