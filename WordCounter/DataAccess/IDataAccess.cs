
namespace WordCounter.DataAccess
{
    public interface IDataAccess
    {
        Task WriteWordCount(string word, int count);
        Task WriteExcluded(Dictionary<string, int> excludedWords, int excludedCount);
        Task<IEnumerable<string>> GetExcludedWords();

        IAsyncEnumerable<string> ReadLines(string inputSource);

        List<string> GetInputSources();
    }
}
