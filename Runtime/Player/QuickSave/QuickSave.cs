#if USE_QUICK_SAVE
using System;
using AchEngine.Managers;
using Achieve.QuickSave;

namespace AchEngine.Player
{
    internal class QuickSave
    {
        private QuickSave<PlayerManager> _instance;
        
        internal void Configure(string encryptionKey = "", int version = 0)
        {
            _instance = new QuickSave<PlayerManager>.Builder()
                .Build();
        }

        internal void Save(PlayerManager manager)
        {
            EnsureConfigured();
            _instance.SaveData(manager);
        }

        internal PlayerManager Load()
        {
            EnsureConfigured();
            _instance ??= new QuickSave<PlayerManager>();
            return _instance.LoadData();
        }

        internal void Delete()
        {
            EnsureConfigured();
            _instance.DeleteData();
        }

        private void EnsureConfigured()
        {
            _instance ??= new QuickSave<PlayerManager>();
        }
    }
}
#endif
