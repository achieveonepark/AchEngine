using System;
using System.Threading.Tasks;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AchEngine.Managers
{
    /// <summary>
    /// 씬 로드·언로드·리로드를 비동기로 처리하고 IScene 라이프사이클을 관리하는 씬 매니저.
    /// </summary>
    public class AchSceneManager : IManager
    {
        /// <summary>
        /// 현재 활성화된 씬의 IScene 컴포넌트. 씬이 없으면 null.
        /// </summary>
        public IScene Current { get; private set; }

        /// <summary>
        /// 현재 활성화된 Unity 씬의 이름.
        /// </summary>
        public string CurrentSceneName => UnitySceneManager.GetActiveScene().name;

        private bool _isLoading;

        /// <summary>
        /// 씬 로드가 시작될 때 발생하는 이벤트.
        /// </summary>
        public event Action OnSceneLoadStarted;

        /// <summary>
        /// 씬 로드가 완료되었을 때 발생하는 정적 이벤트.
        /// </summary>
        public static event Action OnSceneLoadCompleted;

        /// <summary>
        /// 초기화. AchSceneManager는 별도 초기화 작업이 없다.
        /// </summary>
        public Task Initialize() => Task.CompletedTask;

        /// <summary>
        /// 지정한 이름의 씬을 비동기로 로드한다.
        /// 현재 씬의 OnSceneEnd를 먼저 호출하고, 로드 완료 후 새 씬의 OnSceneStart를 호출한다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름.</param>
        public async Task LoadSceneAsync(string sceneName)
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

        /// <summary>
        /// 현재 씬을 비동기로 다시 로드한다.
        /// 활성 IScene이 없으면 예외를 발생시킨다.
        /// </summary>
        public async Task ReloadSceneAsync()
        {
            if (Current == null) throw new NullReferenceException("No active IScene.");
            await LoadSceneAsync(CurrentSceneName);
        }

        /// <summary>
        /// 지정한 이름의 씬을 비동기로 언로드한다.
        /// 현재 씬의 OnSceneEnd를 먼저 호출한다.
        /// </summary>
        /// <param name="sceneName">언로드할 씬 이름.</param>
        public async Task UnloadSceneAsync(string sceneName)
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
