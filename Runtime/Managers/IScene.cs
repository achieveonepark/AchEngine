using System.Threading.Tasks;

namespace AchEngine.Managers
{
    public interface IScene
    {
        Task OnSceneStart();
        Task OnSceneEnd();
    }
}
