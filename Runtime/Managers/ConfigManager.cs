using System.Collections.Generic;
using System.Threading.Tasks;
using AchEngine.Managers.Internal;
using UnityEngine;

namespace AchEngine.Managers
{
    /// <summary>
    /// PlayerPrefs 기반으로 키-값 설정 데이터를 관리하는 매니저.
    /// 앱 실행 시 저장된 설정을 불러오고, 변경 시 즉시 저장한다.
    /// </summary>
    public class ConfigManager : IManager
    {
        private readonly string _prefsKey = $"{Application.identifier}.configs.achengine";
        private Dictionary<string, object> _configs;

        /// <summary>
        /// PlayerPrefs에서 설정 데이터를 불러온다.
        /// 저장된 데이터가 없으면 빈 딕셔너리로 초기화한다.
        /// </summary>
        public Task Initialize()
        {
            _configs = DictionaryPrefs.LoadDictionary<string, object>(_prefsKey) ?? new Dictionary<string, object>();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 지정한 키가 없을 때만 기본값과 함께 키를 추가한다.
        /// 이미 존재하는 키는 무시한다.
        /// </summary>
        /// <param name="key">추가할 설정 키.</param>
        /// <param name="defaultValue">키가 없을 때 사용할 기본값.</param>
        public void AddKey(string key, object defaultValue)
        {
            if (_configs.ContainsKey(key))
                return;
            _configs.Add(key, defaultValue);
        }

        /// <summary>
        /// 키에 해당하는 설정 값을 반환한다.
        /// 키가 없으면 null을 반환한다.
        /// </summary>
        /// <param name="key">조회할 설정 키.</param>
        /// <returns>설정 값 또는 null.</returns>
        public object GetConfig(string key)
        {
            return _configs.TryGetValue(key, out var obj) ? obj : null;
        }

        /// <summary>
        /// 키에 해당하는 설정 값을 지정 타입으로 변환하여 반환한다.
        /// 키가 없거나 변환에 실패하면 fallback을 반환한다.
        /// </summary>
        /// <typeparam name="T">반환할 값의 타입.</typeparam>
        /// <param name="key">조회할 설정 키.</param>
        /// <param name="fallback">값이 없거나 변환 실패 시 반환할 기본값.</param>
        /// <returns>변환된 설정 값 또는 fallback.</returns>
        public T GetConfig<T>(string key, T fallback = default)
        {
            if (!_configs.TryGetValue(key, out var obj) || obj == null)
                return fallback;
            try { return (T)System.Convert.ChangeType(obj, typeof(T)); }
            catch { return fallback; }
        }

        /// <summary>
        /// 등록된 키의 설정 값을 변경하고 즉시 저장한다.
        /// 등록되지 않은 키는 무시한다.
        /// </summary>
        /// <param name="key">변경할 설정 키.</param>
        /// <param name="value">새로 설정할 값.</param>
        public void SetConfig(string key, object value)
        {
            if (!_configs.ContainsKey(key))
                return;
            _configs[key] = value;
            DictionaryPrefs.SaveDictionary(_prefsKey, _configs);
        }
    }
}
