using System.Threading.Tasks;
using AchEngine.Save;

namespace AchEngine.Managers
{
    /// <summary>
    /// 플레이어 데이터 저장·불러오기·삭제를 ISaveService에 위임하는 매니저.
    /// </summary>
    public class SaveManager : IManager
    {
        private readonly PlayerManager _playerManager;
        private readonly ISaveService _saveService;

        /// <summary>
        /// 저장 데이터가 존재하는지 여부.
        /// </summary>
        public bool IsExist => _saveService.IsExist;

        /// <summary>
        /// SaveManager를 생성한다.
        /// </summary>
        /// <param name="playerManager">저장 대상 플레이어 데이터 매니저.</param>
        /// <param name="saveService">실제 저장 로직을 담당하는 서비스.</param>
        public SaveManager(PlayerManager playerManager, ISaveService saveService)
        {
            _playerManager = playerManager;
            _saveService = saveService;
        }

        /// <summary>
        /// 초기화. SaveManager는 별도 초기화 작업이 없다.
        /// </summary>
        public Task Initialize() => Task.CompletedTask;

        /// <summary>
        /// 현재 플레이어 데이터를 동기적으로 저장한다.
        /// </summary>
        public void Save() => _saveService.Save(_playerManager);

        /// <summary>
        /// 저장된 플레이어 데이터를 동기적으로 불러온다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스.</returns>
        public PlayerManager Load() => _saveService.Load();

        /// <summary>
        /// 저장 데이터를 동기적으로 삭제한다.
        /// </summary>
        public void Delete() => _saveService.Delete();

        /// <summary>
        /// 현재 플레이어 데이터를 비동기적으로 저장한다.
        /// </summary>
        public Task SaveAsync() => _saveService.SaveAsync(_playerManager);

        /// <summary>
        /// 저장된 플레이어 데이터를 비동기적으로 불러온다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스.</returns>
        public Task<PlayerManager> LoadAsync() => _saveService.LoadAsync();

        /// <summary>
        /// 저장 데이터를 비동기적으로 삭제한다.
        /// </summary>
        public Task DeleteAsync() => _saveService.DeleteAsync();
    }
}
