using System.Text.RegularExpressions;
using WordCounter.DataAccess;

namespace WordCounterTest
{
    public class FileDataAccessTest
    {

        [TearDown]
        public void TeadDown()
        {
            DirectoryInfo outputDir = new DirectoryInfo(@"..\\..\\..\\TestData\\Output");
            foreach (FileInfo file in outputDir.GetFiles())
            {
                file.Delete();
            }
        }

        [TestCase(@"..\\..\\..\\TestData\\CaseChanges", 2, 2, 1, 1)]
        [TestCase(@"..\\..\\..\\TestData\\MultipleLines", 2, 3, 4, 1)]
        [TestCase(@"..\\..\\..\\TestData\\MultipleFiles", 4, 6, 8, 2)]
        [TestCase(@"..\\..\\..\\TestData\\OneWord", 1, null, null, null)]
        [TestCase(@"..\\..\\..\\TestData\\TwoInstances", 2, null, null, null)]
        [TestCase(@"..\\..\\..\\TestData\\UnwantedCharacters", 2, 2, 1, 1)]
        public async Task TestReadLines(string inputDir, int? fooCount, int? barCount, int? fooBarCount, int? barFooCount)
        {
            //Arrange
            var outputDir = @"..\\..\\..\\TestData\\Output";
            var fileDataAccess = new FileDataAccess(inputDir, outputDir, "");
           
            //Act
            var actual = await fileDataAccess.ReadLines();
            //Assert
            Assert.Multiple(() =>
            {
                int? actualFooCount = fooCount == null ? null : actual["foo"];
                int? actualBarCount = barCount == null ? null : actual["bar"];
                int? actualFooBarCount = fooBarCount == null ? null : actual["foobar"];
                int? actualBarFooCount = barFooCount == null ? null : actual["barfoo"];

                Assert.That(actualFooCount, Is.EqualTo(fooCount));
                Assert.That(actualBarCount, Is.EqualTo(barCount));
                Assert.That(actualFooBarCount, Is.EqualTo(fooBarCount));
                Assert.That(actualBarFooCount, Is.EqualTo(barFooCount));
            });
        }
        [TestCase(@"..\\..\\..\\TestData\\Output", "file_F.txt", "foo", 3)]
        [TestCase(@"..\\..\\..\\TestData\\Output", "file_B.txt", "boo", 6)]
        public async Task TestWriteWordCount(string outputDir, string fileName, string word, int count)
        {
            //Arrange
            var fileDataAccess = new FileDataAccess("", outputDir, "");

            //Act
            await fileDataAccess.WriteWordCount(word, count);

            //Assert
            var wordPattern = $"({word}) ([0-9]+)";
            var mathces = Regex.Match(File.ReadAllText(Path.Combine(outputDir,fileName)), wordPattern);
            int? actualCount = mathces.Success ? int.Parse(mathces.Groups[2].Value) : null;
            Assert.Multiple(() =>
            {
                Assert.That(actualCount, Is.EqualTo(count));
                Assert.That(mathces.NextMatch().Success, Is.False);
            });
        }

        [Test]
        public async Task TestWriteExcluded()
        {
            //Arrange
            var outputDir = @"..\\..\\..\\TestData\\Output";
            var fileDataAccess = new FileDataAccess("", outputDir, "");
            var excludedWords = new Dictionary<string, int>() { { "foo", 3 }, { "bar", 4 } };
            var totalExcluded = 7;

            //Act
            await fileDataAccess.WriteExcluded(excludedWords, totalExcluded);

            //Assert
            var actual = File.ReadAllText(Path.Combine(outputDir, "excluded.txt"));
            Assert.Multiple(() =>
            {
                Assert.That(actual.Contains("foo 3"), Is.True);
                Assert.That(actual.Contains("bar 4"), Is.True);
                Assert.That(actual.Contains("Total words excluded 7"), Is.True);
            });
        }

    }
}
