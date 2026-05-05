using System.Collections;
using System.Collections.Generic;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.RemoteContent
{
    /// <summary>
    /// 샘플 03 - 원격 콘텐츠 다운로드
    ///
    /// 원격 라벨의 다운로드 크기 확인, 다운로드 진행률 표시,
    /// 카탈로그 업데이트 확인/적용 흐름을 보여줍니다.
    /// </summary>
    public class RemoteContentDemo : MonoBehaviour
    {
        [Header("원격 콘텐츠 대상")]
        [Tooltip("다운로드 크기와 종속성 다운로드를 확인할 Addressables 라벨")]
        public string remoteLabel = "DLC_Pack1";

        [Header("실행 옵션")]
        [Tooltip("시작 시 Addressables 초기화를 먼저 수행합니다.")]
        public bool initializeOnStart = true;

        [Tooltip("시작 시 다운로드 크기 확인을 자동으로 수행합니다.")]
        public bool checkDownloadSizeOnStart;

        [Tooltip("시작 시 카탈로그 업데이트 확인을 자동으로 수행합니다.")]
        public bool checkCatalogUpdatesOnStart;

        [Header("UI")]
        public Text statusText;
        public Text downloadSizeText;
        public Text catalogStatusText;
        public Button checkSizeButton;
        public Button downloadButton;
        public Button checkCatalogButton;
        public Button updateCatalogButton;
        public DownloadProgressUI progressUI;

        private readonly List<string> _catalogsToUpdate = new();
        private bool _isBusy;
        private long _lastKnownDownloadSizeBytes;

        private void Start()
        {
            if (checkSizeButton != null)
                checkSizeButton.onClick.AddListener(OnCheckDownloadSize);

            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadDependencies);

            if (checkCatalogButton != null)
                checkCatalogButton.onClick.AddListener(OnCheckCatalogUpdates);

            if (updateCatalogButton != null)
                updateCatalogButton.onClick.AddListener(OnUpdateCatalogs);

            if (progressUI != null)
                progressUI.ResetView("다운로드 대기 중");

            if (downloadSizeText != null)
                downloadSizeText.text = "다운로드 크기: 확인 전";

            if (catalogStatusText != null)
                catalogStatusText.text = "카탈로그 상태: 확인 전";

            SetStatus("원격 콘텐츠 샘플 준비 완료");
            UpdateButtonState();

            if (initializeOnStart)
                StartCoroutine(InitializeAndRunStartupChecks());
        }

        [ContextMenu("다운로드 크기 확인")]
        private void CheckDownloadSizeFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(CheckDownloadSizeRoutine());
        }

        [ContextMenu("원격 콘텐츠 다운로드")]
        private void DownloadDependenciesFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(DownloadDependenciesRoutine());
        }

        [ContextMenu("카탈로그 업데이트 확인")]
        private void CheckCatalogUpdatesFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(CheckCatalogUpdatesRoutine());
        }

        [ContextMenu("카탈로그 업데이트 적용")]
        private void UpdateCatalogsFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(UpdateCatalogsRoutine());
        }

        private IEnumerator InitializeAndRunStartupChecks()
        {
            yield return EnsureInitialized();

            if (checkDownloadSizeOnStart)
                yield return CheckDownloadSizeRoutine();

            if (checkCatalogUpdatesOnStart)
                yield return CheckCatalogUpdatesRoutine();
        }

        private void OnCheckDownloadSize()
        {
            if (!_isBusy)
                StartCoroutine(CheckDownloadSizeRoutine());
        }

        private IEnumerator CheckDownloadSizeRoutine()
        {
            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            if (string.IsNullOrEmpty(remoteLabel))
            {
                SetStatus("[오류] remoteLabel이 비어 있습니다.");
                SetBusy(false);
                yield break;
            }

            SetStatus($"다운로드 크기 확인 중: {remoteLabel}");

            var sizeHandle = AddressableManager.Instance.RemoteContent.GetDownloadSizeAsync(remoteLabel);
            yield return sizeHandle;

            if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus($"[오류] 다운로드 크기 확인 실패: {remoteLabel}");
                SetBusy(false);
                yield break;
            }

            _lastKnownDownloadSizeBytes = sizeHandle.Result;

            if (downloadSizeText != null)
            {
                downloadSizeText.text = $"다운로드 크기: {DownloadProgressUI.FormatBytes(_lastKnownDownloadSizeBytes)}";
            }

            if (_lastKnownDownloadSizeBytes > 0)
                SetStatus("다운로드할 원격 콘텐츠가 있습니다.");
            else
                SetStatus("추가로 다운로드할 원격 콘텐츠가 없습니다.");

            SetBusy(false);
        }

        private void OnDownloadDependencies()
        {
            if (!_isBusy)
                StartCoroutine(DownloadDependenciesRoutine());
        }

        private IEnumerator DownloadDependenciesRoutine()
        {
            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            if (string.IsNullOrEmpty(remoteLabel))
            {
                SetStatus("[오류] remoteLabel이 비어 있습니다.");
                SetBusy(false);
                yield break;
            }

            if (progressUI != null)
                progressUI.ResetView("다운로드 시작");

            SetStatus($"원격 콘텐츠 다운로드 중: {remoteLabel}");

            var downloadHandle = AddressableManager.Instance.RemoteContent.DownloadDependenciesAsync(
                remoteLabel,
                OnDownloadProgress);

            yield return downloadHandle;

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus($"[오류] 원격 콘텐츠 다운로드 실패: {remoteLabel}");

                if (progressUI != null)
                    progressUI.ShowMessage("다운로드 실패");

                SetBusy(false);
                yield break;
            }

            _lastKnownDownloadSizeBytes = 0;

            if (downloadSizeText != null)
                downloadSizeText.text = "다운로드 크기: 0 B";

            if (progressUI != null)
                progressUI.ShowMessage("다운로드 완료");

            SetStatus("원격 콘텐츠 다운로드가 완료되었습니다.");
            SetBusy(false);
        }

        private void OnCheckCatalogUpdates()
        {
            if (!_isBusy)
                StartCoroutine(CheckCatalogUpdatesRoutine());
        }

        private IEnumerator CheckCatalogUpdatesRoutine()
        {
            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            SetStatus("카탈로그 업데이트 확인 중...");

            var catalogHandle = AddressableManager.Instance.RemoteContent.CheckForCatalogUpdatesAsync();
            yield return catalogHandle;

            if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus("[오류] 카탈로그 업데이트 확인 실패");
                SetBusy(false);
                yield break;
            }

            _catalogsToUpdate.Clear();
            _catalogsToUpdate.AddRange(catalogHandle.Result);

            if (catalogStatusText != null)
                catalogStatusText.text = $"카탈로그 상태: {_catalogsToUpdate.Count}개 업데이트 가능";

            SetStatus(_catalogsToUpdate.Count > 0
                ? "업데이트 가능한 카탈로그가 있습니다."
                : "업데이트할 카탈로그가 없습니다.");

            SetBusy(false);
        }

        private void OnUpdateCatalogs()
        {
            if (!_isBusy)
                StartCoroutine(UpdateCatalogsRoutine());
        }

        private IEnumerator UpdateCatalogsRoutine()
        {
            SetBusy(true);
            yield return EnsureInitialized();

            if (!AddressableManager.Instance.IsInitialized)
            {
                SetBusy(false);
                yield break;
            }

            if (_catalogsToUpdate.Count == 0)
            {
                SetStatus("먼저 카탈로그 업데이트 확인을 실행합니다.");
                yield return CheckCatalogUpdatesRoutine();

                if (_catalogsToUpdate.Count == 0)
                {
                    SetBusy(false);
                    yield break;
                }

                SetBusy(true);
            }

            SetStatus($"카탈로그 업데이트 적용 중... ({_catalogsToUpdate.Count}개)");

            var updateHandle = AddressableManager.Instance.RemoteContent.UpdateCatalogsAsync(_catalogsToUpdate);
            yield return updateHandle;

            if (updateHandle.Status != AsyncOperationStatus.Succeeded)
            {
                SetStatus("[오류] 카탈로그 업데이트 적용 실패");
                SetBusy(false);
                yield break;
            }

            _catalogsToUpdate.Clear();

            if (catalogStatusText != null)
                catalogStatusText.text = "카탈로그 상태: 최신 상태";

            SetStatus("카탈로그 업데이트 적용이 완료되었습니다.");
            SetBusy(false);
        }

        private IEnumerator EnsureInitialized()
        {
            var initHandle = AddressableManager.Instance.InitializeAsync();
            yield return initHandle;

            if (initHandle.Status != AsyncOperationStatus.Succeeded)
                SetStatus("[오류] Addressables 초기화에 실패했습니다.");
        }

        private void OnDownloadProgress(DownloadProgress progress)
        {
            if (progressUI != null)
                progressUI.Apply(progress);
        }

        private void SetBusy(bool value)
        {
            _isBusy = value;
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (checkSizeButton != null)
                checkSizeButton.interactable = !_isBusy;

            if (downloadButton != null)
                downloadButton.interactable = !_isBusy && _lastKnownDownloadSizeBytes > 0;

            if (checkCatalogButton != null)
                checkCatalogButton.interactable = !_isBusy;

            if (updateCatalogButton != null)
                updateCatalogButton.interactable = !_isBusy && _catalogsToUpdate.Count > 0;
        }

        private void SetStatus(string message)
        {
            Debug.Log($"[RemoteContentDemo] {message}");

            if (statusText != null)
                statusText.text = message;
        }
    }
}
