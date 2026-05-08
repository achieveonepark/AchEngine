#if USE_QUICK_SAVE
using System;
using Achieve.QuickSave;

namespace AchEngine.Player
{
    internal class QuickSave
    {
        private Achieve.QuickSave.QuickSave<PlayerManager> _instance;

        internal void Configure(string encryptionKey, int version = 0)
        {
            _instance = new Achieve.QuickSave.QuickSave<PlayerManager>.Builder()
                .UseEncryption(encryptionKey)
                .UseVersion(version)
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
            return _instance.LoadData();
        }

        private void EnsureConfigured()
        {
            if (_instance == null)
                throw new InvalidOperationException("[AchEngine] Call PlayerManager.Configure() before Save/Load.");
        }
    }
}
#endif
