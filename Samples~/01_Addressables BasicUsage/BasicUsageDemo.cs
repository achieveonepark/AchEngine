using System;
using AchEngine;
using AchEngine.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.BasicUsage
{
    /// <summary>
    /// 샘플 01 - 기본 사용 흐름
    /// </summary>
    public class BasicUsageDemo : MonoBehaviour
    {
        [Header("주소 설정")]
        [Tooltip("LoadAsync<Sprite>로 로드할 스프라이트 주소")]
        public string spriteAddress = "UI/icon_star";

        [Tooltip("InstantiateAsync로 생성할 프리팹 주소")]
        public string prefabAddress = "Characters/Hero";

        [Header("실행 옵션")]
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

        private void Start()
        {
            if (runOnStart)
                RunDemoAsync();
        }

        [ContextMenu("데모 실행")]
        private void RunDemoFromContextMenu()
        {
            if (gameObject.activeInHierarchy)
                RunDemoAsync();
        }

        [ContextMenu("샘플 정리")]
        private void CleanupFromContextMenu()
        {
            CleanupLoadedObjects();
        }

        private async void RunDemoAsync()
        {
            if (_isRunning)
            {
                SetStatus("데모가 이미 실행 중입니다.");
                return;
            }

            _isRunning = true;
            CleanupLoadedObjects();

            try
            {
                SetStatus("Addressables 초기화 중...");
                await AddressableManager.InitializeAsync();

                SetStatus($"스프라이트 로드 중: {spriteAddress}");
                var sprite = await AddressableManager.LoadAsync<Sprite>(spriteAddress);
                if (targetImage != null)
                    targetImage.sprite = sprite;

                SetStatus("스프라이트 로드 완료. 같은 주소는 캐시에서 재사용됩니다.");
                await AchTask.Delay(waitBetweenSteps);

                await AddressableManager.LoadAsync<Sprite>(spriteAddress);
                SetStatus($"캐시 재사용 확인: {AddressableManager.IsLoaded(spriteAddress)}");
                await AchTask.Delay(waitBetweenSteps);

                SetStatus($"프리팹 생성 중: {prefabAddress}");
                _spawnedInstance = await AddressableManager.InstantiateAsync(prefabAddress, spawnParent);
                SetStatus($"프리팹 생성 완료: {_spawnedInstance.name}");
                await AchTask.Delay(waitBetweenSteps);

                AddressableManager.ReleaseInstance(_spawnedInstance);
                _spawnedInstance = null;
                AddressableManager.Release(prefabAddress);
                AddressableManager.Release(spriteAddress);
                SetStatus("인스턴스와 캐시 에셋을 해제했습니다.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"[오류] {exception.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void CleanupLoadedObjects()
        {
            if (_spawnedInstance != null)
            {
                AddressableManager.ReleaseInstance(_spawnedInstance);
                _spawnedInstance = null;
            }

            AddressableManager.Release(spriteAddress);
            AddressableManager.Release(prefabAddress);

            if (targetImage != null)
                targetImage.sprite = null;
        }

        private void SetStatus(string message)
        {
            Debug.Log($"[BasicUsageDemo] {message}");

            if (statusText != null)
                statusText.text = message;
        }
    }
}
