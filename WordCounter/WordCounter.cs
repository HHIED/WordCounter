using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using WordCounter.DataAccess;

namespace WordCounter
{
    public class WordCounter
    {
        private IDataAccess DataAccess { get; }
        private IDictionary<string, int> WordCount { get; set; }
        private IEnumerable<string> Excluded { get; set; }

        private const string _onlyLettersRegex = "[^a-zA-z]+";
        private const char _seperator = ' ';

        public WordCounter(IDataAccess dataAccess) {
            DataAccess = dataAccess;
            WordCount = new Dictionary<string, int>();
            Excluded = new List<string>();
        }

        public async Task ProcessData()
        {
            Excluded = await DataAccess.GetExcludedWords();
            await CountWords();
            await PersistWords();
        }

        private async Task CountWords()
        {
            var wordCountResult = new ConcurrentDictionary<string, int>();
            var inputSources = DataAccess.GetInputSources();
            await Parallel.ForEachAsync(inputSources, async (inputSource, cancellationToken) =>
            {
                await foreach (var line in DataAccess.ReadLines(inputSource)) {
                    var words = line.Split(_seperator);
                    foreach (var word in words)
                    {
                        var cleanWord = Regex.Replace(word, _onlyLettersRegex, string.Empty).ToLower();
                        if (string.IsNullOrEmpty(cleanWord)) { continue; }
                        wordCountResult.AddOrUpdate(cleanWord, 1, (key, oldValue) => oldValue + 1);
                    }
                }
            });
            WordCount = wordCountResult;
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
