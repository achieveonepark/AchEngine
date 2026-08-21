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
        /// 초기화 시 현재 활성 씬의 IScene 컴포넌트를 탐색한다.
        /// </summary>
        public Task Initialize()
        {
            Current = FindSceneComponent();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 지정한 이름의 씬을 비동기로 로드한다.
        /// 현재 씬의 OnSceneEnd를 먼저 호출하고, 로드 완료 후 새 씬의 OnSceneStart를 호출한다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름.</param>
        public async Task LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("씬 이름은 비어 있을 수 없습니다.", nameof(sceneName));
            if (_isLoading)
                throw new InvalidOperationException("다른 씬 작업이 진행 중입니다.");

            _isLoading = true;
            try
            {
                if (Current != null)
                    await Current.OnSceneEnd();

                InvokeEventSafely(OnSceneLoadStarted, nameof(OnSceneLoadStarted));
                var operation = UnitySceneManager.LoadSceneAsync(sceneName);
                if (operation == null)
                    throw new InvalidOperationException($"씬 '{sceneName}' 로드 작업을 시작하지 못했습니다.");
                await operation.ToAchTask();

                Current = FindSceneComponent();
                if (Current != null)
                    await Current.OnSceneStart();

                InvokeEventSafely(OnSceneLoadCompleted, nameof(OnSceneLoadCompleted));
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// 현재 씬을 비동기로 다시 로드한다.
        /// 활성 IScene이 없으면 예외를 발생시킨다.
        /// </summary>
        public async Task ReloadSceneAsync()
        {
            if (Current == null)
                throw new InvalidOperationException("현재 활성 씬에 IScene 컴포넌트가 없습니다.");
            await LoadSceneAsync(CurrentSceneName);
        }

        /// <summary>
        /// 지정한 이름의 씬을 비동기로 언로드한다.
        /// 현재 씬의 OnSceneEnd를 먼저 호출한다.
        /// </summary>
        /// <param name="sceneName">언로드할 씬 이름.</param>
        public async Task UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("씬 이름은 비어 있을 수 없습니다.", nameof(sceneName));
            if (_isLoading)
                throw new InvalidOperationException("다른 씬 작업이 진행 중입니다.");

            _isLoading = true;
            try
            {
                bool unloadsCurrent = string.Equals(CurrentSceneName, sceneName, StringComparison.Ordinal);
                if (unloadsCurrent && Current != null)
                    await Current.OnSceneEnd();

                var operation = UnitySceneManager.UnloadSceneAsync(sceneName);
                if (operation == null)
                    throw new InvalidOperationException($"씬 '{sceneName}' 언로드 작업을 시작하지 못했습니다.");
                await operation.ToAchTask();

                if (unloadsCurrent)
                    Current = null;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private IScene FindSceneComponent()
        {
            var scene = UnitySceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is IScene sceneComponent)
                        return sceneComponent;
                }
            }

            // IScene은 선택적 라이프사이클 훅이다. 이를 구현하지 않는 씬도 정상적으로 로드한다.
            return null;
        }

        private static void InvokeEventSafely(Action handlers, string eventName)
        {
            if (handlers == null) return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AchSceneManager] {eventName} 구독자 실행 중 예외가 발생했습니다.");
                    Debug.LogException(e);
                }
            }
        }
    }
}
