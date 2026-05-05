#if UNITY_6000_3_OR_NEWER
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor.Decorators
{
    /// <summary>
    /// Draws extra hints on Project window rows: alternating stripes,
    /// file size badge for assets, item count for folders.
    /// </summary>
    [InitializeOnLoad]
    internal static class AchEngineProjectDecorator
    {
        private const int RowHeight = 16;

        private static readonly Color StripeColor = new Color(1f, 1f, 1f, 0.025f);
        private static readonly Color BadgeBg = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color FolderBadgeBg = new Color(0.18f, 0.36f, 0.55f, 0.85f);
        private static readonly Color BadgeText = new Color(0.92f, 0.92f, 0.92f, 1f);

        private static GUIStyle _badgeStyle;

        // Cached folder counts to avoid scanning every repaint.
        private static readonly Dictionary<string, (int count, double until)> _folderCountCache =
            new Dictionary<string, (int, double)>();

        static AchEngineProjectDecorator()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

            AchEngineDecoratorSettings.Changed -= ClearCache;
            AchEngineDecoratorSettings.Changed += ClearCache;
        }

        private static void ClearCache()
        {
            _folderCountCache.Clear();
        }

        private static void OnProjectGUI(string guid, Rect rect)
        {
            if (!AchEngineDecoratorSettings.ProjectEnabled)
            {
                return;
            }

            EnsureStyle();

            // Only handle one-line list rows; skip the icon-grid view.
            var isListRow = rect.height <= RowHeight + 4;
            if (!isListRow)
            {
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // ----- Stripes -----
            if (AchEngineDecoratorSettings.ProjectStripes)
            {
                var row = Mathf.FloorToInt(rect.y / RowHeight);
                if ((row & 1) == 1)
                {
                    var stripeRect = new Rect(rect.x, rect.y, rect.width + 200, rect.height);
                    EditorGUI.DrawRect(stripeRect, StripeColor);
                }
            }

            var isFolder = AssetDatabase.IsValidFolder(path);
            var cursor = rect.xMax;

            if (isFolder)
            {
                if (AchEngineDecoratorSettings.ProjectFolderItemCount)
                {
                    var count = GetFolderItemCount(path);
                    if (count > 0)
                    {
                        cursor = DrawBadge(cursor, rect, count.ToString(), FolderBadgeBg);
                    }
                }
            }
            else if (AchEngineDecoratorSettings.ProjectFileSizeBadge)
            {
                var size = TryGetFileSize(path);
                if (size > 0)
                {
                    cursor = DrawBadge(cursor, rect, FormatBytes(size), BadgeBg);
                }
            }
        }

        private static int GetFolderItemCount(string path)
        {
            if (_folderCountCache.TryGetValue(path, out var entry) &&
                EditorApplication.timeSinceStartup < entry.until)
            {
                return entry.count;
            }

            var absolute = Path.GetFullPath(path);
            int count = 0;
            try
            {
                if (Directory.Exists(absolute))
                {
                    foreach (var f in Directory.EnumerateFileSystemEntries(absolute))
                    {
                        if (f.EndsWith(".meta"))
                        {
                            continue;
                        }
                        count++;
                    }
                }
            }
            catch
            {
                count = 0;
            }

            _folderCountCache[path] = (count, EditorApplication.timeSinceStartup + 5.0);
            return count;
        }

        private static long TryGetFileSize(string path)
        {
            try
            {
                var absolute = Path.GetFullPath(path);
                if (File.Exists(absolute))
                {
                    return new FileInfo(absolute).Length;
                }
            }
            catch
            {
                // ignore
            }
            return 0;
        }

        private static float DrawBadge(float right, Rect row, string text, Color bg)
        {
            var content = new GUIContent(text);
            var size = _badgeStyle.CalcSize(content);
            var width = Mathf.Min(size.x + 8, 90);
            var badgeRect = new Rect(right - width, row.y + (row.height - 12) * 0.5f, width, 12);
            EditorGUI.DrawRect(badgeRect, bg);
            GUI.Label(badgeRect, content, _badgeStyle);
            return right - width - 2;
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            return $"{value:0.#}{units[unit]}";
        }

        private static void EnsureStyle()
        {
            if (_badgeStyle != null)
            {
                return;
            }
            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(4, 4, 0, 0),
                normal = { textColor = BadgeText },
            };
        }
    }
}
#endif
