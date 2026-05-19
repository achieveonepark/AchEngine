namespace AchEngine.UI
{
    /// <summary>UI 뷰가 배치될 레이어를 나타냅니다. 값이 클수록 위쪽에 렌더링됩니다.</summary>
    public enum UILayerId
    {
        /// <summary>배경 레이어 — 가장 아래에 위치합니다.</summary>
        Background = 0,
        /// <summary>일반 화면 레이어.</summary>
        Screen = 10,
        /// <summary>팝업 레이어 — Screen 위에 표시됩니다.</summary>
        Popup = 20,
        /// <summary>오버레이 레이어 — 화면 전체를 덮는 UI에 사용합니다.</summary>
        Overlay = 30,
        /// <summary>툴팁 레이어 — 가장 위에 표시됩니다.</summary>
        Tooltip = 40
    }
}
