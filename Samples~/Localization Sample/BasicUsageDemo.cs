using System.Collections.Generic;
using UnityEngine;
using AchEngine.Localization;

/// <summary>
/// Achieve Localization 기본 사용법 예제
/// </summary>
public class BasicUsageDemo : MonoBehaviour
{
    [Header("Localized String References")]
    [SerializeField] private LocalizedString titleString;
    [SerializeField] private LocalizedString welcomeString;

    private void Start()
    {
        // ── 기본 사용법 ──

        // 1. 단순 문자열 가져오기
        string title = LocalizationManager.Get("app.title");
        Debug.Log($"Title: {title}");

        // 2. LocalizedString 구조체 사용 (Inspector에서 키 설정 가능)
        Debug.Log($"Title via struct: {titleString.Value}");

        // 3. 위치 기반 인자 사용
        string welcome = LocalizationManager.Get("dialog.welcome", "Player1", 5);
        Debug.Log($"Welcome: {welcome}");

        // 4. 이름 기반 인자 사용
        var args = new Dictionary<string, object>
        {
            { "playerName", "홍길동" },
            { "count", 3 }
        };
        string welcomeNamed = LocalizationManager.Get("dialog.welcome", args);
        Debug.Log($"Welcome (named): {welcomeNamed}");

        // 5. 키 존재 여부 확인
        bool hasKey = LocalizationManager.HasKey("menu.start");
        Debug.Log($"Has 'menu.start': {hasKey}");

        // 6. TryGet 사용
        if (LocalizationManager.TryGet("menu.settings", out string settingsText))
            Debug.Log($"Settings: {settingsText}");

        // ── locale 변경 이벤트 구독 ──
        LocalizationManager.LocaleChanged += OnLocaleChanged;

        // ── 현재 상태 출력 ──
        Debug.Log($"Current Locale: {LocalizationManager.CurrentLocale}");
        Debug.Log($"Available Locales: {string.Join(", ", LocalizationManager.AvailableLocales)}");
    }

    private void OnDestroy()
    {
        LocalizationManager.LocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(LocaleChangedEventArgs args)
    {
        Debug.Log($"Locale changed: {args.PreviousLocale} → {args.NewLocale}");
        Debug.Log($"Title is now: {LocalizationManager.Get("app.title")}");
    }

    /// <summary>
    /// 테스트용: 스페이스바로 영어/한국어 전환
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string current = LocalizationManager.CurrentLocale.Code;
            string next = current == "en" ? "ko" : "en";
            LocalizationManager.SetLocale(next);
        }
    }
}
