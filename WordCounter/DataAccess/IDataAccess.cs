using System.Collections.Concurrent;

namespace WordCounter.DataAccess
{
    public interface IDataAccess
    {
        Task<IDictionary<string, int>> ReadLines();
        Task WriteWordCount(string word, int count);
        Task WriteExcluded(Dictionary<string, int> excludedWords, int excludedCount);
        Task<IEnumerable<string>> GetExcludedWords();
    }
}
