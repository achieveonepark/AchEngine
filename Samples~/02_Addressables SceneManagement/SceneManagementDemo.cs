using System;
using AchEngine;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.SceneManagement
{
    /// <summary>
    /// 샘플 02 - 씬 관리
    /// </summary>
    public class SceneManagementDemo : MonoBehaviour
    {
        [Header("씬 설정")]
        [Tooltip("Addressable로 마킹된 씬 주소")]
        public string sceneAddress = "Scenes/GameLevel";

        [Tooltip("로드한 씬에서 함께 로드해 볼 에셋 주소")]
        public string assetInSceneAddress = "Props/Barrel";

        [Header("실행 옵션")]
        [Tooltip("Start 시 자동으로 씬 로드를 시작합니다.")]
        public bool autoLoadOnStart;

        [Header("UI")]
        public Text statusText;
        public Button loadSceneBtn;
        public Button unloadSceneBtn;

        private bool _sceneLoaded;
        private bool _isBusy;

        private void Start()
        {
            if (loadSceneBtn != null)
                loadSceneBtn.onClick.AddListener(LoadSceneAsync);

            if (unloadSceneBtn != null)
                unloadSceneBtn.onClick.AddListener(UnloadSceneAsync);

            UpdateButtonState();

            if (autoLoadOnStart)
                LoadSceneAsync();
        }

        [ContextMenu("씬 로드")]
        private void LoadSceneFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                LoadSceneAsync();
        }

        [ContextMenu("씬 언로드")]
        private void UnloadSceneFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                UnloadSceneAsync();
        }

        private async void LoadSceneAsync()
        {
            if (_isBusy || _sceneLoaded)
                return;

            if (string.IsNullOrWhiteSpace(sceneAddress))
            {
                SetStatus("[오류] sceneAddress가 비어 있습니다.");
                return;
            }

            _isBusy = true;
            UpdateButtonState();

            try
            {
                SetStatus($"씬 로드 중: {sceneAddress}");
                await AddressableManager.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive);
                _sceneLoaded = true;

                if (!string.IsNullOrWhiteSpace(assetInSceneAddress))
                {
                    SetStatus($"에셋 로드 중: {assetInSceneAddress}");
                    await AddressableManager.LoadAsync<GameObject>(assetInSceneAddress);
                    SetStatus($"에셋 캐시 상태: {AddressableManager.IsLoaded(assetInSceneAddress)}");
                }
                else
                {
                    SetStatus("씬 로드 완료");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                _isBusy = false;
                UpdateButtonState();
            }
        }

        private async void UnloadSceneAsync()
        {
            if (_isBusy || !_sceneLoaded)
                return;

            _isBusy = true;
            UpdateButtonState();

            try
            {
                AddressableManager.Release(assetInSceneAddress);
                var unloaded = await AddressableManager.UnloadSceneAsync(sceneAddress);
                _sceneLoaded = !unloaded;
                SetStatus(unloaded ? "씬과 씬 에셋 캐시를 해제했습니다." : "관리 중인 씬을 찾을 수 없습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                _isBusy = false;
                UpdateButtonState();
            }
        }

        private void UpdateButtonState()
        {
            if (loadSceneBtn != null)
                loadSceneBtn.interactable = !_isBusy && !_sceneLoaded;

            if (unloadSceneBtn != null)
                unloadSceneBtn.interactable = !_isBusy && _sceneLoaded;
        }

        private void SetStatus(string message)
        {
            Debug.Log($"[SceneManagementDemo] {message}");

            if (statusText != null)
                statusText.text = message;
        }
    }
}
