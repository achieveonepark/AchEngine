#if ACHENGINE_ADDRESSABLES
using UnityEngine;

namespace AchEngine.Assets
{
    public enum CloudProvider
    {
        None,
        AWSS3,
        GoogleCloudStorage,
        Custom
    }

    public class AddressableManagerSettings : ScriptableObject
    {
        private const string SettingsPath = "AddressableManagerSettings";

        [Header("원격 설정")]
        [Tooltip("원격 번들에 사용할 클라우드 스토리지 제공업체")]
        public CloudProvider cloudProvider = CloudProvider.None;

        [Tooltip("버킷 또는 컨테이너 이름")]
        public string bucketName;

        [Tooltip("리전(AWS) 또는 위치(GCS)")]
        public string bucketRegion;

        [Tooltip("원격 카탈로그를 불러올 전체 URL입니다. 비워 두면 클라우드 제공업체 설정으로 자동 생성된 URL을 사용합니다.")]
        public string remoteCatalogUrl;

        [Tooltip("원격 번들을 불러올 전체 URL입니다. 비워 두면 클라우드 제공업체 설정으로 자동 생성된 URL을 사용합니다.")]
        public string remoteBundleUrl;

        [Header("초기화")]
        [Tooltip("애플리케이션 시작 시 Addressables를 자동으로 초기화합니다.")]
        public bool autoInitialize = true;

        [Header("씬 관리")]
        [Tooltip("소유한 씬이 언로드될 때 에셋 핸들을 자동으로 해제합니다.")]
        public bool autoReleaseOnSceneUnload = true;

        private static AddressableManagerSettings _instance;

        public static AddressableManagerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AddressableManagerSettings>(SettingsPath);
                    if (_instance == null)
                    {
                        _instance = CreateInstance<AddressableManagerSettings>();
                    }
                }
                return _instance;
            }
        }

        public string GetRemoteCatalogUrl()
        {
            if (!string.IsNullOrEmpty(remoteCatalogUrl))
                return remoteCatalogUrl;

            return GenerateBaseUrl();
        }

        public string GetRemoteBundleUrl()
        {
            if (!string.IsNullOrEmpty(remoteBundleUrl))
                return remoteBundleUrl;

            return GenerateBaseUrl();
        }

        private string GenerateBaseUrl()
        {
            return cloudProvider switch
            {
                CloudProvider.AWSS3 =>
                    $"https://{bucketName}.s3.{bucketRegion}.amazonaws.com/[BuildTarget]",
                CloudProvider.GoogleCloudStorage =>
                    $"https://storage.googleapis.com/{bucketName}/[BuildTarget]",
                _ => string.Empty
            };
        }
    }
}
#endif
