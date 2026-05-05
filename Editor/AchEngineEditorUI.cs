using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor
{
    /// <summary>
    /// AchEngine Project Settings 패널에서 공통으로 사용하는 UI Toolkit 헬퍼.
    /// AchEngine.Editor 어셈블리에 있으므로 하위 에디터 어셈블리(Addressables.Editor 등)에서도 참조 가능.
    /// </summary>
    public static class AchEngineEditorUI
    {
        // ─────────────────────────────────────────────────────────────
        // 색상 팔레트
        // ─────────────────────────────────────────────────────────────
        public static readonly Color ColorAccent      = new(0.36f, 0.63f, 0.83f);
        public static readonly Color ColorText        = new(0.85f, 0.85f, 0.85f);
        public static readonly Color ColorTextMuted   = new(0.60f, 0.60f, 0.60f);
        public static readonly Color ColorTextBody    = new(0.80f, 0.80f, 0.80f);
        public static readonly Color ColorDivider     = new(0.27f, 0.27f, 0.27f);
        public static readonly Color ColorSurface     = new(0.18f, 0.18f, 0.18f);
        public static readonly Color ColorCodeBg      = new(0.13f, 0.13f, 0.13f);
        public static readonly Color ColorGreen       = new(0.18f, 0.80f, 0.44f);
        public static readonly Color ColorRed         = new(0.91f, 0.30f, 0.24f);
        public static readonly Color ColorButtonBlue  = new(0.22f, 0.44f, 0.69f);
        public static readonly Color ColorButtonDanger = new(0.42f, 0.12f, 0.12f);

        // ─────────────────────────────────────────────────────────────
        // 루트 스타일 적용
        // ─────────────────────────────────────────────────────────────
        public static void ApplyRootStyle(VisualElement root)
        {
            root.style.paddingLeft   = 0;
            root.style.paddingRight  = 0;
            root.style.paddingTop    = 0;
            root.style.paddingBottom = 0;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow      = 1f;

            var ussPath = FindAssetPath<StyleSheet>("TableLoaderWindow");
            if (ussPath != null)
                root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath));
        }

        /// <summary>패딩이 포함된 ScrollView를 만들어 root에 추가하고 반환합니다.</summary>
        public static ScrollView MakeScrollContent(VisualElement root)
        {
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1f;
            scroll.contentContainer.style.paddingLeft   = 20f;
            scroll.contentContainer.style.paddingRight  = 20f;
            scroll.contentContainer.style.paddingTop    = 20f;
            scroll.contentContainer.style.paddingBottom = 20f;
            root.Add(scroll);
            return scroll;
        }

        // ─────────────────────────────────────────────────────────────
        // 타이포그래피
        // ─────────────────────────────────────────────────────────────
        public static Label MakePageTitle(string text, string version = null)
        {
            var label = new Label(version != null ? $"{text}  v{version}" : text);
            label.style.fontSize                  = 20f;
            label.style.unityFontStyleAndWeight   = FontStyle.Bold;
            label.style.color                     = new StyleColor(ColorAccent);
            label.style.marginBottom              = 8f;
            return label;
        }

        public static Label MakeSectionTitle(string text)
        {
            var label = new Label(text);
            label.style.fontSize                  = 14f;
            label.style.unityFontStyleAndWeight   = FontStyle.Bold;
            label.style.color                     = new StyleColor(ColorAccent);
            label.style.marginBottom              = 6f;
            label.style.paddingBottom             = 4f;
            label.style.borderBottomWidth         = 1f;
            label.style.borderBottomColor         = new StyleColor(ColorDivider);
            return label;
        }

        public static Label MakeSubTitle(string text)
        {
            var label = new Label(text);
            label.style.fontSize                  = 13f;
            label.style.unityFontStyleAndWeight   = FontStyle.Bold;
            label.style.color                     = new StyleColor(ColorText);
            label.style.marginTop                 = 8f;
            label.style.marginBottom              = 4f;
            return label;
        }

        public static Label MakeBodyText(string text)
        {
            var label = new Label(text);
            label.style.fontSize     = 12f;
            label.style.color        = new StyleColor(ColorTextBody);
            label.style.whiteSpace   = WhiteSpace.Normal;
            label.style.marginBottom = 8f;
            return label;
        }

        // ─────────────────────────────────────────────────────────────
        // 레이아웃
        // ─────────────────────────────────────────────────────────────
        public static VisualElement MakeDivider()
        {
            var divider = new VisualElement();
            divider.style.height          = 1f;
            divider.style.marginTop       = 12f;
            divider.style.marginBottom    = 12f;
            divider.style.backgroundColor = new StyleColor(ColorDivider);
            return divider;
        }

        public static VisualElement MakeCodeBlock(string code)
        {
            var container = new VisualElement();
            container.style.backgroundColor        = new StyleColor(ColorCodeBg);
            container.style.borderTopLeftRadius    = 4f;
            container.style.borderTopRightRadius   = 4f;
            container.style.borderBottomLeftRadius  = 4f;
            container.style.borderBottomRightRadius = 4f;
            container.style.paddingTop    = 10f;
            container.style.paddingBottom = 10f;
            container.style.paddingLeft   = 12f;
            container.style.paddingRight  = 12f;
            container.style.marginBottom  = 12f;

            var label = new Label(code);
            label.style.fontSize   = 11f;
            label.style.color      = new StyleColor(new Color(0.78f, 0.90f, 0.72f));
            label.style.whiteSpace = WhiteSpace.Normal;
            container.Add(label);
            return container;
        }

        /// <summary>카드 스타일 컨테이너 (둥근 모서리, 어두운 배경)</summary>
        public static VisualElement MakeCard()
        {
            var card = new VisualElement();
            card.style.backgroundColor        = new StyleColor(ColorSurface);
            card.style.borderTopLeftRadius    = 4f;
            card.style.borderTopRightRadius   = 4f;
            card.style.borderBottomLeftRadius  = 4f;
            card.style.borderBottomRightRadius = 4f;
            card.style.paddingTop    = 8f;
            card.style.paddingBottom = 8f;
            card.style.paddingLeft   = 10f;
            card.style.paddingRight  = 10f;
            card.style.marginBottom  = 6f;
            return card;
        }

        // ─────────────────────────────────────────────────────────────
        // 정보 행
        // ─────────────────────────────────────────────────────────────
        public static VisualElement MakeInfoRow(string labelText, string value, float labelWidth = 110f)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom  = 3f;

            var l = new Label(labelText);
            l.style.width   = labelWidth;
            l.style.color   = new StyleColor(ColorTextMuted);
            l.style.fontSize = 12f;

            var v = new Label(value);
            v.style.color   = new StyleColor(ColorText);
            v.style.fontSize = 12f;

            row.Add(l);
            row.Add(v);
            return row;
        }

        public static VisualElement MakeStatusRow(string labelText, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom  = 4f;

            var l = new Label(labelText);
            l.style.width   = 100f;
            l.style.color   = new StyleColor(ColorTextMuted);
            l.style.fontSize = 12f;

            bool empty = string.IsNullOrEmpty(value);
            var v = new Label(empty ? "씬에 없음" : value);
            v.style.color   = new StyleColor(empty ? ColorRed : ColorGreen);
            v.style.fontSize = 12f;

            row.Add(l);
            row.Add(v);
            return row;
        }

        // ─────────────────────────────────────────────────────────────
        // 입력 필드 (SerializedObject 바인딩)
        // ─────────────────────────────────────────────────────────────
        public static TextField MakeSerializedTextField(
            SerializedObject so, Object target, string propName, string label)
        {
            var field = new TextField(label)
            {
                value     = so.FindProperty(propName).stringValue,
                isDelayed = true
            };
            field.style.marginBottom = 4f;
            field.RegisterValueChangedCallback(evt =>
            {
                so.Update();
                so.FindProperty(propName).stringValue = evt.newValue;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            return field;
        }

        public static Toggle MakeSerializedToggle(
            SerializedObject so, Object target, string propName, string label)
        {
            var toggle = new Toggle(label) { value = so.FindProperty(propName).boolValue };
            toggle.style.marginBottom = 4f;
            toggle.RegisterValueChangedCallback(evt =>
            {
                so.Update();
                so.FindProperty(propName).boolValue = evt.newValue;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            return toggle;
        }

        // ─────────────────────────────────────────────────────────────
        // 버튼
        // ─────────────────────────────────────────────────────────────
        /// <summary>행 레이아웃용 버튼 컨테이너</summary>
        public static VisualElement MakeButtonRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.flexWrap      = Wrap.Wrap;
            row.style.marginTop     = 8f;
            return row;
        }

        // ─────────────────────────────────────────────────────────────
        // 유틸
        // ─────────────────────────────────────────────────────────────
        public static string FindAssetPath<T>(string name) where T : Object
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name} {name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                    return path;
            }
            return null;
        }
    }
}
