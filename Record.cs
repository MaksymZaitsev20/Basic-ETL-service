using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Task1
{
    class Record
    {
        [JsonProperty(Order = -2)]
        public string City { get; set; }
        public List<Service> Services;
        public decimal Total { get; set; }

        public class Service
        {
            [JsonProperty(Order = -2)]
            public string Name { get; set; }
            public List<Payer> Payers;
            public decimal Total { get; set; }
            public class Payer
            {
                public string Name { get; set; }
                public decimal Payment { get; set; }
                public string Date { get; set; }
                public long AccountNumber { get; set; }

                public Payer()
                {
                    Name = String.Empty;
                    Payment = 0;
                    Date = String.Empty;
                    AccountNumber = 0;
                }
            }

            public Service()
            {
                Name = String.Empty;
                Total = 0;
                Payers = new();
            }
        }

        public Record()
        {
            City = String.Empty;
            Total = 0;
            Services = new();
        }
    }
}
