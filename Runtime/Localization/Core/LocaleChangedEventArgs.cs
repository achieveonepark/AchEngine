using System;

namespace AchEngine.Localization
{
    /// <summary>
    /// 언어 변경 이벤트 인자
    /// </summary>
    public class LocaleChangedEventArgs : EventArgs
    {
        public Locale PreviousLocale { get; }
        public Locale NewLocale { get; }

        public LocaleChangedEventArgs(Locale previousLocale, Locale newLocale)
        {
            PreviousLocale = previousLocale;
            NewLocale = newLocale;
        }
    }
}
