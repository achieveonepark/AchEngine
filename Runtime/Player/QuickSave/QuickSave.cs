#if USE_QUICK_SAVE
using System;
using System.Threading.Tasks;
using AchEngine.Managers;
using Achieve.QuickSave;

namespace AchEngine.Player
{
    internal class QuickSave
    {
        private QuickSave<PlayerManager> _instance;
        
        internal void Configure(string encryptionKey = "", int version = 0)
        {
            if (version < 0)
                throw new ArgumentOutOfRangeException(nameof(version), "저장 버전은 0 이상이어야 합니다.");

            var builder = new QuickSave<PlayerManager>.Builder();
#if USE_ENCRYPT
            if (!string.IsNullOrEmpty(encryptionKey))
                builder.UseEncryption(encryptionKey);
            builder.UseVersion(version);
#else
            if (!string.IsNullOrEmpty(encryptionKey) || version != 0)
                throw new InvalidOperationException(
                    "암호화 키 또는 버전을 사용하려면 QuickSave의 USE_ENCRYPT 심볼과 암호화 의존성을 활성화해야 합니다.");
#endif
            _instance = builder.Build();
        }

        internal void Save(PlayerManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            EnsureConfigured();
            _instance.SaveData(manager);
        }

        internal Task SaveAsync(PlayerManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            EnsureConfigured();
            return _instance.SaveDataAsync(manager);
        }

        internal PlayerManager Load()
        {
            EnsureConfigured();
            return _instance.LoadData();
        }

        internal Task<PlayerManager> LoadAsync()
        {
            EnsureConfigured();
            return _instance.LoadDataAsync();
        }

        internal bool Exists()
        {
            EnsureConfigured();
            return _instance.HasSaveData();
        }

        internal void Delete()
        {
            EnsureConfigured();
            _instance.DeleteData();
        }

        private void EnsureConfigured()
        {
            _instance ??= new QuickSave<PlayerManager>.Builder().Build();
        }
    }
}
#endif
