using NUnit.Framework;

namespace AchEngine.Localization.Editor.Tests
{
    public class LocalizedTMPCharacterSetBuilderTests
    {
        [Test]
        public void BuildFallback_WithKoreanPreset_IncludesHangul()
        {
            var result = LocalizedTMPCharacterSetBuilder.BuildFallback(new LocalizedTMPCharacterSetOptions
            {
                IncludeCommonAscii = true,
                IncludeKorean = true
            });

            Assert.That(result.CharacterSet, Does.Contain("A"));
            Assert.That(result.CharacterSet, Does.Contain("ㄱ"));
            Assert.That(result.CharacterSet, Does.Contain("가"));
            Assert.That(result.CharacterSet, Does.Contain("힣"));
        }

        [Test]
        public void BuildFallback_WithAdditionalTexts_IncludesTableText()
        {
            var result = LocalizedTMPCharacterSetBuilder.BuildFallback(new LocalizedTMPCharacterSetOptions
            {
                IncludeCommonAscii = true,
                AdditionalTexts = new[]
                {
                    "Id,Name,Description\n1,Stone Shot,기본 투사체 공격",
                    "{\"help\":\"이동과 자동 공격\"}"
                }
            });

            Assert.That(result.SourceStringCount, Is.EqualTo(2));
            Assert.That(result.CharacterSet, Does.Contain("기"));
            Assert.That(result.CharacterSet, Does.Contain("본"));
            Assert.That(result.CharacterSet, Does.Contain("공"));
            Assert.That(result.CharacterSet, Does.Contain("이"));
            Assert.That(result.CharacterSet, Does.Contain("동"));
        }

        [Test]
        public void BuildFallback_WithoutKoreanPreset_LeavesHangulOut()
        {
            var result = LocalizedTMPCharacterSetBuilder.BuildFallback(new LocalizedTMPCharacterSetOptions
            {
                IncludeCommonAscii = true,
                IncludeKorean = false
            });

            Assert.That(result.CharacterSet, Does.Contain("A"));
            Assert.That(result.CharacterSet, Does.Not.Contain("가"));
        }
    }
}
