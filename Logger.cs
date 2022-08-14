namespace Task1
{
    internal class Logger
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
            => $"parsed_files: {ProcessedFilesCount}\n" +
            $"parsed_lines: {ProcessedRowsCount}\n" +
            $"found_errors: {InvalidRowsPaths.Count}\n" +
            $"invalid_files:\n" +
            (InvalidRowsPaths.Count == 0 ? 0 : InvalidRowsPaths.Select(i => $"\t{i}").Aggregate((i, j) => i + "\n" + j));

        public static void Reset()
        {
            ProcessedFilesCount = 0;
            ProcessedRowsCount = 0;
            InvalidRowsPaths.Clear();
        }
    }
}
