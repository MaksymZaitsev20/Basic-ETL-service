namespace Task1
{
    internal static class MyFileSystemWatcher
    {
        public static FileSystemWatcher watcher;
        private static readonly object locker1 = new();
        private static readonly object locker2 = new();
        private static readonly object locker3 = new();
        public static void MonitorDirectory(string path, string filter)
        {
            #region Watchers list
            // Checking new files in directory after app running

            //List<FileSystemWatcher> watchers = new();

            // Files extentions for checking
            // string[] filters = { "*.txt", "*.csv" };

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
            #endregion

            watcher = new FileSystemWatcher(Helper.inputFiles);

            watcher.Created += OnCreated;

            watcher.Filter = "*.txt";
            watcher.EnableRaisingEvents = true;
        }

        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            // Waiting for the end of file copying
            /// TODO: the file was not copied, but the program is already starting to process it, which causes exceptions - TO FIX IT!
            Thread.Sleep(25);

            string[] data;

            try
            {
                if (e.Name.EndsWith(".txt") || e.Name.EndsWith(".csv"))
                    data = ETL.Extract(e.FullPath);
                else
                    return;
            }
            catch (Exception)
            {
                lock (locker1)
                {
                    Console.WriteLine($"{e.Name} not processed");
                }
                return;
            }

            var transformedData = ETL.TransformData(data, e.FullPath);

            lock (locker2)
            {
                ETL.LoadData(transformedData, Path.Combine(Helper.GetOutputDirectory(), Helper.GetFileName()));
            }

            lock (locker3)
            {
                Console.WriteLine($"Process {e.FullPath} in thread #{Thread.CurrentThread.ManagedThreadId}");
            }
            Logger.ProcessedFilesCount++;
        }
    }
}
