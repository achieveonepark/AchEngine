using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AchEngine
{
    public static class PlayerPrefsExt
    {
        public static void SaveDictionary<TKey, TValue>(string key, Dictionary<TKey, TValue> dictionary)
        {
            var json = JsonConvert.SerializeObject(dictionary);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return new Dictionary<TKey, TValue>();

            var json = PlayerPrefs.GetString(key);
            return JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json) ?? new Dictionary<TKey, TValue>();
        }
    }
}
