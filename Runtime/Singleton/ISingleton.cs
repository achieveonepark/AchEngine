namespace AchEngine
{
    /// <summary>싱글턴 초기화 및 정리 계약을 정의하는 인터페이스.</summary>
    public interface ISingleton
    {
        /// <summary>싱글턴 인스턴스를 초기화합니다. 최초 생성 시 한 번만 호출됩니다.</summary>
        void InitializeSingleton();

        /// <summary>싱글턴 인스턴스를 정리합니다. DestroyInstance 호출 시 실행됩니다.</summary>
        void ClearSingleton();
    }
}
