using System.Configuration;

namespace Task1
{
    internal static class Helper
    {
        public static int outputFileCounter = 1;
        private static readonly object locker1 = new();

        // Path to folder "A" (input files) from App.config key="inputFiles"
        public static string inputFilesFolder = ConfigurationManager.AppSettings["inputFiles"];
        public static string outputFilesFolder = ConfigurationManager.AppSettings["outputFiles"];

        public static string GetOutputDirectory()
            => $"{Path.Combine(outputFilesFolder, DateTime.Today.ToShortDateString())}";

        public static string GetFileName()
        {
            string fileName;
            lock (locker1)
            {
                fileName = $"output{outputFileCounter++}.json";
            }
            return fileName;
        }
    }
}
