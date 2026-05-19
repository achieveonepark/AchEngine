using System.Threading.Tasks;
using AchEngine.Managers;

namespace AchEngine.Save
{
    /// <summary>
    /// 플레이어 데이터의 저장·불러오기·삭제를 정의하는 저장 서비스 인터페이스.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// 암호화 키와 버전을 설정한다.
        /// </summary>
        /// <param name="encryptionKey">데이터 암호화에 사용할 키. 빈 문자열이면 암호화 미적용.</param>
        /// <param name="version">저장 데이터 버전 번호.</param>
        void Configure(string encryptionKey = "", int version = 0);

        /// <summary>
        /// 저장 데이터가 존재하는지 여부.
        /// </summary>
        bool IsExist { get; set; }

        /// <summary>
        /// 플레이어 데이터를 동기적으로 저장한다.
        /// </summary>
        /// <param name="manager">저장할 PlayerManager 인스턴스.</param>
        void Save(PlayerManager manager);

        /// <summary>
        /// 저장된 플레이어 데이터를 동기적으로 불러온다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스. 데이터가 없으면 null.</returns>
        PlayerManager Load();

        /// <summary>
        /// 저장 데이터를 동기적으로 삭제한다.
        /// </summary>
        void Delete();

        /// <summary>
        /// 플레이어 데이터를 비동기적으로 저장한다.
        /// </summary>
        /// <param name="manager">저장할 PlayerManager 인스턴스.</param>
        Task SaveAsync(PlayerManager manager);

        /// <summary>
        /// 저장된 플레이어 데이터를 비동기적으로 불러온다.
        /// </summary>
        /// <returns>불러온 PlayerManager 인스턴스. 데이터가 없으면 null.</returns>
        Task<PlayerManager> LoadAsync();

        /// <summary>
        /// 저장 데이터를 비동기적으로 삭제한다.
        /// </summary>
        Task DeleteAsync();
    }
}
