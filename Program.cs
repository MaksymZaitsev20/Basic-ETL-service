using System.Configuration;
using System.Text;
using Task1;



Console.WriteLine("ETL Service\n");
Console.WriteLine("Press any key to exit\n\n");

object locker = new();

Task.Delay(DateTime.Now.AddDays(1).Date - DateTime.Now).ContinueWith(DoLogAsync());
Task.Delay(DateTime.Now.AddMinutes(1) - DateTime.Now).ContinueWith(PrintInfoAsync());

// UTF8 encoding for normal ukrainian characters vizualization
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;


// Checking for the presence of files in the directory at the time of program launch

Directory.CreateDirectory(Helper.GetOutputDirectory());

// If config file not found or empty, close app
if (Helper.inputFiles == null || Helper.inputFiles == String.Empty || Helper.GetOutputDirectory() == null)
{
    Console.WriteLine("Файл конфігурації не знайдено або він порожній\nНатисніть будь-яку клавішу, щоб продовжити");
    Console.ReadLine();
    return;
}

if (!Directory.Exists(Helper.inputFiles))
{
    Console.WriteLine($"Директорія {Helper.inputFiles} не знайдена\nНатисніть будь-яку клавішу, щоб продовжити");
    Console.ReadLine();
    return;
}

// Checking files
string[] filters = { ".txt", ".csv" };
var files = new DirectoryInfo(Helper.inputFiles).GetFiles().Where(i => filters.Any(filter => filter == i.Extension));

if (files.Count() > 0)
{
    Console.WriteLine($"In {Helper.inputFiles} were found {files.Count()} files\n\n");

    foreach (var file in files)
    {
        var id = Task.Run(new Action(() =>
        ETL.LoadData(
            ETL.TransformData(
                ETL.Extract(file.FullName),
                file.FullName),
            Path.Combine(
                Helper.GetOutputDirectory(),
                Helper.GetFileName())))).Id;

        lock (locker)
        {
            Console.WriteLine($"Process {file.FullName} in thread #{id}");
        }

        Logger.ProcessedFilesCount++;
    }
}



MyFileSystemWatcher.MonitorDirectory(Helper.inputFiles, "*.txt");


Console.ReadKey();

Console.WriteLine("\n\n");
Console.WriteLine("Show the information log on console?");
Console.Write("(y - yes, any - no) -> ");
var consoleKey = Console.ReadKey().Key;
Console.WriteLine("\n\n");

if (consoleKey == ConsoleKey.Y)
{
    Console.WriteLine(Logger.GetData());
}
Console.WriteLine("\n\n");

DoLogIntoFile(Path.Combine(Helper.GetOutputDirectory(), "meta.log"));



Action<Task> DoLogAsync()
{
    return (Task task) => DoLogIntoFile(Path.Combine(DateTime.Today.AddDays(-1).ToShortDateString(), "meta.log"));
}

void DoLogIntoFile(string path)
{
    Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["outputFiles"], (DateTime.Today.AddDays(-1)).ToShortDateString()));
    File.WriteAllText(path, Logger.GetData());
    Logger.Reset();
    Helper.outputFileCounter = 1;
    Task.Delay(DateTime.Now.AddDays(1) - DateTime.Now).ContinueWith(DoLogAsync());
}


Action<Task> PrintInfoAsync() => (Task task) => PrintInfo();

void PrintInfo()
{
    Console.WriteLine("\nPress any key to exit\n");
    Task.Delay(DateTime.Now.AddMinutes(1) - DateTime.Now).ContinueWith(PrintInfoAsync());
}