using System;
using UnityEngine;

namespace AchEngine.UI
{
    /// <summary>
    /// UIService의 전역 정적 파사드입니다.
    /// VContainer 사용 시 AchEngineScope가 자동으로 설정하고, 아닌 경우 UIBootstrapper가 설정합니다.
    /// </summary>
    public static class UI
    {
        /// <summary>현재 활성화된 UIService 인스턴스입니다.</summary>
        public static UIService Current { get; private set; }
        /// <summary>UIService가 초기화되어 사용 가능한 상태인지 나타냅니다.</summary>
        public static bool IsReady => Current != null;

        /// <summary>지정한 ID의 뷰를 열고 인스턴스를 반환합니다.</summary>
        /// <param name="id">카탈로그에 등록된 뷰 ID.</param>
        /// <param name="payload">OnBeforeOpen에 전달할 데이터.</param>
        public static UIView Show(string id, object payload = null)
        {
            return RequireService().Show(id, payload);
        }

        /// <summary>지정한 ID의 뷰를 <typeparamref name="T"/>로 캐스팅해 반환합니다.</summary>
        public static T Show<T>(string id, object payload = null)
            where T : UIView
        {
            return RequireService().Show<T>(id, payload);
        }

        /// <summary>ID로 열린 뷰를 닫습니다.</summary>
        public static bool Close(string id, bool closeAll = false)
        {
            return Current != null && Current.Close(id, closeAll);
        }

        /// <summary>뷰 인스턴스를 직접 닫습니다.</summary>
        public static bool Close(UIView view)
        {
            return Current != null && Current.Close(view);
        }

        /// <summary>활성화 스택에서 가장 마지막에 열린 뷰를 닫습니다.</summary>
        public static bool CloseTopmost()
        {
            return Current != null && Current.CloseTopmost();
        }

        /// <summary>현재 열려있는 모든 뷰를 닫습니다.</summary>
        public static void CloseAll()
        {
            Current?.CloseAll();
        }

        /// <summary>지정한 ID의 열린 뷰를 가져옵니다. 없으면 false를 반환합니다.</summary>
        public static bool TryGetOpen(string id, out UIView view)
        {
            if (Current != null)
            {
                return Current.TryGetOpen(id, out view);
            }

            view = null;
            return false;
        }

        /// <summary>활성화 스택에서 <typeparamref name="T"/> 타입의 뷰를 찾아 반환합니다.</summary>
        public static bool TryGetOpen<T>(out T view)
            where T : UIView
        {
            if (Current != null)
            {
                return Current.TryGetOpen(out view);
            }

            view = null;
            return false;
        }

        /// <summary>지정한 ID의 뷰가 현재 열려있는지 확인합니다.</summary>
        public static bool IsOpen(string id)
        {
            return Current != null && Current.IsOpen(id);
        }

        internal static void SetService(UIService service)
        {
            Current = service;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetDomain() => Current = null;

        private static UIService RequireService()
        {
            if (Current == null)
            {
                throw new InvalidOperationException(
                    "UI is not ready. Add a AchEngineScope (VContainer) or UIBootstrapper to your scene first.");
            }

            return Current;
        }
    }
}
