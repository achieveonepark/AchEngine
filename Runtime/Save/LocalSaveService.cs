#if USE_QUICK_SAVE
using System.Threading.Tasks;
using AchEngine.Managers;
using AchEngine.Player;

namespace AchEngine.Save
{
    public class LocalSaveService : ISaveService
    {
        private readonly QuickSave _quickSave = new();
        public bool IsExist { get; set; }

        public void Configure(string encryptionKey = "", int version = 0)
            => _quickSave.Configure(encryptionKey, version);

        public void Save(PlayerManager manager) => _quickSave.Save(manager);

        public PlayerManager Load()
        {
            var result = _quickSave.Load();
            IsExist = result != null;
            return result;
        }

        public void Delete() => _quickSave.Delete();

        public Task SaveAsync(PlayerManager manager)
        {
            Save(manager);
            return Task.CompletedTask;
        }

        public Task<PlayerManager> LoadAsync() => Task.FromResult(Load());

        public Task DeleteAsync()
        {
            Delete();
            return Task.CompletedTask;
        }
    }
}
#endif
