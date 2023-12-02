using Moq;
using WordCounter.DataAccess;

namespace WordCounterTest
{
    public class WordCounterTest
    {
        [Test]
        public async Task TestCorrectCount()
        {
            //Arrange
            var dataAccessMock = new Mock<IDataAccess>();
            var fileReader = new WordCounter.WordCounter(dataAccessMock.Object);
            IDictionary<string, int> testWords = new Dictionary<string, int> { { "foo", 3 }, { "bar", 1 }, { "fooBar", 99 } };
            IEnumerable<string> excludedWords = new List<string>();
            dataAccessMock.Setup(x => x.ReadLines()).Returns(Task.FromResult(testWords));
            dataAccessMock.Setup(x => x.WriteWordCount(It.IsAny<string>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.GetExcludedWords()).Returns(Task.FromResult(excludedWords));

            //Act
            await fileReader.CountWords();

            //Assert
            Assert.Multiple(() =>
            {
                dataAccessMock.Verify(x => x.WriteWordCount("foo", 3), Times.AtMostOnce());
                dataAccessMock.Verify(x => x.WriteWordCount("bar", 1), Times.AtMostOnce());
                dataAccessMock.Verify(x => x.WriteWordCount("fooBar", 99), Times.AtMostOnce());
                dataAccessMock.Verify(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()), Times.Never);

            });
        }

        [Test]
        public async Task TestExcluded()
        {
            //Arrange
            var dataAccessMock = new Mock<IDataAccess>();
            var fileReader = new WordCounter.WordCounter(dataAccessMock.Object);
            IDictionary<string, int> testWords = new Dictionary<string, int> { { "foo", 3 }, { "bar", 1 }, { "fooBar", 99 } };
            IEnumerable<string> excludedWords = new List<string> { "foo", "bar" };
            dataAccessMock.Setup(x => x.ReadLines()).Returns(Task.FromResult(testWords));
            dataAccessMock.Setup(x => x.WriteWordCount(It.IsAny<string>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.WriteExcluded(It.IsAny<Dictionary<string, int>>(), It.IsAny<int>()));
            dataAccessMock.Setup(x => x.GetExcludedWords()).Returns(Task.FromResult(excludedWords));

            //Act
            await fileReader.CountWords();

            //Assert
            Assert.Multiple(() =>
            {
                dataAccessMock.Verify(x => x.WriteWordCount("foo", 3), Times.Never());
                dataAccessMock.Verify(x => x.WriteWordCount("bar", 1), Times.Never());
                dataAccessMock.Verify(x => x.WriteWordCount("fooBar", 99), Times.AtMostOnce());
                dataAccessMock.Verify(x => x.WriteExcluded(new Dictionary<string, int> { { "foo", 3 }, { "bar", 1 } }, 4), Times.Once);

            });
        }
    }
}