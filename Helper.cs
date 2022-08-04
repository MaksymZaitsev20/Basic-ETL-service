using System.Configuration;

namespace Task1
{
    internal static class Helper
    {
        public static int outputFileCounter = 1;

        // Path to folder "A" (input files) from App.config key="inputFiles"
        public static string inputFiles = ConfigurationManager.AppSettings["inputFiles"];
        public static string outputFiles = ConfigurationManager.AppSettings["outputFiles"];

        public static string GetOutputDirectory()
        {
            return $"{Path.Combine(outputFiles, DateTime.Today.ToShortDateString())}";
        }

        public static string GetFileName()
        {
            return $"output{outputFileCounter++}.json";
        }
    }
}
