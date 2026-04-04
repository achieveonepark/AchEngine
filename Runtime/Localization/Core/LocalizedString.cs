using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// Inspector에서 사용 가능한 직렬화 가능 localization 키 참조.
    /// LocalizedStringAttribute와 함께 사용하면 키 선택 드롭다운을 제공.
    /// </summary>
    [Serializable]
    public struct LocalizedString
    {
        [SerializeField] private string key;

        /// <summary>현재 설정된 키</summary>
        public string Key => key;

        /// <summary>현재 locale에서 번역된 값</summary>
        public string Value => LocalizationManager.Get(key);

        public LocalizedString(string key)
        {
            this.key = key;
        }

        /// <summary>
        /// 위치 기반 인자를 사용하여 번역된 값을 반환
        /// </summary>
        public string GetValue(params object[] args)
        {
            return LocalizationManager.Get(key, args);
        }

        /// <summary>
        /// 이름 기반 인자를 사용하여 번역된 값을 반환
        /// </summary>
        public string GetValue(Dictionary<string, object> namedArgs)
        {
            return LocalizationManager.Get(key, namedArgs);
        }

        public static implicit operator string(LocalizedString ls) => ls.Value;

        public override string ToString() => Value;

        public bool IsValid => !string.IsNullOrEmpty(key);
    }
}
