// See https://aka.ms/new-console-template for more information
using WordCounter.DataAccess;

switch (args.Length)
{
    case 0:
        Console.WriteLine("Please input path to input files");
        break;
    case 1:
        var outputFolder = Path.Combine(args[0], "output");
        IDataAccess dataAccess = new FileDataAccess(args[0], outputFolder, Path.Combine(args[0], "excluded.txt"));
        var wordCounter = new WordCounter.WordCounter(dataAccess);
        await wordCounter.ProcessData();
        break;
    default:
        Console.WriteLine("Too many command line arguments");
        break;
}