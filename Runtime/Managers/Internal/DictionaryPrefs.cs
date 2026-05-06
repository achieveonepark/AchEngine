using System;
using System.Collections.Generic;
using UnityEngine;

namespace AchEngine.Managers.Internal
{
    internal static class DictionaryPrefs
    {
        [Serializable]
        private class SerializablePair
        {
            public string key;
            public string value;
            public string typeName;
        }

        [Serializable]
        private class SerializableDict
        {
            public List<SerializablePair> pairs = new();
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string prefsKey)
        {
            if (!PlayerPrefs.HasKey(prefsKey))
                return null;

            var json = PlayerPrefs.GetString(prefsKey);
            var sd = JsonUtility.FromJson<SerializableDict>(json);
            if (sd?.pairs == null)
                return null;

            var result = new Dictionary<TKey, TValue>();
            foreach (var pair in sd.pairs)
            {
                try
                {
                    var k = (TKey)Convert.ChangeType(pair.key, typeof(TKey));
                    object v;
                    if (!string.IsNullOrEmpty(pair.typeName))
                    {
                        var t = Type.GetType(pair.typeName);
                        v = t != null ? Convert.ChangeType(pair.value, t) : pair.value;
                    }
                    else
                    {
                        v = pair.value;
                    }
                    result[k] = (TValue)v;
                }
                catch { /* skip corrupt entry */ }
            }
            return result;
        }

        public static void SaveDictionary<TKey, TValue>(string prefsKey, Dictionary<TKey, TValue> dict)
        {
            var sd = new SerializableDict();
            foreach (var kvp in dict)
            {
                sd.pairs.Add(new SerializablePair
                {
                    key = kvp.Key.ToString(),
                    value = kvp.Value?.ToString() ?? string.Empty,
                    typeName = kvp.Value?.GetType().AssemblyQualifiedName ?? string.Empty
                });
            }
            PlayerPrefs.SetString(prefsKey, JsonUtility.ToJson(sd));
            PlayerPrefs.Save();
        }
    }
}
