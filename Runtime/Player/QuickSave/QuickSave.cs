#if USE_QUICK_SAVE
using Achieve.QuickSave;

namespace AchEngine.Player
{
    internal class QuickSave
    {
        private readonly QuickSave<PlayerManager> _quickSave = new QuickSave<PlayerManager>.Builder()
            .UseEncryption("348GJ32ndh@R*gh#")
            .UseVersion(0)
            .Build();

        internal void Save(PlayerManager manager) => _quickSave.SaveData(manager);
        internal PlayerManager Load() => _quickSave.LoadData();
    }
}
#endif
