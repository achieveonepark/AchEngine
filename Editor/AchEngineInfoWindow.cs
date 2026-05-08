using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor
{
    internal sealed class AchEngineInfoWindow : EditorWindow
    {
        [MenuItem("Window/AchEngine/AchEngine Info")]
        public static void Open() => GetWindow<AchEngineInfoWindow>("AchEngine Info");

        // package availability (resolved at compile time)
#if ACHENGINE_VCONTAINER
        private const bool HasVContainer = true;
#else
        private const bool HasVContainer = false;
#endif
#if ACHENGINE_MEMORYPACK
        private const bool HasMemoryPack = true;
#else
        private const bool HasMemoryPack = false;
#endif
#if ACHENGINE_ADDRESSABLES
        private const bool HasAddressables = true;
#else
        private const bool HasAddressables = false;
#endif
#if ACHENGINE_R3
        private const bool HasR3 = true;
#else
        private const bool HasR3 = false;
#endif

        private struct PackageRow
        {
            public string Name;
            public string PackageId;
            public bool Installed;
            public string Feature;
        }

        private static readonly PackageRow[] Packages =
        {
            new() { Name = "VContainer",   PackageId = "jp.hadashikick.vcontainer",  Installed = HasVContainer,   Feature = "DI 컨테이너 (AchEngineScope, ServiceLocator)" },
            new() { Name = "MemoryPack",   PackageId = "com.cysharp.memorypack",     Installed = HasMemoryPack,   Feature = "QuickSave 직렬화 (USE_QUICK_SAVE)" },
            new() { Name = "Addressables", PackageId = "com.unity.addressables",     Installed = HasAddressables, Feature = "AddressableManager, RemoteContentManager" },
            new() { Name = "R3",           PackageId = "com.cysharp.r3",             Installed = HasR3,           Feature = "UIBindingManager (Reactive pub/sub)" },
        };

        public void CreateGUI()
        {
            var scroll = AchEngineEditorUI.MakeScrollContent(rootVisualElement);

            scroll.Add(AchEngineEditorUI.MakePageTitle("AchEngine Info"));
            scroll.Add(AchEngineEditorUI.MakeBodyText("패키지 설치 여부에 따라 활성화되는 기능 목록입니다."));
            scroll.Add(AchEngineEditorUI.MakeDivider());

            scroll.Add(AchEngineEditorUI.MakeSectionTitle("Optional Packages"));
            scroll.Add(BuildHeader());

            foreach (var pkg in Packages)
                scroll.Add(BuildRow(pkg));
        }

        private static VisualElement BuildHeader()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.paddingLeft   = 10f;
            row.style.paddingBottom = 4f;
            row.style.marginBottom  = 2f;
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new StyleColor(AchEngineEditorUI.ColorDivider);

            row.Add(MakeHeaderCell("Package",    140f));
            row.Add(MakeHeaderCell("Status",      70f));
            row.Add(MakeHeaderCell("Package ID",  220f));
            row.Add(MakeHeaderCell("활성화 기능", 0f, true));
            return row;
        }

        private static Label MakeHeaderCell(string text, float width, bool grow = false)
        {
            var label = new Label(text);
            label.style.fontSize = 11f;
            label.style.color    = new StyleColor(AchEngineEditorUI.ColorTextMuted);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            if (width > 0f) label.style.width = width;
            if (grow)       label.style.flexGrow = 1f;
            return label;
        }

        private static VisualElement BuildRow(PackageRow pkg)
        {
            var card = AchEngineEditorUI.MakeCard();
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems    = Align.Center;

            // Package name
            var name = new Label(pkg.Name);
            name.style.width    = 140f;
            name.style.fontSize = 12f;
            name.style.color    = new StyleColor(AchEngineEditorUI.ColorText);
            name.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Status badge
            bool on = pkg.Installed;
            var badge = new Label(on ? "● Installed" : "○ Missing");
            badge.style.width    = 70f;
            badge.style.fontSize = 11f;
            badge.style.color    = new StyleColor(on ? AchEngineEditorUI.ColorGreen : AchEngineEditorUI.ColorTextMuted);

            // Package ID
            var id = new Label(pkg.PackageId);
            id.style.width    = 220f;
            id.style.fontSize = 11f;
            id.style.color    = new StyleColor(AchEngineEditorUI.ColorTextMuted);

            // Feature description
            var feature = new Label(on ? pkg.Feature : pkg.Feature + "  (비활성)");
            feature.style.flexGrow = 1f;
            feature.style.fontSize = 11f;
            feature.style.color    = new StyleColor(on ? AchEngineEditorUI.ColorTextBody : AchEngineEditorUI.ColorTextMuted);
            feature.style.whiteSpace = WhiteSpace.Normal;

            card.Add(name);
            card.Add(badge);
            card.Add(id);
            card.Add(feature);
            return card;
        }
    }
}
