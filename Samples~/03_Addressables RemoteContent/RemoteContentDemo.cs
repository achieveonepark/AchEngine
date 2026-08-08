using System;
using System.Collections.Generic;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.RemoteContent
{
    /// <summary>
    /// 샘플 03 - 원격 콘텐츠 다운로드
    /// </summary>
    public class RemoteContentDemo : MonoBehaviour
    {
        [Header("원격 콘텐츠 대상")]
        [Tooltip("다운로드 크기와 종속성 다운로드를 확인할 Addressables 라벨")]
        public string remoteLabel = "DLC_Pack1";

        [Header("실행 옵션")]
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
                checkSizeButton.onClick.AddListener(CheckDownloadSizeAsync);

            if (downloadButton != null)
                downloadButton.onClick.AddListener(DownloadDependenciesAsync);

            if (checkCatalogButton != null)
                checkCatalogButton.onClick.AddListener(CheckCatalogUpdatesAsync);

            if (updateCatalogButton != null)
                updateCatalogButton.onClick.AddListener(UpdateCatalogsAsync);

            if (progressUI != null)
                progressUI.ResetView("다운로드 대기 중");

            if (downloadSizeText != null)
                downloadSizeText.text = "다운로드 크기: 확인 전";

            if (catalogStatusText != null)
                catalogStatusText.text = "카탈로그 상태: 확인 전";

            UpdateButtonState();

            if (checkDownloadSizeOnStart)
                CheckDownloadSizeAsync();
            else if (checkCatalogUpdatesOnStart)
                CheckCatalogUpdatesAsync();
        }

        [ContextMenu("다운로드 크기 확인")]
        private void CheckDownloadSizeFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                CheckDownloadSizeAsync();
        }

        [ContextMenu("원격 콘텐츠 다운로드")]
        private void DownloadDependenciesFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                DownloadDependenciesAsync();
        }

        [ContextMenu("카탈로그 업데이트 확인")]
        private void CheckCatalogUpdatesFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                CheckCatalogUpdatesAsync();
        }

        [ContextMenu("카탈로그 업데이트 적용")]
        private void UpdateCatalogsFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                UpdateCatalogsAsync();
        }

        private async void CheckDownloadSizeAsync()
        {
            if (!BeginOperation())
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(remoteLabel))
                    throw new InvalidOperationException("remoteLabel이 비어 있습니다.");

                SetStatus($"다운로드 크기 확인 중: {remoteLabel}");
                _lastKnownDownloadSizeBytes = await AddressableManager.GetDownloadSizeAsync(remoteLabel);

                if (downloadSizeText != null)
                    downloadSizeText.text = $"다운로드 크기: {DownloadProgressUI.FormatBytes(_lastKnownDownloadSizeBytes)}";

                SetStatus(_lastKnownDownloadSizeBytes > 0
                    ? "다운로드할 원격 콘텐츠가 있습니다."
                    : "추가로 다운로드할 원격 콘텐츠가 없습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                EndOperation();
            }
        }

        private async void DownloadDependenciesAsync()
        {
            if (!BeginOperation())
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(remoteLabel))
                    throw new InvalidOperationException("remoteLabel이 비어 있습니다.");

                progressUI?.ResetView("다운로드 시작");
                SetStatus($"원격 콘텐츠 다운로드 중: {remoteLabel}");
                await AddressableManager.DownloadDependenciesAsync(remoteLabel, OnDownloadProgress);

                _lastKnownDownloadSizeBytes = 0;
                if (downloadSizeText != null)
                    downloadSizeText.text = "다운로드 크기: 0 B";

                progressUI?.ShowMessage("다운로드 완료");
                SetStatus("원격 콘텐츠 다운로드가 완료되었습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                progressUI?.ShowMessage("다운로드 실패");
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                EndOperation();
            }
        }

        private async void CheckCatalogUpdatesAsync()
        {
            if (!BeginOperation())
                return;

            try
            {
                SetStatus("카탈로그 업데이트 확인 중...");
                var catalogs = await AddressableManager.CheckForCatalogUpdatesAsync();
                _catalogsToUpdate.Clear();
                _catalogsToUpdate.AddRange(catalogs);

                if (catalogStatusText != null)
                    catalogStatusText.text = $"카탈로그 상태: {_catalogsToUpdate.Count}개 업데이트 가능";

                SetStatus(_catalogsToUpdate.Count > 0
                    ? "업데이트 가능한 카탈로그가 있습니다."
                    : "업데이트할 카탈로그가 없습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                EndOperation();
            }
        }

        private async void UpdateCatalogsAsync()
        {
            if (!BeginOperation())
                return;

            try
            {
                if (_catalogsToUpdate.Count == 0)
                {
                    SetStatus("카탈로그 업데이트 확인 중...");
                    _catalogsToUpdate.AddRange(await AddressableManager.CheckForCatalogUpdatesAsync());
                }

                if (_catalogsToUpdate.Count == 0)
                {
                    SetStatus("업데이트할 카탈로그가 없습니다.");
                    return;
                }

                SetStatus($"카탈로그 업데이트 적용 중... ({_catalogsToUpdate.Count}개)");
                await AddressableManager.UpdateCatalogsAsync(_catalogsToUpdate);
                _catalogsToUpdate.Clear();

                if (catalogStatusText != null)
                    catalogStatusText.text = "카탈로그 상태: 최신 상태";

                SetStatus("카탈로그 업데이트 적용이 완료되었습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                EndOperation();
            }
        }

        private bool BeginOperation()
        {
            if (_isBusy)
                return false;

            _isBusy = true;
            UpdateButtonState();
            return true;
        }

        private void EndOperation()
        {
            _isBusy = false;
            UpdateButtonState();
        }

        private void OnDownloadProgress(DownloadProgress progress)
        {
            if (progressUI != null)
                progressUI.Apply(progress);
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
