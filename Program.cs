using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    public class Program
    {
        static int outputFileCounter = 1;
        static object locker = new();

        public static void Main(string[] args)
        {
            Task.Delay(DateTime.Now.AddSeconds(20) - DateTime.Now).ContinueWith(DoLogAsync());

            // UTF8 encoding for normal ukrainian characters vizualization
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;


            // Checking for the presence of files in the directory at the time of program launch

            // Path to folder "A" (input files) from App.config key="inputFiles"
            string inputFiles = ConfigurationManager.AppSettings["inputFiles"];

            Directory.CreateDirectory(GetOutputDirectory());

            // If config file not found or empty, close app
            if (inputFiles == null || GetOutputDirectory() == null)
            {
                Console.WriteLine("Файл конфігурації не знайдено або він порожній\nНатисніть будь-яку клавішу, щоб продовжити");
                Console.ReadLine();
                return;
            }

            List<Task> tasks = new();

            // Checking .txt files
            foreach (var file in new DirectoryInfo(inputFiles).GetFiles("*.txt"))
            {
                tasks.Add(
                    Task.Run(new Action(() =>
                        ETL.LoadData(
                            ETL.TransformData(
                                ETL.ExtractFromTxtFile(
                                    file.FullName),
                                file.FullName),
                            Path.Combine(
                                GetOutputDirectory(),
                                GetFileName())))));

                Console.WriteLine($"Process {file.FullName} in thread #{tasks.Last().Id}");

                Logger.ProcessedFilesCount++;
            }


            // Checking new files in directory after app running

            List<FileSystemWatcher> watchers = new();

            // Files extentions for checking
            string[] filters = { "*.txt", "*.csv" };

            //foreach (var filter in filters)
            //{
            //    var watcher = new FileSystemWatcher(inputFiles);

            //    //watcher.NotifyFilter = NotifyFilters.Attributes
            //    //                     | NotifyFilters.CreationTime
            //    //                     | NotifyFilters.DirectoryName
            //    //                     | NotifyFilters.FileName
            //    //                     | NotifyFilters.LastAccess
            //    //                     | NotifyFilters.LastWrite
            //    //                     | NotifyFilters.Security
            //    //                     | NotifyFilters.Size;

            //    watcher.Created += OnCreated;

            //    watcher.Filter = filter;
            //    watcher.EnableRaisingEvents = true;

            //    watchers.Add(watcher);
            //}

            var watcher = new FileSystemWatcher(inputFiles);

            watcher.Created += OnCreated;

            watcher.Filter = "*.txt";
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();

            Task.WaitAll(tasks.ToArray());
        }

        public static Action<Task> DoLogAsync()
        {
            return (Task task) => DoLog();
        }

        public static void DoLog()
        {
            Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["outputFiles"], (DateTime.Today.AddDays(-1)).ToShortDateString()));
            File.WriteAllText($"{Path.Combine(ConfigurationManager.AppSettings["outputFiles"], (DateTime.Today.AddDays(-1)).ToShortDateString(), "meta.log")}", Logger.GetData());
            Logger.Reset();
            outputFileCounter = 1;
            Task.Delay(DateTime.Now.AddDays(1) - DateTime.Now).ContinueWith(DoLogAsync());
        }

        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string[] data = null;

            if (e.Name.EndsWith(".txt"))
                data = ETL.ExtractFromTxtFile(e.FullPath);
            else if (e.Name.EndsWith(".csv"))
                data = ETL.ExtractFromCsvFile(e.FullPath);
            else
                return;

            var task = Task.Run(new Action(() =>
                ETL.LoadData(
                    ETL.TransformData(data, e.FullPath),
                    Path.Combine(GetOutputDirectory(), GetFileName())
                    )));

            Console.WriteLine($"Process {e.FullPath} in thread #{task.Id}");

            Logger.ProcessedFilesCount++;
        }

        public static string GetOutputDirectory()
        {
            return $"{Path.Combine(ConfigurationManager.AppSettings["outputFiles"], DateTime.Today.ToShortDateString())}";
        }

        public static string GetFileName()
        {
            lock (locker)
            {
                return $"output{outputFileCounter++}.json";
            }
        }
    }
}