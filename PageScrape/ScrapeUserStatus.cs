using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageScrape
{
    public class ScrapeUserStatus
    {
        public int OfficeCount { get; set; }

        public int OfficesSearched { get; set; }

        public string Message { get; set; }

        public string Candidate { get; set; }

        public string ElapsedTime { get; set; }

        public bool Cancelled { get; set; } = false;

        public long BytesReceived { get; set; } = 0;
    }
}
