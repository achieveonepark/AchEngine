using System.Threading.Tasks;
using AchEngine.Save;

namespace AchEngine.Managers
{
    public class SaveManager : IManager
    {
        private readonly PlayerManager _playerManager;
        private readonly ISaveService _saveService;

        public bool IsExist => _saveService.IsExist;

        public SaveManager(PlayerManager playerManager, ISaveService saveService)
        {
            _playerManager = playerManager;
            _saveService = saveService;
        }

        public Task Initialize() => Task.CompletedTask;

        public void Save() => _saveService.Save(_playerManager);
        public PlayerManager Load() => _saveService.Load();
        public void Delete() => _saveService.Delete();

        public Task SaveAsync() => _saveService.SaveAsync(_playerManager);
        public Task<PlayerManager> LoadAsync() => _saveService.LoadAsync();
        public Task DeleteAsync() => _saveService.DeleteAsync();
    }
}
