#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace AchEngine.Assets
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;
    
    public class RemoteContentManager
    {
        private static RemoteContentManager _instance;
        public static RemoteContentManager Instance => _instance ??= new RemoteContentManager();

        /// <summary>
        /// 지정한 라벨을 가진 에셋의 다운로드 크기를 확인합니다.
        /// </summary>
        public AsyncOperationHandle<long> GetDownloadSizeAsync(string label)
        {
            return Addressables.GetDownloadSizeAsync(label);
        }

        /// <summary>
        /// 지정한 키와 일치하는 에셋의 다운로드 크기를 확인합니다.
        /// </summary>
        public AsyncOperationHandle<long> GetDownloadSizeAsync(IEnumerable<string> keys)
        {
            return Addressables.GetDownloadSizeAsync((IEnumerable)keys);
        }

        /// <summary>
        /// 지정한 라벨의 종속성을 다운로드하며, 필요하면 진행률 콜백을 전달할 수 있습니다.
        /// </summary>
        public AsyncOperationHandle DownloadDependenciesAsync(string label, Action<DownloadProgress> onProgress = null)
        {
            var handle = Addressables.DownloadDependenciesAsync(label);

            if (onProgress != null)
            {
                CoroutineRunner.Run(TrackProgress(handle, onProgress));
            }

            return handle;
        }

        /// <summary>
        /// 지정한 키의 종속성을 다운로드하며, 필요하면 진행률 콜백을 전달할 수 있습니다.
        /// </summary>
        public AsyncOperationHandle DownloadDependenciesAsync(
            IEnumerable<string> keys,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Union,
            Action<DownloadProgress> onProgress = null)
        {
            var handle = Addressables.DownloadDependenciesAsync((IEnumerable)keys, mergeMode);

            if (onProgress != null)
            {
                CoroutineRunner.Run(TrackProgress(handle, onProgress));
            }

            return handle;
        }

        /// <summary>
        /// 사용 가능한 카탈로그 업데이트가 있는지 확인합니다.
        /// </summary>
        public AsyncOperationHandle<List<string>> CheckForCatalogUpdatesAsync()
        {
            return Addressables.CheckForCatalogUpdates();
        }

        /// <summary>
        /// 카탈로그를 업데이트합니다. 수정된 모든 카탈로그를 업데이트하려면 null을 전달합니다.
        /// </summary>
        public AsyncOperationHandle<List<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>> UpdateCatalogsAsync(
            IEnumerable<string> catalogs = null)
        {
            return Addressables.UpdateCatalogs(catalogs);
        }

        private static IEnumerator TrackProgress(AsyncOperationHandle handle, Action<DownloadProgress> onProgress)
        {
            while (!handle.IsDone)
            {
                var status = handle.GetDownloadStatus();
                onProgress.Invoke(new DownloadProgress(
                    totalBytes: status.TotalBytes,
                    downloadedBytes: status.DownloadedBytes,
                    percent: status.Percent,
                    status: DownloadStatus.Downloading
                ));
                yield return null;
            }

            var finalStatus = handle.Status == AsyncOperationStatus.Succeeded
                ? DownloadStatus.Complete
                : DownloadStatus.Failed;

            var finalDownloadStatus = handle.GetDownloadStatus();
            onProgress.Invoke(new DownloadProgress(
                totalBytes: finalDownloadStatus.TotalBytes,
                downloadedBytes: finalDownloadStatus.DownloadedBytes,
                percent: finalDownloadStatus.Percent,
                status: finalStatus
            ));
        }
    }

    /// <summary>
    /// MonoBehaviour가 아닌 클래스에서 코루틴을 실행하기 위한 도우미 MonoBehaviour입니다.
    /// </summary>
    internal class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        private static CoroutineRunner GetInstance()
        {
            if (_instance == null)
            {
                var go = new GameObject("[CoroutineRunner]");
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CoroutineRunner>();
            }
            return _instance;
        }

        public static Coroutine Run(IEnumerator coroutine)
        {
            return GetInstance().StartCoroutine(coroutine);
        }
    }
}
#endif
