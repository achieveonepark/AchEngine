using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// Localization 시스템 설정 ScriptableObject.
    /// Resources 폴더에 "LocalizationSettings" 이름으로 배치.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "Achieve/Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        private const string ResourcePath = "LocalizationSettings";

        [Header("General")]
        [Tooltip("기본 언어 코드")]
        public string defaultLocaleCode = "en";

        [Tooltip("폴백 언어 코드 (키를 찾을 수 없을 때 사용)")]
        public string fallbackLocaleCode = "en";

        [Tooltip("시스템 언어를 자동 감지하여 적용")]
        public bool autoDetectSystemLanguage = true;

        [Tooltip("앱 시작 시 자동으로 초기화")]
        public bool autoInitialize = true;

        [Tooltip("Locale 데이터베이스 참조")]
        public LocaleDatabase database;

        [Header("Code Generation")]
        [Tooltip("생성될 키 상수 클래스 이름")]
        public string generatedClassName = "L";

        [Tooltip("생성될 클래스의 네임스페이스 (비어있으면 글로벌)")]
        public string generatedNamespace = "";

        [Tooltip("생성된 파일의 출력 경로")]
        public string generatedOutputPath = "Assets/Scripts/Generated";

        private static LocalizationSettings _instance;

        public static LocalizationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LocalizationSettings>(ResourcePath);
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        _instance = CreateInstance<LocalizationSettings>();
                    }
#endif
                }
                return _instance;
            }
        }

        /// <summary>
        /// 에디터에서 인스턴스를 직접 설정할 때 사용
        /// </summary>
        public static void SetInstance(LocalizationSettings settings)
        {
            _instance = settings;
        }

        private void OnEnable()
        {
            if (_instance == null)
                _instance = this;
        }
    }
}
