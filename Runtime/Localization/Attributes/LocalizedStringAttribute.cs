using UnityEngine;

namespace AchEngine.Localization
{
    /// <summary>
    /// string 필드에 적용하여 Inspector에서 localization 키 선택 드롭다운을 표시.
    /// LocalizedString 구조체의 key 필드에는 자동 적용됨.
    /// </summary>
    public class LocalizedStringAttribute : PropertyAttribute
    {
    }
}
