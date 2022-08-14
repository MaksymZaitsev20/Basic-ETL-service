namespace Task1
{
    internal static class MyFileSystemWatcher
    {
        public static List<FileSystemWatcher> watchers = new();

        private static readonly object locker1 = new();
        private static readonly object locker2 = new();
        private static readonly object locker3 = new();

        /// <summary>
        /// Method to monitor directory
        /// </summary>
        public static void MonitorDirectory()
        {
            // Files extentions for checking
            string[] filters = { "*.txt", "*.csv" };

            foreach (var filter in filters)
            {
                var watcher = new FileSystemWatcher(Helper.inputFilesFolder);

                watcher.Created += OnCreated;

                watcher.Filter = filter;
                watcher.EnableRaisingEvents = true;

                watchers.Add(watcher);
            }
        }

        /// <summary>
        /// Event handler for FileSystemWatcher.Created event
        /// </summary>
        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            // Waiting for the end of file copying
            // TODO: the file was not copied, but the program is already starting to process it, which causes exceptions - TO FIX IT!
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
