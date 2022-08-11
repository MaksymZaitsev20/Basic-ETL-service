using System.Configuration;
using System.Text;

namespace Task1
{
    public static class ThisApplication
    {
        private static readonly object locker = new();

        public static void Run()
        {
            Console.WriteLine("ETL Service\n");
            Console.WriteLine("Press any key to exit\n\n");

            Task.Delay(DateTime.Now.AddDays(1).Date - DateTime.Now).ContinueWith(DoLogAsync());
            Task.Delay(DateTime.Now.AddMinutes(1) - DateTime.Now).ContinueWith(PrintInfoAsync());

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            Directory.CreateDirectory(Helper.GetOutputDirectory());

            if (Helper.inputFilesFolder == null || Helper.inputFilesFolder == String.Empty || Helper.GetOutputDirectory() == null)
            {
                Console.WriteLine("Файл конфігурації не знайдено або він порожній\nНатисніть будь-яку клавішу, щоб продовжити");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            if (!Directory.Exists(Helper.inputFilesFolder))
            {
                Console.WriteLine($"Директорія {Helper.inputFilesFolder} не знайдена\nНатисніть будь-яку клавішу, щоб продовжити");
                Console.ReadLine();
                Environment.Exit(-1);
            }


            string[] filters = { ".txt", ".csv" };
            var files = new DirectoryInfo(Helper.inputFilesFolder).GetFiles().Where(i => filters.Any(filter => filter == i.Extension));

            if (files.Count() > 0)
            {
                Console.WriteLine($"In {Helper.inputFilesFolder} were found {files.Count()} files\n\n");

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

            MyFileSystemWatcher.MonitorDirectory();
        }
        public static void Stop()
        {
            Console.WriteLine("\n\n");
            Console.WriteLine("Show the information log on console?");
            Console.Write("(y - yes, any - no) -> ");

            var consoleKey = Console.ReadKey().Key;

            Console.WriteLine("\n\n");

            if (consoleKey == ConsoleKey.Y)
                Console.WriteLine(Logger.GetData());

            Console.WriteLine("\n\n");

            DoLogIntoFile(Path.Combine(Helper.GetOutputDirectory(), "meta.log"));
        }

        private static Action<Task> DoLogAsync()
            => (Task task) => DoLogIntoFile(Path.Combine(DateTime.Today.AddDays(-1).ToShortDateString(), "meta.log"));
        private static void DoLogIntoFile(string path)
        {
            Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["outputFiles"], (DateTime.Today.AddDays(-1)).ToShortDateString()));
            File.WriteAllText(path, Logger.GetData());
            Logger.Reset();
            Helper.outputFileCounter = 1;
            Task.Delay(DateTime.Now.AddDays(1) - DateTime.Now).ContinueWith(DoLogAsync());
        }

        private static Action<Task> PrintInfoAsync()
            => (Task task) => PrintInfo();
        private static void PrintInfo()
        {
            Console.WriteLine("\nPress any key to exit\n");
            Task.Delay(DateTime.Now.AddMinutes(1) - DateTime.Now).ContinueWith(PrintInfoAsync());
        }
    }
}
