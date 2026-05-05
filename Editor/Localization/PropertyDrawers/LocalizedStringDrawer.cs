using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Localization.Editor
{
    /// <summary>
    /// LocalizedString 구조체의 커스텀 PropertyDrawer.
    /// Inspector에서 키 선택 드롭다운과 번역 미리보기를 제공.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalizedString))]
    [CustomPropertyDrawer(typeof(LocalizedStringAttribute))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        private const float PreviewHeight = 18f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + PreviewHeight + Spacing * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // key 프로퍼티 가져오기
            SerializedProperty keyProp;
            if (property.type == "LocalizedString")
                keyProp = property.FindPropertyRelative("key");
            else
                keyProp = property; // LocalizedStringAttribute가 string 필드에 적용된 경우

            // 첫 번째 줄: 키 입력 필드 + 드롭다운 버튼
            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var fieldRect = new Rect(lineRect.x, lineRect.y, lineRect.width - 25, lineRect.height);
            var buttonRect = new Rect(lineRect.xMax - 23, lineRect.y, 23, lineRect.height);

            // 라벨 + 텍스트 필드
            keyProp.stringValue = EditorGUI.TextField(fieldRect, label, keyProp.stringValue);

            // 드롭다운 버튼
            if (GUI.Button(buttonRect, "\u25bc", EditorStyles.miniButton)) // ▼
            {
                ShowKeyDropdown(buttonRect, keyProp);
            }

            // 두 번째 줄: 번역 미리보기
            var previewRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y + EditorGUIUtility.singleLineHeight + Spacing,
                position.width - EditorGUIUtility.labelWidth,
                PreviewHeight
            );

            string key = keyProp.stringValue;
            string preview = GetPreviewText(key);

            var prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            EditorGUI.LabelField(previewRect, preview, EditorStyles.miniLabel);
            GUI.color = prevColor;

            EditorGUI.EndProperty();
        }

        private void ShowKeyDropdown(Rect buttonRect, SerializedProperty keyProp)
        {
            var settings = LocalizationSettings.Instance;
            if (settings?.database == null)
            {
                EditorUtility.DisplayDialog("No Database",
                    "LocaleDatabase is not configured.\nGo to Project Settings > Achieve Localization.", "OK");
                return;
            }

            settings.database.InvalidateCache();
            settings.database.ParseJsonAssets();
            var allKeys = settings.database.GetAllKeys();

            if (allKeys.Count == 0)
            {
                EditorUtility.DisplayDialog("No Keys",
                    "No localization keys found in the database.", "OK");
                return;
            }

            // 검색 가능 드롭다운 메뉴
            var menu = new GenericMenu();

            // 카테고리별로 그룹화 (dot-notation 기준)
            var categories = new Dictionary<string, List<string>>();
            foreach (var key in allKeys)
            {
                int dotIndex = key.IndexOf('.');
                string category = dotIndex >= 0 ? key.Substring(0, dotIndex) : "(root)";

                if (!categories.TryGetValue(category, out var list))
                {
                    list = new List<string>();
                    categories[category] = list;
                }
                list.Add(key);
            }

            foreach (var category in categories)
            {
                foreach (var key in category.Value)
                {
                    string menuPath = key.Replace('.', '/');
                    bool isSelected = key == keyProp.stringValue;

                    menu.AddItem(new GUIContent(menuPath), isSelected, () =>
                    {
                        keyProp.stringValue = key;
                        keyProp.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.DropDown(buttonRect);
        }

        private string GetPreviewText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "(no key set)";

            var settings = LocalizationSettings.Instance;
            if (settings?.database == null)
                return "(no database)";

            settings.database.InvalidateCache();
            settings.database.ParseJsonAssets();

            // 기본 locale에서 미리보기
            string previewLocale = settings.defaultLocaleCode;
            if (settings.database.TryGetValue(previewLocale, key, out var value) && !string.IsNullOrEmpty(value))
                return $"[{previewLocale}] {value}";

            // 폴백 locale 시도
            if (settings.database.TryGetValue(settings.fallbackLocaleCode, key, out var fallback) && !string.IsNullOrEmpty(fallback))
                return $"[{settings.fallbackLocaleCode}] {fallback}";

            return $"[missing: {key}]";
        }
    }
}
