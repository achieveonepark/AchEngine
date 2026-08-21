using System.Collections.Generic;
using NUnit.Framework;

namespace AchEngine.Localization.Editor.Tests
{
    public class LocalizationKeyGeneratorTests
    {
        [Test]
        public void Generate_SimpleKeys_ProducesValidCode()
        {
            var keys = new List<string>
            {
                "app.title",
                "menu.start",
                "menu.quit"
            };

            string code = LocalizationKeyGenerator.GenerateCode(keys, "L", "");

            Assert.IsTrue(code.Contains("public static class L"));
            Assert.IsTrue(code.Contains("public static class App"));
            Assert.IsTrue(code.Contains("public static class Menu"));
            Assert.IsTrue(code.Contains("public const string Title = \"app.title\""));
            Assert.IsTrue(code.Contains("public const string Start = \"menu.start\""));
            Assert.IsTrue(code.Contains("public const string Quit = \"menu.quit\""));
        }

        [Test]
        public void Generate_WithNamespace_WrapsInNamespace()
        {
            var keys = new List<string> { "test.key" };
            string code = LocalizationKeyGenerator.GenerateCode(keys, "L", "MyGame");

            Assert.IsTrue(code.Contains("namespace MyGame"));
        }

        [Test]
        public void Generate_RootLevelKeys_PlacedDirectly()
        {
            var keys = new List<string> { "title", "subtitle" };
            string code = LocalizationKeyGenerator.GenerateCode(keys, "L", "");

            Assert.IsTrue(code.Contains("public const string Title = \"title\""));
            Assert.IsTrue(code.Contains("public const string Subtitle = \"subtitle\""));
        }

        [Test]
        public void Generate_EmptyKeys_ProducesEmptyClass()
        {
            var keys = new List<string>();
            string code = LocalizationKeyGenerator.GenerateCode(keys, "L", "");

            Assert.IsTrue(code.Contains("public static class L"));
        }

        [Test]
        public void Generate_CollidingIdentifiers_Throws()
        {
            var keys = new List<string> { "menu.foo-bar", "menu.foo_bar" };

            Assert.Throws<System.InvalidOperationException>(
                () => LocalizationKeyGenerator.GenerateCode(keys, "L", ""));
        }

        [Test]
        public void Generate_SameInput_ProducesDeterministicCode()
        {
            var keys = new List<string> { "menu.start", "menu.quit" };

            var first = LocalizationKeyGenerator.GenerateCode(keys, "L", "MyGame");
            var second = LocalizationKeyGenerator.GenerateCode(keys, "L", "MyGame");

            Assert.AreEqual(first, second);
        }
    }
}
