using System.Collections;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.SceneManagement
{
    /// <summary>
    /// 샘플 02 - 씬 관리
    ///
    /// Addressable 씬을 로드한 뒤 해당 씬을 활성 씬으로 전환하고,
    /// 그 컨텍스트에서 에셋을 로드해 씬 언로드 시 자동 해제가 어떻게 동작하는지 보여줍니다.
    /// </summary>
    public class SceneManagementDemo : MonoBehaviour
    {
        [Header("씬 설정")]
        [Tooltip("Addressable로 마킹된 씬 주소")]
        public string sceneAddress = "Scenes/GameLevel";

        [Tooltip("로드한 씬이 활성 씬일 때 함께 로드해 볼 에셋 주소")]
        public string assetInSceneAddress = "Props/Barrel";

        [Header("실행 옵션")]
        [Tooltip("Start 시 자동으로 씬 로드를 시작합니다.")]
        public bool autoLoadOnStart;

        [Tooltip("씬 로드 후 에셋을 로드하기 전에 한 프레임 더 기다립니다.")]
        public bool waitOneExtraFrameBeforeLoadingAsset = true;

        [Header("UI")]
        public Text statusText;
        public Button loadSceneBtn;
        public Button unloadSceneBtn;

        private bool _sceneLoaded;
        private bool _isBusy;

        private void Start()
        {
            if (loadSceneBtn != null)
                loadSceneBtn.onClick.AddListener(OnLoadScene);

            if (unloadSceneBtn != null)
                unloadSceneBtn.onClick.AddListener(OnUnloadScene);

            UpdateButtonState();

            if (autoLoadOnStart)
                StartCoroutine(LoadSceneRoutine());
        }

        [ContextMenu("씬 로드")]
        private void LoadSceneFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(LoadSceneRoutine());
        }

        [ContextMenu("씬 언로드")]
        private void UnloadSceneFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(UnloadSceneRoutine());
        }

        private void OnLoadScene()
        {
            if (!_isBusy)
                StartCoroutine(LoadSceneRoutine());
        }

        private IEnumerator LoadSceneRoutine()
        {
            if (_sceneLoaded)
            {
                SetStatus("씬이 이미 로드되어 있습니다.");
                yield break;
            }

            if (string.IsNullOrEmpty(sceneAddress))
            {
                SetStatus("[오류] sceneAddress가 비어 있습니다.");
                yield break;
            }

            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            SetStatus($"씬 로드 중: {sceneAddress}");

            var loadHandle = AddressableManager.Instance.LoadSceneAsync(
                sceneAddress,
                LoadSceneMode.Additive,
                activateOnLoad: true);

            yield return loadHandle;

            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus($"[오류] 씬 로드 실패: {sceneAddress}");
                SetBusy(false);
                yield break;
            }

            _sceneLoaded = true;

            var loadedScene = loadHandle.Result.Scene;
            var previousActiveScene = SceneManager.GetActiveScene();

            if (loadedScene.IsValid() && loadedScene.isLoaded)
                SceneManager.SetActiveScene(loadedScene);

            if (waitOneExtraFrameBeforeLoadingAsset)
                yield return null;

            SetStatus($"씬 로드 완료: {sceneAddress}\n이제 같은 씬 컨텍스트에서 에셋을 로드합니다.");
            UpdateButtonState();

            yield return LoadAssetInLoadedSceneContext();

            if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                SceneManager.SetActiveScene(previousActiveScene);

            SetBusy(false);
        }

        private IEnumerator LoadAssetInLoadedSceneContext()
        {
            if (string.IsNullOrEmpty(assetInSceneAddress))
            {
                SetStatus("assetInSceneAddress가 비어 있어 에셋 로드 단계를 건너뜁니다.");
                yield break;
            }

            SetStatus($"씬 컨텍스트 에셋 로드 중: {assetInSceneAddress}");

            var assetHandle = AddressableManager.Instance.LoadAssetAsync<GameObject>(assetInSceneAddress);
            yield return assetHandle;

            if (assetHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus($"[경고] 에셋 로드 실패: {assetInSceneAddress}\n씬 언로드 데모는 계속 진행할 수 있습니다.");
                yield break;
            }

            var refCount = AddressableManager.Instance.GetReferenceCount(assetInSceneAddress);
            SetStatus(
                $"씬 컨텍스트 에셋 로드 완료: {assetInSceneAddress}\n" +
                $"현재 참조 수: {refCount}\n" +
                "이 핸들은 씬 언로드 시 자동으로 해제되어야 합니다.");
        }

        private void OnUnloadScene()
        {
            if (!_isBusy)
                StartCoroutine(UnloadSceneRoutine());
        }

        private IEnumerator UnloadSceneRoutine()
        {
            if (!_sceneLoaded)
            {
                SetStatus("언로드할 씬이 없습니다.");
                yield break;
            }

            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            var assetWasLoaded = !string.IsNullOrEmpty(assetInSceneAddress) &&
                                 AddressableManager.Instance.IsLoaded(assetInSceneAddress);

            SetStatus(
                $"씬 언로드 중: {sceneAddress}\n" +
                $"언로드 전 씬 컨텍스트 에셋 로드 여부: {assetWasLoaded}");

            var unloadHandle = AddressableManager.Instance.UnloadSceneAsync(sceneAddress);
            if (!unloadHandle.IsValid())
            {
                SetStatus("[오류] UnloadSceneAsync가 유효한 핸들을 반환하지 않았습니다.");
                SetBusy(false);
                yield break;
            }

            yield return unloadHandle;

            _sceneLoaded = false;
            UpdateButtonState();

            var assetStillLoaded = !string.IsNullOrEmpty(assetInSceneAddress) &&
                                   AddressableManager.Instance.IsLoaded(assetInSceneAddress);

            SetStatus(
                $"씬 언로드 완료: {sceneAddress}\n" +
                $"씬 컨텍스트 에셋이 남아 있는가: {assetStillLoaded}\n" +
                "(기대값: False)");

            Debug.Log($"[SceneManagementDemo] 씬 언로드 후 에셋 로드 상태: {assetStillLoaded}");
            SetBusy(false);
        }

        private IEnumerator EnsureInitialized()
        {
            var initHandle = AddressableManager.Instance.InitializeAsync();
            yield return initHandle;

            if (initHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus("[오류] Addressables 초기화에 실패했습니다.");
            }
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            UpdateButtonState();
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
