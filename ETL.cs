using Newtonsoft.Json;
using System.Globalization;

namespace Task1
{
    internal class ETL
    {
        private static readonly object locker1 = new();
        private static readonly object locker2 = new();
        private static readonly object locker3 = new();
        private static readonly object locker4 = new();

        public static string[] Extract(string path)
        {
            return path.EndsWith(".txt") ? File.ReadAllLines(path).ToArray() : File.ReadLines(path).Skip(1).ToArray();
        }
        public static string TransformData(string[] strings, string path)
        {
            List<Record> records = new();

            for (int i = 0; i < strings.Length; i++)
            {
                lock (locker1)
                {
                    Logger.ProcessedRowsCount++;
                }

                var fields = strings[i].Split(',');

                for (int j = 0; j < fields.Length; j++)
                {
                    fields[j] = fields[j].Trim(' ', '”', '“', '\'', '`', '"');
                }

                if (fields.Length < 9)
                {
                    lock (locker2)
                    {
                        Logger.InvalidRowsPaths.Add($"{path}: line {i + 1}");
                    }
                    continue;
                }

                string firstName = fields[0] ?? String.Empty;
                string lastName = fields[1] ?? String.Empty;
                string city = fields[2] ?? String.Empty;
                decimal payment;
                string date = fields[6] ?? String.Empty;
                long accountNumber;
                string serviceName = fields[8] ?? String.Empty;

                if ((firstName == String.Empty && lastName == String.Empty)
                 || city == String.Empty
                 || !(decimal.TryParse(fields[5], NumberStyles.Any, CultureInfo.InvariantCulture, out payment))
                 || date == String.Empty
                 || !long.TryParse(fields[7], out accountNumber)
                 || serviceName == String.Empty)
                {
                    lock (locker3)
                    {
                        Logger.InvalidRowsPaths.Add($"{path}: line {i + 1}");
                    }
                    continue;
                }

                if (!records.Any(i => i.City == city))
                {
                    records.Add(new Record
                    {
                        City = city,
                    });
                }

                var record = records.Find(i => i.City == city);
                record.Total += payment;

                if (!record.Services.Any(i => i.Name == serviceName))
                {
                    record.Services.Add(new Record.Service
                    {
                        Name = serviceName,
                    });
                }

                var service = record.Services.Find(i => i.Name == serviceName);
                service.Total += payment;

                service.Payers.Add(new Record.Service.Payer
                {
                    Name = firstName + (lastName == String.Empty ? String.Empty : " " + lastName),
                    Payment = payment,
                    Date = date,
                    AccountNumber = accountNumber
                });
            }

            return JsonConvert.SerializeObject(records, Formatting.Indented);
        }
        public static void LoadData(string data, string path)
        {
            lock (locker4)
            {
                File.WriteAllText(path, data);
            }
        }
    }
}
