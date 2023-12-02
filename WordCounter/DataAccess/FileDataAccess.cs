using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace WordCounter.DataAccess
{
    public class FileDataAccess : IDataAccess
    {
        private readonly string _inputDir;
        private readonly string _outputDir;
        private readonly string _excludedFilePath;
        private const string _onlyLettersRegex = "[^a-zA-z]+";
        private const char _seperator = ' ';
        private readonly ConcurrentDictionary<string, int> _wordCounts;
        private const string _leadingFileName = "file_";
        private readonly string[] _files = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Æ", "Ø", "Å" };

        public FileDataAccess(string inputDir, string outputDir, string excludedFilePath) { 
            _inputDir = inputDir;
            _wordCounts = new ConcurrentDictionary<string, int>();
            _outputDir = outputDir;
            _excludedFilePath = excludedFilePath;
            InitializeFiles(outputDir);
            
        }

        private void InitializeFiles(string outputDir)
        {
            DeleteOldFiles(outputDir);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            foreach (var fileName in _files)
            {
                var reader = File.AppendText(Path.Combine(outputDir, _leadingFileName + fileName + ".txt"));
                reader.Close();
            }
        }

        private void DeleteOldFiles(string outputDir)
        {
            if(!Directory.Exists(outputDir)) { return; }
            foreach (FileInfo file in new DirectoryInfo(outputDir).GetFiles())
            {
                file.Delete();
            }
        }

        public async Task<IDictionary<string, int>> ReadLines()
        {
            await Parallel.ForEachAsync(Directory.EnumerateFiles(_inputDir), async (file, cancellationToken) =>
            {
                using var streamReader = new StreamReader(file);
                while(!streamReader.EndOfStream) { 
                    var line = await streamReader.ReadLineAsync();
                    if (line == null || string.IsNullOrEmpty(line)) { continue; }
                    var words = line.Split(_seperator);
                    foreach (var word in words)
                    {
                        var cleanWord = Regex.Replace(word, _onlyLettersRegex, string.Empty).ToLower();
                        if (string.IsNullOrEmpty(cleanWord)) { continue; }
                        _wordCounts.AddOrUpdate(cleanWord, 1, (key, oldValue) => oldValue + 1);
                    }
                }
                streamReader.Close();
            });
            return _wordCounts;
        }

        public async Task WriteWordCount(string word, int count)
        {   
            await File.AppendAllTextAsync(Path.Combine(_outputDir, _leadingFileName+word.Substring(0,1)+".txt"), $"{word} {count}\n");
        }

        public async Task WriteExcluded(Dictionary<string, int> excludedWords, int excludedCount)
        {
            foreach(var excludedWord in excludedWords)
            {
                await File.AppendAllTextAsync(Path.Combine(_outputDir, "excluded.txt"), $"{excludedWord.Key} {excludedWord.Value}\n");
            }
            await File.AppendAllTextAsync(Path.Combine(_outputDir, "excluded.txt"), $"Total words excluded {excludedCount}");
            
        }

        public async Task<IEnumerable<string>> GetExcludedWords()
        {
            if(!File.Exists(_excludedFilePath)) 
            {
                Console.WriteLine("No file called excluded.txt found in input");
                return Enumerable.Empty<string>();
            }
            return await File.ReadAllLinesAsync(_excludedFilePath);
        }
    }
}
