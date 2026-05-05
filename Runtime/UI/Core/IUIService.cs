using System.Collections.Generic;

namespace AchEngine.UI
{
    public interface IUIService
    {
        bool IsInitialized { get; }
        UIViewCatalog Catalog { get; }
        UIRoot Root { get; }
        UIView Show(string id, object payload = null);
        T Show<T>(string id, object payload = null) where T : UIView;
        bool Close(string id, bool closeAll = false);
        bool Close(UIView view);
        bool CloseTopmost();
        void CloseAll();
        bool TryGetOpen(string id, out UIView view);
        bool TryGetOpen<T>(out T view) where T : UIView;
        bool IsOpen(string id);
        void Prewarm();
    }
}
