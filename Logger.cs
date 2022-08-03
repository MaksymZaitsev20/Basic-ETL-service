using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Task1
{
    static class Logger
    {
        public static int ProcessedFilesCount { get; set; }
        public static int ProcessedRowsCount { get; set; }
        public static List<string> InvalidRowsPaths { get; set; }

        static Logger()
        {
            ProcessedFilesCount = 0;
            ProcessedRowsCount = 0;
            InvalidRowsPaths = new();
        }
        
        public static string GetData()
        {
            return $"parsed_files: {ProcessedFilesCount}\n" +
                $"parsed_lines: {ProcessedRowsCount}\n" +
                $"found_errors: {InvalidRowsPaths.Count}\n" +
                $"invalid_files:\n" +
                InvalidRowsPaths.Select(i => $"\t{i}").Aggregate((i, j) => i + "\n" + j);
        }

        public static void Reset()
        {
            ProcessedFilesCount = 0;
            ProcessedRowsCount = 0;
            InvalidRowsPaths.Clear();
        }
    }
}
