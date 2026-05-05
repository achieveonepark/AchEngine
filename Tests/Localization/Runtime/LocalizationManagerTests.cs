using System.Collections.Generic;
using NUnit.Framework;

namespace AchEngine.Localization.Tests
{
    public class SimpleJsonParserTests
    {
        [Test]
        public void Parse_EmptyJson_ReturnsEmptyDictionary()
        {
            var result = SimpleJsonParser.Parse("{}");
            Assert.NotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Parse_SimpleKeyValue_ReturnsDictionary()
        {
            var json = "{\"key1\": \"value1\", \"key2\": \"value2\"}";
            var result = SimpleJsonParser.Parse(json);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"]);
            Assert.AreEqual("value2", result["key2"]);
        }

        [Test]
        public void Parse_EscapedCharacters_HandledCorrectly()
        {
            var json = "{\"key\": \"line1\\nline2\\ttab\"}";
            var result = SimpleJsonParser.Parse(json);

            Assert.AreEqual("line1\nline2\ttab", result["key"]);
        }

        [Test]
        public void Parse_UnicodeContent_HandledCorrectly()
        {
            var json = "{\"greeting\": \"안녕하세요\", \"title\": \"タイトル\"}";
            var result = SimpleJsonParser.Parse(json);

            Assert.AreEqual("안녕하세요", result["greeting"]);
            Assert.AreEqual("タイトル", result["title"]);
        }

        [Test]
        public void Serialize_EmptyDictionary_ReturnsEmptyJson()
        {
            var result = SimpleJsonParser.Serialize(new Dictionary<string, string>());
            Assert.AreEqual("{}", result);
        }

        [Test]
        public void Serialize_RoundTrip_PreservesData()
        {
            var data = new Dictionary<string, string>
            {
                { "app.title", "Test App" },
                { "menu.start", "Start" },
                { "dialog.msg", "Hello, {name}!" }
            };

            string json = SimpleJsonParser.Serialize(data);
            var result = SimpleJsonParser.Parse(json);

            Assert.AreEqual(data.Count, result.Count);
            foreach (var kvp in data)
                Assert.AreEqual(kvp.Value, result[kvp.Key]);
        }
    }

    public class StringFormatterTests
    {
        [Test]
        public void Format_PositionalArgs_ReplacesCorrectly()
        {
            string result = StringFormatter.Format("Hello {0}, you have {1} items", "World", 5);
            Assert.AreEqual("Hello World, you have 5 items", result);
        }

        [Test]
        public void Format_NamedArgs_ReplacesCorrectly()
        {
            var args = new Dictionary<string, object>
            {
                { "playerName", "TestPlayer" },
                { "count", 3 }
            };

            string result = StringFormatter.Format("Welcome, {playerName}! You have {count} messages.", args);
            Assert.AreEqual("Welcome, TestPlayer! You have 3 messages.", result);
        }

        [Test]
        public void Format_MissingNamedArg_KeepsOriginal()
        {
            var args = new Dictionary<string, object>
            {
                { "name", "Test" }
            };

            string result = StringFormatter.Format("Hello {name}, score: {score}", args);
            Assert.AreEqual("Hello Test, score: {score}", result);
        }

        [Test]
        public void Format_NullTemplate_ReturnsNull()
        {
            string result = StringFormatter.Format(null, "arg");
            Assert.IsNull(result);
        }

        [Test]
        public void Format_NoArgs_ReturnsTemplate()
        {
            string result = StringFormatter.Format("Hello World");
            Assert.AreEqual("Hello World", result);
        }
    }

    public class LocaleTests
    {
        [Test]
        public void Locale_Equality_CaseInsensitive()
        {
            var en1 = new Locale("en", "English");
            var en2 = new Locale("EN", "English");

            Assert.AreEqual(en1, en2);
            Assert.IsTrue(en1 == en2);
        }

        [Test]
        public void Locale_Inequality_DifferentCodes()
        {
            var en = new Locale("en", "English");
            var ko = new Locale("ko", "Korean");

            Assert.AreNotEqual(en, ko);
            Assert.IsTrue(en != ko);
        }
    }
}
