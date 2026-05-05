using System.Collections;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.BasicUsage
{
    /// <summary>
    /// 샘플 01 - 기본 사용 흐름
    ///
    /// 초기화, 단일 에셋 로드, 같은 주소 재로드에 따른 참조 수 증가,
    /// 프리팹 인스턴스화, 수동 해제 흐름을 순서대로 보여줍니다.
    /// </summary>
    public class BasicUsageDemo : MonoBehaviour
    {
        [Header("주소 설정")]
        [Tooltip("LoadAssetAsync<Sprite>로 로드할 스프라이트 주소")]
        public string spriteAddress = "UI/icon_star";

        [Tooltip("InstantiateAsync로 생성할 프리팹 주소")]
        public string prefabAddress = "Characters/Hero";

        [Header("실행 옵션")]
        [Tooltip("주소 대신 등록 키를 사용하도록 샘플 시작 시 핸들 키를 등록합니다.")]
        public bool registerAssetHandlesBeforeRun = true;

        [Tooltip("스프라이트용 등록 키")]
        public string spriteHandleKey = "sample_sprite";

        [Tooltip("프리팹용 등록 키")]
        public string prefabHandleKey = "sample_prefab";

        [Tooltip("Start 시 자동으로 데모를 실행합니다.")]
        public bool runOnStart = true;

        [Min(0f)]
        [Tooltip("각 단계 사이의 대기 시간(초)")]
        public float waitBetweenSteps = 1f;

        [Tooltip("프리팹 인스턴스가 배치될 부모 Transform")]
        public Transform spawnParent;

        [Header("UI")]
        public Image targetImage;
        public Text statusText;

        private GameObject _spawnedInstance;
        private bool _isRunning;
        private bool _demoFailed;

        private void Start()
        {
            if (runOnStart)
                StartCoroutine(RunDemo());
        }

        [ContextMenu("데모 실행")]
        private void RunDemoFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(RunDemo());
        }

        [ContextMenu("샘플 정리")]
        private void CleanupFromContextMenu()
        {
            CleanupLoadedObjects();
        }

        private IEnumerator RunDemo()
        {
            if (_isRunning)
            {
                SetStatus("데모가 이미 실행 중입니다.");
                yield break;
            }

            _isRunning = true;
            _demoFailed = false;

            CleanupLoadedObjects();
            RegisterAssetHandlesIfNeeded();

            yield return InitializeAddressablesStep();
            if (_demoFailed)
                yield break;

            yield return LoadSpriteStep();
            if (_demoFailed)
                yield break;

            yield return WaitStep();

            yield return LoadSpriteAgainStep();
            if (_demoFailed)
                yield break;

            yield return WaitStep();

            yield return InstantiatePrefabStep();
            if (_demoFailed)
                yield break;

            yield return new WaitForSeconds(Mathf.Max(waitBetweenSteps, 0.25f) * 1.5f);

            ReleaseInstanceStep();
            yield return WaitStep();

            ReleaseSpriteHandlesStep();
            SetStatus("기본 사용 샘플이 완료되었습니다.");

            _isRunning = false;
        }

        private IEnumerator InitializeAddressablesStep()
        {
            SetStatus("Addressables 초기화 중...");

            var initHandle = AddressableManager.Instance.InitializeAsync();
            yield return initHandle;

            if (initHandle.Status != AsyncOperationStatus.Succeeded)
            {
                FailDemo("[오류] Addressables 초기화에 실패했습니다.");
                yield break;
            }

            SetStatus("Addressables 초기화 완료.");
        }

        private IEnumerator LoadSpriteStep()
        {
            if (string.IsNullOrEmpty(spriteAddress))
            {
                FailDemo("[오류] spriteAddress가 비어 있습니다.");
                yield break;
            }

            var spriteLoadKey = GetSpriteLoadKey();
            SetStatus($"스프라이트 로드 중: {spriteLoadKey}");

            var spriteHandle = AddressableManager.Instance.LoadAssetAsync<Sprite>(spriteLoadKey);
            yield return spriteHandle;

            if (spriteHandle.Status != AsyncOperationStatus.Succeeded)
            {
                FailDemo($"[오류] 스프라이트 로드 실패: {spriteLoadKey}");
                yield break;
            }

            if (targetImage != null)
                targetImage.sprite = spriteHandle.Result;

            var refCount = AddressableManager.Instance.GetReferenceCount(spriteLoadKey);
            SetStatus($"스프라이트 로드 완료. 현재 참조 수: {refCount}");
        }

        private IEnumerator LoadSpriteAgainStep()
        {
            SetStatus("같은 주소를 다시 로드해 캐시와 참조 수 증가를 확인합니다.");

            var spriteLoadKey = GetSpriteLoadKey();
            var spriteHandle = AddressableManager.Instance.LoadAssetAsync<Sprite>(spriteLoadKey);
            yield return spriteHandle;

            if (spriteHandle.Status != AsyncOperationStatus.Succeeded)
            {
                FailDemo($"[오류] 캐시된 스프라이트 재로드 실패: {spriteLoadKey}");
                yield break;
            }

            var refCount = AddressableManager.Instance.GetReferenceCount(spriteLoadKey);
            SetStatus($"같은 주소 재로드 완료. 현재 참조 수: {refCount}");
        }

        private IEnumerator InstantiatePrefabStep()
        {
            if (string.IsNullOrEmpty(prefabAddress))
            {
                SetStatus("prefabAddress가 비어 있어 프리팹 인스턴스화 단계를 건너뜁니다.");
                yield break;
            }

            var prefabLoadKey = GetPrefabLoadKey();
            SetStatus($"프리팹 인스턴스화 중: {prefabLoadKey}");

            var instHandle = spawnParent != null
                ? AddressableManager.Instance.InstantiateAsync(prefabLoadKey, spawnParent)
                : AddressableManager.Instance.InstantiateAsync(prefabLoadKey);

            yield return instHandle;

            if (instHandle.Status != AsyncOperationStatus.Succeeded || instHandle.Result == null)
            {
                FailDemo($"[오류] 프리팹 인스턴스화 실패: {prefabLoadKey}");
                yield break;
            }

            _spawnedInstance = instHandle.Result;
            SetStatus($"프리팹 인스턴스화 완료: {_spawnedInstance.name}");
        }

        private void ReleaseInstanceStep()
        {
            if (_spawnedInstance == null)
            {
                SetStatus("해제할 인스턴스가 없어 인스턴스 해제 단계를 건너뜁니다.");
                return;
            }

            AddressableManager.Instance.ReleaseInstance(_spawnedInstance);
            _spawnedInstance = null;
            SetStatus("프리팹 인스턴스를 해제했습니다.");
        }

        private void ReleaseSpriteHandlesStep()
        {
            var spriteLoadKey = GetSpriteLoadKey();
            var refCountBeforeRelease = AddressableManager.Instance.GetReferenceCount(spriteLoadKey);
            if (refCountBeforeRelease <= 0)
            {
                SetStatus("해제할 스프라이트 핸들이 없습니다.");
                _isRunning = false;
                return;
            }

            while (AddressableManager.Instance.GetReferenceCount(spriteLoadKey) > 0)
            {
                AddressableManager.Instance.Release(spriteLoadKey);
            }

            var stillLoaded = AddressableManager.Instance.IsLoaded(spriteLoadKey);
            SetStatus($"스프라이트 핸들을 모두 해제했습니다. 현재 로드 상태: {stillLoaded}");
        }

        private IEnumerator WaitStep()
        {
            if (waitBetweenSteps > 0f)
                yield return new WaitForSeconds(waitBetweenSteps);
        }

        private void CleanupLoadedObjects()
        {
            if (_spawnedInstance != null)
            {
                AddressableManager.Instance.ReleaseInstance(_spawnedInstance);
                _spawnedInstance = null;
            }

            var spriteLoadKey = GetSpriteLoadKey();
            if (!string.IsNullOrEmpty(spriteLoadKey))
            {
                while (AddressableManager.Instance != null &&
                       AddressableManager.Instance.GetReferenceCount(spriteLoadKey) > 0)
                {
                    AddressableManager.Instance.Release(spriteLoadKey);
                }
            }

            if (targetImage != null)
                targetImage.sprite = null;
        }

        private void RegisterAssetHandlesIfNeeded()
        {
            if (!registerAssetHandlesBeforeRun)
                return;

            if (!string.IsNullOrEmpty(spriteHandleKey) && !string.IsNullOrEmpty(spriteAddress))
                AddressableManager.Instance.RegisterAssetHandle(spriteHandleKey, spriteAddress);

            if (!string.IsNullOrEmpty(prefabHandleKey) && !string.IsNullOrEmpty(prefabAddress))
                AddressableManager.Instance.RegisterAssetHandle(prefabHandleKey, prefabAddress);
        }

        private string GetSpriteLoadKey()
        {
            return registerAssetHandlesBeforeRun && !string.IsNullOrEmpty(spriteHandleKey)
                ? spriteHandleKey
                : spriteAddress;
        }

        private string GetPrefabLoadKey()
        {
            return registerAssetHandlesBeforeRun && !string.IsNullOrEmpty(prefabHandleKey)
                ? prefabHandleKey
                : prefabAddress;
        }

        private void FailDemo(string message)
        {
            _demoFailed = true;
            _isRunning = false;
            SetStatus(message);
        }

        private void SetStatus(string message)
        {
            Debug.Log($"[BasicUsageDemo] {message}");

            if (statusText != null)
                statusText.text = message;
        }
    }
}
