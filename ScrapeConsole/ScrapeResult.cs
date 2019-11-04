using System.Collections.Generic;
using PageScrape;

namespace ScrapeConsole
{
    // The background worker returns this object to the UI

    public class ScrapeResult
    {
        public bool ErrorEncountered { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;

        public int CandidatesScraped { get; set; } = 0;

        public List<Candidate> Candidates { get; set; }

        public string ElapsedTime { get; set; } = string.Empty;
    }
}
