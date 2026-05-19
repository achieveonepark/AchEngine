using System.Collections.Generic;
using AchEngine.Table;
using UnityEngine;

namespace AchEngine.DI
{
    /// <summary>
    /// ITableService의 전역 정적 파사드입니다.
    /// VContainer 사용 시 AchEngineScope가 자동으로 설정하고, 아닌 경우 직접 SetService를 호출하세요.
    /// </summary>
    public static class TableManager
    {
        private static ITableService _service;
        private static TableService _fallback;

        /// <summary>현재 활성화된 ITableService입니다. 미설정 시 내부 폴백 인스턴스를 반환합니다.</summary>
        public static ITableService Current => _service ?? FallbackService;

        private static ITableService FallbackService
        {
            get
            {
                if (_fallback == null)
                {
                    _fallback = new TableService(new TableDatabase());
                }
                return _fallback;
            }
        }

        /// <summary>외부에서 ITableService 구현체를 주입합니다.</summary>
        public static void SetService(ITableService service) => _service = service;

        /// <summary>바이트 배열에서 타입 <typeparamref name="T"/>의 테이블 데이터를 로드합니다.</summary>
        public static void Load<T>(byte[] bytes) where T : ITableData
            => Current.Load<T>(bytes);

        /// <summary>TextAsset에서 타입 <typeparamref name="T"/>의 테이블 데이터를 로드합니다.</summary>
        public static void Load<T>(TextAsset asset) where T : ITableData
            => Current.Load<T>(asset);

        /// <summary>JSON 문자열에서 타입 <typeparamref name="T"/>의 테이블 데이터를 로드합니다.</summary>
        public static void LoadFromJsonText<T>(string json) where T : ITableData
            => Current.LoadFromJsonText<T>(json);

        /// <summary>ID로 항목을 가져옵니다. 없으면 default를 반환합니다.</summary>
        public static T Get<T>(int id) where T : ITableData
            => Current.Database.Get<T>(id);

        /// <summary>ID로 항목을 시도해 가져옵니다. 없으면 false를 반환합니다.</summary>
        public static bool TryGet<T>(int id, out T result) where T : ITableData
            => Current.Database.TryGet(id, out result);

        /// <summary>타입 <typeparamref name="T"/>의 모든 항목을 열거합니다.</summary>
        public static IEnumerable<T> GetAll<T>() where T : ITableData
            => Current.Database.GetAll<T>();

        /// <summary>서비스와 폴백 인스턴스를 초기화합니다.</summary>
        public static void Reset()
        {
            _service = null;
            _fallback = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => Reset();
    }
}
