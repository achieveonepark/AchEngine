using System.Collections.Generic;
using System.Threading.Tasks;
using AchEngine.Managers.Internal;
using UnityEngine;

namespace AchEngine.Managers
{
    public class ConfigManager : IManager
    {
        private readonly string _prefsKey = $"{Application.identifier}.configs.achengine";
        private Dictionary<string, object> _configs;

        public Task Initialize()
        {
            _configs = DictionaryPrefs.LoadDictionary<string, object>(_prefsKey) ?? new Dictionary<string, object>();
            return Task.CompletedTask;
        }

        public void AddKey(string key, object defaultValue)
        {
            if (_configs.ContainsKey(key))
                return;
            _configs.Add(key, defaultValue);
        }

        public object GetConfig(string key)
        {
            return _configs.TryGetValue(key, out var obj) ? obj : null;
        }

        public T GetConfig<T>(string key, T fallback = default)
        {
            if (!_configs.TryGetValue(key, out var obj) || obj == null)
                return fallback;
            try { return (T)System.Convert.ChangeType(obj, typeof(T)); }
            catch { return fallback; }
        }

        public void SetConfig(string key, object value)
        {
            if (!_configs.ContainsKey(key))
                return;
            _configs[key] = value;
            DictionaryPrefs.SaveDictionary(_prefsKey, _configs);
        }
    }
}
