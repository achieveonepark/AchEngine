using UnityEngine;
#if UNITY_EDITOR
using System.IO;
#endif

namespace AchEngine
{
    public abstract class AchScriptableObject : ScriptableObject
    {
        private static readonly string AssetPath = "Assets/AchEngine/Resources/Settings";
        private static readonly string ResourcePath = "Settings";

        public static T GetOrAdd<T>() where T : ScriptableObject
        {
            var asset = Resources.Load<T>($"{ResourcePath}/{typeof(T).Name}");

            if (asset == null)
            {
                asset = CreateInstance<T>();
#if UNITY_EDITOR
                if (!Directory.Exists(AssetPath))
                    Directory.CreateDirectory(AssetPath);

                string fullPath = Path.Combine(AssetPath, typeof(T).Name + ".asset");
                UnityEditor.AssetDatabase.CreateAsset(asset, fullPath);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }

            return asset;
        }

#if UNITY_EDITOR
        public static T Add<T>() where T : ScriptableObject
        {
            var asset = CreateInstance<T>();

            if (!Directory.Exists(AssetPath))
                Directory.CreateDirectory(AssetPath);

            string fullPath = Path.Combine(AssetPath, typeof(T).Name + ".asset");
            UnityEditor.AssetDatabase.CreateAsset(asset, fullPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return asset;
        }

        public static void PingAsset<T>() where T : ScriptableObject
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return;

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            UnityEditor.EditorGUIUtility.PingObject(obj);
            UnityEditor.Selection.activeObject = obj;
        }
#endif
    }
}
