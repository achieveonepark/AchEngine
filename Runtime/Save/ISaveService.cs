using System.Threading.Tasks;
using AchEngine.Managers;

namespace AchEngine.Save
{
    public interface ISaveService
    {
        void Configure(string encryptionKey = "", int version = 0);
        bool IsExist { get; set; }

        void Save(PlayerManager manager);
        PlayerManager Load();
        void Delete();

        Task SaveAsync(PlayerManager manager);
        Task<PlayerManager> LoadAsync();
        Task DeleteAsync();
    }
}
