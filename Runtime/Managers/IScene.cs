namespace AchEngine.Managers
{
    public interface IScene
    {
        AchTask OnSceneStart();
        AchTask OnSceneEnd();
    }
}
