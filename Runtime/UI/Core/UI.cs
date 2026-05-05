using System;

namespace AchEngine.UI
{
    /// <summary>
    /// Static facade for UIService.
    /// VContainer ?ъ슜 ??AchEngineScope媛 ?먮룞?쇰줈 ?ㅼ젙?⑸땲??
    /// VContainer ?놁씠 ?ъ슜??寃쎌슦 UIBootstrapper媛 ?ㅼ젙?섍굅?? ?섎룞?쇰줈 SetService瑜??몄텧?섏꽭??
    /// </summary>
    public static class UI
    {
        public static UIService Current { get; private set; }
        public static bool IsReady => Current != null;

        public static UIView Show(string id, object payload = null)
        {
            return RequireService().Show(id, payload);
        }

        public static T Show<T>(string id, object payload = null)
            where T : UIView
        {
            return RequireService().Show<T>(id, payload);
        }

        public static bool Close(string id, bool closeAll = false)
        {
            return Current != null && Current.Close(id, closeAll);
        }

        public static bool Close(UIView view)
        {
            return Current != null && Current.Close(view);
        }

        public static bool CloseTopmost()
        {
            return Current != null && Current.CloseTopmost();
        }

        public static void CloseAll()
        {
            Current?.CloseAll();
        }

        public static bool TryGetOpen(string id, out UIView view)
        {
            if (Current != null)
            {
                return Current.TryGetOpen(id, out view);
            }

            view = null;
            return false;
        }

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

        public static bool IsOpen(string id)
        {
            return Current != null && Current.IsOpen(id);
        }

        internal static void SetService(UIService service)
        {
            Current = service;
        }

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
