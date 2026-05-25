/* ============================================================
   AchEngine API Docs — VitePress 가이드와의 접합면
   - VitePress nav/sidebar에서 ?lang=<locale>을 전달받아 백링크에 사용
   - 첫 진입 시 localStorage에 저장 → DocFX 내 페이지 이동에도 유지
   - DocFX 네비게이션 좌측에 "← 가이드" 링크 자동 주입
   ============================================================ */
(function () {
    var STORAGE_KEY = "achengine-locale";
    var VALID = ["ko", "en", "ja", "zh"];

    function resolveLocale() {
        try {
            var params = new URLSearchParams(window.location.search);
            var q = params.get("lang");
            if (q && VALID.indexOf(q) >= 0) {
                window.localStorage.setItem(STORAGE_KEY, q);
                return q;
            }
            var stored = window.localStorage.getItem(STORAGE_KEY);
            if (stored && VALID.indexOf(stored) >= 0) return stored;
        } catch (e) {
            /* localStorage 비활성 환경 — 무시 */
        }
        return "ko";
    }

    var LABELS = {
        ko: { back: "← 가이드", search: "API 검색" },
        en: { back: "← Guide", search: "Search API" },
        ja: { back: "← ガイド", search: "API検索" },
        zh: { back: "← 指南", search: "搜索 API" }
    };

    function guideHref(locale) {
        var prefix = locale === "ko" ? "" : "/" + locale;
        return "/AchEngine" + prefix + "/guide/";
    }

    function buildBackLink(locale) {
        var a = document.createElement("a");
        a.href = guideHref(locale);
        a.textContent = LABELS[locale].back;
        a.className = "achengine-back-link";
        a.setAttribute("aria-label", LABELS[locale].back);
        return a;
    }

    function inject() {
        var locale = resolveLocale();

        // DocFX modern 템플릿(Bootstrap 5) — 네비게이션 브랜드 옆에 삽입
        var brand = document.querySelector("header .navbar-brand, .bd-header .navbar-brand");
        if (brand && brand.parentNode) {
            var link = buildBackLink(locale);
            brand.parentNode.insertBefore(link, brand);
            return;
        }

        // 폴백: 페이지 최상단에 sticky 바로 삽입
        var bar = document.createElement("div");
        bar.className = "achengine-back-bar";
        bar.appendChild(buildBackLink(locale));
        if (document.body && document.body.firstChild) {
            document.body.insertBefore(bar, document.body.firstChild);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", inject);
    } else {
        inject();
    }
})();
