namespace AchEngine.UI
{
    /// <summary>UIView 열기/닫기 시 사용할 전환 애니메이션 방식입니다.</summary>
    public enum UITransitionMode
    {
        /// <summary>전환 없이 즉시 표시/숨김.</summary>
        None = 0,
        /// <summary>알파 페이드 전환.</summary>
        Fade = 1,
        /// <summary>알파 페이드 + 스케일 전환.</summary>
        FadeScale = 2
    }
}
