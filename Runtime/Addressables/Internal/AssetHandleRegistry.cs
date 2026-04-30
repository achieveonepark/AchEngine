#if ACHENGINE_ADDRESSABLES
using System;
using System.Collections.Generic;

namespace AchEngine.Assets.Internal
{
    internal class AssetHandleRegistry
    {
        private readonly Dictionary<string, string> _registeredKeys = new(StringComparer.Ordinal);

        public void Register(string key, string runtimeKey)
        {
            _registeredKeys[key] = runtimeKey;
        }

        public bool TryGetRuntimeKey(string key, out string runtimeKey)
        {
            return _registeredKeys.TryGetValue(key, out runtimeKey);
        }

        public bool TryGetKeyByRuntimeKey(string runtimeKey, out string key)
        {
            foreach (var pair in _registeredKeys)
            {
                if (string.Equals(pair.Value, runtimeKey, StringComparison.Ordinal))
                {
                    key = pair.Key;
                    return true;
                }
            }

            key = null;
            return false;
        }

        public bool Unregister(string key)
        {
            return _registeredKeys.Remove(key);
        }

        public bool Contains(string key)
        {
            return _registeredKeys.ContainsKey(key);
        }
    }
}
#endif
