using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace PageScrape
{
    public class Office
    {
        [JsonProperty("OfficeTypeId")]
        public int OfficeTypeId { get; set; }
        [JsonProperty("OfficeName")]
        public string OfficeName { get; set; }
    }

    public class ScrapeSequence
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _officeNamesIdsFilePath;

        private List<FormSearch> FormSearches { get; set; } = new List<FormSearch>();

        private List<Office> Offices { get; set; }

        public List<Candidate> Candidates { get; set; } = new List<Candidate>();

        public SequenceStatus SeqStatus = new SequenceStatus
        {
            TotalSequences = -1,
            LastSequenceIdCompleted = 0,
            SequenceComplete = false,
            LastOpMessage = "Static initializer",
            LoggingOn = true,
            InternalLoggingOn = true
        };

        public ScrapeSequence(string officeNamesIdsFilePath)
        {
            _officeNamesIdsFilePath = officeNamesIdsFilePath;

        }

        public bool RunAllQueries(int year, BackgroundWorker bgWorker)
        {
            UpdateCandidates.Candidates.Clear();

            var officesCount = ReadOfficeJson();

            if (officesCount <= 0)
            {
                SeqStatus.LastOpMessage = $"ReadOfficeJson returned Offices count of: {officesCount}";
                return false;
            }

            //FormSearches.Add(new FormSearch
            //{
            //    OfficeTypeId = "220",
            //    OfficeName = "County%20Commissioner",
            //    FilerId = "C2018000126"
            //});


            foreach (var office in Offices)
            {
                FormSearches.Add(new FormSearch
                {
                    OfficeTypeId = office.OfficeTypeId.ToString(),
                    OfficeName = office.OfficeName,
                    FilerId = $"C{year}"
                });
            }
            var userStatus = new ScrapeUserStatus
            {
                OfficeCount = FormSearches.Count,
                OfficesSearched = 1,
                Message = string.Empty
            };

            bgWorker.ReportProgress(0, userStatus);

            SeqStatus.LastOpMessage = $"RunAllQueries preparing to run {FormSearches.Count} searches for {year}.";

            var queryCounter = 1;
            var queries = FormSearches.Count;

            var timer = new Stopwatch();
            timer.Start();


            foreach (var search in FormSearches)
            {
                // SeqStatus.LastOpMessage = $"RunAllQueries RunQuery num {queryCounter++} for search: {search.ToSingleLine()}.";

                try
                {
                    userStatus.ElapsedTime = timer.Elapsed.ToString();
                    userStatus.Candidate = search.OfficeName;
                    bgWorker.ReportProgress(queryCounter++ / queries * 100, userStatus);  // can add userState object to return

                    if (!RunQuery(search))
                    {
                        // Internet or some other fatal error
                        SeqStatus.SequenceFail = true;
                        return false;
                    }
                    userStatus.ElapsedTime = timer.Elapsed.ToString();
                    bgWorker.ReportProgress(queryCounter++/queries * 100, userStatus);  // can add userState object to return

                    if (bgWorker.CancellationPending)
                    {
                        userStatus.Cancelled = true;
                        bgWorker.ReportProgress(queryCounter++ / queries * 100, userStatus);
                        break;
                    }

                }
                catch (Exception ex)
                {
                    Log.Error($"RunAllQueries threw an exception: {ex.Message}");
                    throw;
                }
            }

            SeqStatus.BytesReceived = UpdateCandidates.BytesReceived;
            userStatus.BytesReceived = UpdateCandidates.BytesReceived;

            return true;
        }

        private bool RunQuery(FormSearch search)
        {
            SeqStatus.TheFormSearch = search;

            if (!UpdateCandidates.ReadFirstPage(search))
            {
                // Don't continue, say why

                switch (UpdateCandidates.CurrentStatus.TotalPages)
                {
                    case -2:
                        // Problem with internet connection
                        SeqStatus.LastOpMessage = 
                            $"RunQuery: Fail in first page search for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}: {UpdateCandidates.CurrentStatus.LastOpMessage}";
                        SeqStatus.SequenceFail = true;
                        return false;

                    case -1:
                        // Problem with search: Could not retrieve URL, null content
                        SeqStatus.LastOpMessage = 
                            $"RunQuery: Fail in first page search for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}: {UpdateCandidates.CurrentStatus.LastOpMessage}";
                        break;

                    case 0:
                        // No candidates found in category
                        SeqStatus.LastOpMessage = 
                            $"RunQuery: No candidates found for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}.";
                        break;

                    case 1:
                        // Only one page of results
                        SeqStatus.LastOpMessage = 
                            $"RunQuery: Found {UpdateCandidates.Candidates.Count} for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}.";
                        break;

                    default:
                        SeqStatus.LastOpMessage =
                            $"RunQuery: ReadFirstPage said don't continue for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}, PageCount: {UpdateCandidates.CurrentStatus.TotalPages}: Should never get here!";
                        break;
                }
            }

            while (UpdateCandidates.CurrentStatus.LastPageCompleted < UpdateCandidates.CurrentStatus.TotalPages)
            {
                // SeqStatus.LastOpMessage = $"RunQuery: Reading subsequent page {pageCounter++} for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}."; 
                var finished = UpdateCandidates.ReadSubsequentPage(search);
            }

            var candidates = UpdateCandidates.Candidates;

            //SeqStatus.LastOpMessage = 
            //    $"RunQuery: Finished query for {search.OfficeName}, officeTypeId: {search.OfficeTypeId}, Candidate Count: {UpdateCandidates.CurrentStatus.TotalCandidates}";

            return true;
        }

        private int ReadOfficeJson()
        {
            // OfficeTypeIds: 1-199 = State, 200-499 = County, 500-599 = Municipal

            string json;

            try
            {
                json = System.IO.File.ReadAllText(_officeNamesIdsFilePath);
            }
            catch (IOException ex)
            {
                Log.Error($"ReadOfficeJson: Cannot read json file at location {_officeNamesIdsFilePath}, ex: {ex.Message}");
                return -1;
            }

            Offices = JsonConvert.DeserializeObject<List<Office>>(json);

            return Offices.Count;
        }
    }

}
