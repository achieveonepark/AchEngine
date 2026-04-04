using System;
using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// 언어 정보를 나타내는 구조체
    /// </summary>
    [Serializable]
    public struct Locale : IEquatable<Locale>
    {
        [SerializeField] private string code;
        [SerializeField] private string displayName;

        /// <summary>ISO 639-1 언어 코드 (예: "en", "ko", "ja")</summary>
        public string Code => code;

        /// <summary>표시 이름 (예: "English", "한국어")</summary>
        public string DisplayName => displayName;

        public Locale(string code, string displayName)
        {
            this.code = code ?? throw new ArgumentNullException(nameof(code));
            this.displayName = displayName ?? code;
        }

        public bool Equals(Locale other)
        {
            return string.Equals(code, other.code, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is Locale other && Equals(other);
        }

        public override int GetHashCode()
        {
            return code != null ? code.ToLowerInvariant().GetHashCode() : 0;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(displayName) ? code : $"{displayName} ({code})";
        }

        public static bool operator ==(Locale left, Locale right) => left.Equals(right);
        public static bool operator !=(Locale left, Locale right) => !left.Equals(right);
    }
}
