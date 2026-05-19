#if USE_QUICK_SAVE
using System.Threading.Tasks;
using AchEngine.Managers;
using AchEngine.Player;

namespace AchEngine.Save
{
    /// <summary>
    /// QuickSave를 사용하여 로컬 파일에 플레이어 데이터를 저장하는 서비스.
    /// USE_QUICK_SAVE 심볼이 정의된 경우에만 컴파일된다.
    /// </summary>
    public class LocalSaveService : ISaveService
    {
        private readonly QuickSave _quickSave = new();

        /// <summary>
        /// 저장 데이터가 존재하는지 여부.
        /// </summary>
        public bool IsExist { get; set; }

        /// <summary>
        /// 암호화 키와 버전을 QuickSave에 전달하여 설정한다.
        /// </summary>
        /// <param name="encryptionKey">데이터 암호화에 사용할 키.</param>
        /// <param name="version">저장 데이터 버전 번호.</param>
        public void Configure(string encryptionKey = "", int version = 0)
            => _quickSave.Configure(encryptionKey, version);

        /// <summary>
        /// 플레이어 데이터를 동기적으로 저장한다.
        /// </summary>
        /// <param name="manager">저장할 PlayerManager 인스턴스.</param>
        public void Save(PlayerManager manager) => _quickSave.Save(manager);

        /// <summary>
        /// 저장된 플레이어 데이터를 동기적으로 불러온다.
        /// 결과에 따라 IsExist를 갱신한다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스. 데이터가 없으면 null.</returns>
        public PlayerManager Load()
        {
            var result = _quickSave.Load();
            IsExist = result != null;
            return result;
        }

        /// <summary>
        /// 저장 데이터를 동기적으로 삭제한다.
        /// </summary>
        public void Delete() => _quickSave.Delete();

        /// <summary>
        /// 플레이어 데이터를 비동기적으로 저장한다. 내부적으로 동기 저장을 호출한다.
        /// </summary>
        /// <param name="manager">저장할 PlayerManager 인스턴스.</param>
        public Task SaveAsync(PlayerManager manager)
        {
            Save(manager);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 저장된 플레이어 데이터를 비동기적으로 불러온다. 내부적으로 동기 로드를 호출한다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스.</returns>
        public Task<PlayerManager> LoadAsync() => Task.FromResult(Load());

        /// <summary>
        /// 저장 데이터를 비동기적으로 삭제한다. 내부적으로 동기 삭제를 호출한다.
        /// </summary>
        public Task DeleteAsync()
        {
            Delete();
            return Task.CompletedTask;
        }
    }
}
#endif
