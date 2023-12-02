using System.Collections.Concurrent;
using WordCounter.DataAccess;

namespace WordCounter
{
    public class WordCounter
    {
        private IDataAccess DataAccess { get; }
        private IDictionary<string, int> WordCount { get; set; }
        private IEnumerable<string> Excluded { get; set; }
        
        public WordCounter(IDataAccess dataAccess) {
            DataAccess = dataAccess;
            WordCount = new Dictionary<string, int>();
            Excluded = new List<string>();
        }

        public async Task CountWords()
        {
            Excluded = await DataAccess.GetExcludedWords();
            await ReadWords();
            await PersistWords();
        }

        private async Task ReadWords()
        {
            WordCount = await DataAccess.ReadLines();
        }

        private async Task PersistWords()
        {
            int excluded = 0;
            var excludedWords = new Dictionary<string, int>();
            foreach(var wordCount in WordCount)
            {
                if(IsExcluded(wordCount.Key)) {
                    excludedWords.Add(wordCount.Key, wordCount.Value);
                    excluded += wordCount.Value;
                    continue;
                }
                await DataAccess.WriteWordCount(wordCount.Key, wordCount.Value);
            }
            if (excludedWords.Any())
            {
                await DataAccess.WriteExcluded(excludedWords, excluded);
            }
        }

        public bool IsExcluded(string word)
        {
            if (Excluded.Contains(word)) { return true; }
            return false;
        }
    }
}
