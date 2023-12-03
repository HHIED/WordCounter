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
