using Moq;
using WordCounter.DataAccess;

namespace WordCounterTest
{
    public class WordCounterTest
    {
        [TestCase("foo", "", 1, null , null , null)]
        [TestCase("foo foo", "", 2, null, null, null)]
        [TestCase("foo bar", "", 1, 1, null, null)]
        [TestCase("foo", "foo", 2, null, null, null)]
        [TestCase("foo bar foobar barfoo", "", 1, 1, 1, 1)]
        [TestCase("foo! bar?, foobar. barfoo", "", 1, 1, 1, 1)]
        [TestCase("foo bar foobar barfoo", "foo bar foo bar", 3, 3, 1, 1)]
        public async Task TestCorrectCount(string line1, string line2, int? fooCount, int? barCount, int? fooBarCount, int? barFooCount)
        {
            //Arrange
            var dataAccessMock = new Mock<IDataAccess>();
            var fileReader = new WordCounter.WordCounter(dataAccessMock.Object);
            List<string> testLines =  new List<string> { line1, line2 };
            IEnumerable<string> excludedWords = new List<string>();
            dataAccessMock.Setup(x => x.WriteWordCount(It.IsAny<string>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.GetExcludedWords()).Returns(Task.FromResult(excludedWords));
            dataAccessMock.Setup(x => x.GetInputSources()).Returns(new List<string> { "inputSource" });
            dataAccessMock.Setup(x => x.ReadLines(It.IsAny<string>())).Returns(testLines.ToAsyncEnumerable());

            //Act
            await fileReader.ProcessData();

            //Assert
            Assert.Multiple(() =>
            {
                if(fooCount != null)
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("foo", fooCount.Value), Times.Once());

                } else
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("foo", It.IsAny<int>()), Times.Never);
                }
                if (barCount != null)
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("bar", barCount.Value), Times.Once());

                }
                else
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("bar", It.IsAny<int>()), Times.Never);
                }
                if (fooBarCount != null)
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("foobar", fooBarCount.Value), Times.Once());

                }
                else
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("foobar", It.IsAny<int>()), Times.Never);
                }
                if (barFooCount != null)
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("barfoo", barFooCount.Value), Times.Once());

                }
                else
                {
                    dataAccessMock.Verify(x => x.WriteWordCount("barfoo", It.IsAny<int>()), Times.Never);
                }
                dataAccessMock.Verify(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()), Times.Never);

            });
        }

        [Test]
        public async Task TestExcluded()
        {
            //Arrange
            var dataAccessMock = new Mock<IDataAccess>();
            var fileReader = new WordCounter.WordCounter(dataAccessMock.Object);
            IEnumerable<string> excludedWords = new List<string> { "foo", "bar" };
            List<string> testLines = new List<string> { "foo foo foo bar foobar barfoo" };
            dataAccessMock.Setup(x => x.WriteWordCount(It.IsAny<string>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.GetExcludedWords()).Returns(Task.FromResult(excludedWords));
            dataAccessMock.Setup(x => x.GetInputSources()).Returns(new List<string> { "inputSource" });
            dataAccessMock.Setup(x => x.ReadLines(It.IsAny<string>())).Returns(testLines.ToAsyncEnumerable());

            //Act
            await fileReader.ProcessData();

            //Assert
            Assert.Multiple(() =>
            {
                dataAccessMock.Verify(x => x.WriteWordCount("foo", It.IsAny<int>()), Times.Never());
                dataAccessMock.Verify(x => x.WriteWordCount("bar", It.IsAny<int>()), Times.Never());
                dataAccessMock.Verify(x => x.WriteWordCount("foobar", 1), Times.Once());
                dataAccessMock.Verify(x => x.WriteWordCount("barfoo", 1), Times.Once());
                dataAccessMock.Verify(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), 4), Times.Once);

            });
        }
    }
}