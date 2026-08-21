using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AchEngine.Player
{
    [Serializable]
    public class PlayerDataContainerBase<TKey, TValue> : IPlayerDataContainerBase
    {
        [JsonProperty]
        public string DataKey { get; protected set; }

        [JsonProperty]
        protected Dictionary<TKey, TValue> _dataDic = new();

        public void Add(TKey key, TValue value)      => _dataDic[key] = value;
        public bool RemoveInfo(TKey key)              => _dataDic.Remove(key);
        public TValue GetInfo(TKey key)               => _dataDic.GetValueOrDefault(key);
        public IEnumerable<KeyValuePair<TKey, TValue>> GetAll() => _dataDic;
    }
}
