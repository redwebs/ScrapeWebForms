using System;
using System.Net.Http;
using HtmlAgilityPack;

namespace PageScrape
{
    public static class AdditionalInfo
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly ScrapeStatus AddInfoStatus = new ScrapeStatus
        {
            TotalPages = 1,
            LastPageCompleted = 0,
            ScrapeComplete = false,
            LastOpMessage = "Static initializer",
            LoggingOn = true,
            InternalLoggingOn = true
        };

        private static void ResetStatus(bool loggingOn, bool intLoggingOn)
        {
            AddInfoStatus.TotalCandidates = 1;
            AddInfoStatus.LastPageCompleted = 0;
            AddInfoStatus.ScrapeComplete = false;
            AddInfoStatus.LoggingOn = false;
            AddInfoStatus.LastOpMessage = "Reset Status";
            AddInfoStatus.LoggingOn = true;
            AddInfoStatus.SbLog.Clear();
            AddInfoStatus.InternalLoggingOn = intLoggingOn;
            AddInfoStatus.LoggingOn = loggingOn;
        }

        public static bool ReadThePage(Candidate candidate)
        {
            ResetStatus(true, true);
            AddInfoStatus.Url = candidate.InfoUrl;
            AddInfoStatus.TheUri = new Uri(candidate.InfoUrl);

            var httpRespMsg = UpdateCandidates.GetSearchPage(AddInfoStatus.TheUri, HttpMethod.Get).Result;

            if (!httpRespMsg.IsSuccessStatusCode)
            {
                AddInfoStatus.LastOpMessage = $"ReadThePage could not retrieve URL, StatusCode: {httpRespMsg.StatusCode}";
                AddInfoStatus.ScrapeComplete = true;
                return false;

            }
            var contentString = httpRespMsg.Content.ReadAsStringAsync().Result;
            UpdateCandidates.BytesReceived += contentString.Length;


            if (string.IsNullOrEmpty(contentString))
            {
                AddInfoStatus.LastOpMessage = "ReadThePage received null content";
                AddInfoStatus.ScrapeComplete = true;
                return false;
            }

            // AddInfoStatus.LastOpMessage = "ReadThePage received document length = " + contentString.Length;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contentString);

            // Check for "CAMPAIGN REPORTS - NAME SEARCH" - indicates no Results
            const string tgtNoResults = "/html/body/form/table/tr[2]/td/table/tr/td[2]/div/h3";

            var noResultsNodes = htmlDoc.DocumentNode.SelectNodes(tgtNoResults);

            if (noResultsNodes != null)
            {
                if (noResultsNodes[0].InnerHtml.Contains("CAMPAIGN REPORTS - NAME SEARCH"))
                {
                    AddInfoStatus.LastOpMessage = "Additional Info Search Returned No Data.";
                    AddInfoStatus.TotalPages = 0;
                    AddInfoStatus.TotalCandidates = 0;
                    AddInfoStatus.ScrapeComplete = true;
                    return false;
                }
            }

            // Get candidate data from the table
            const string tgtTable = 
                "/html/body/form/table/tr[2]/td/table/tr/td[2]/div/table/tr/td/div[2]/table[2]/tr/td/div/table/tr[2]";

            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtTable);

            if (nodes == null)
            {
                AddInfoStatus.ScrapeComplete = true;
                AddInfoStatus.LastOpMessage = $"Additional Data table search returned null, FilerId: {candidate.FilerId}, Candidate Name: {candidate.CandidateName}";
                return false;
            }

            var filerId = nodes[0].ChildNodes[1].InnerText.Trim();

            if (filerId != candidate.FilerId)
            {
                // Just make a note of it and continue
                AddInfoStatus.LastOpMessage = $"FilerId mismatch, first row: {filerId}, input: {candidate.FilerId}.";
                candidate.Notes += $"Different FilerId on info page: {filerId}.";
            }

            var txt = nodes[0].ChildNodes[2].InnerText.TrimStart().TrimEnd();

            ParseOfficeName(txt, candidate);
            candidate.Status = nodes[0].ChildNodes[4].InnerText.Trim();
            
            // Now get affiliation from Registration Information tab
            const string tgtCampaignRegInfo =
                "/html/body/form/table/tr[2]/td/table/tr/td[2]/div/table/tr/td/div[2]/div/div/div[2]/div[2]/div/div/table/tr/td/div/table";

            var campaignRegInfo = htmlDoc.DocumentNode.SelectNodes(tgtCampaignRegInfo);

            if (campaignRegInfo == null)
            {
                AddInfoStatus.ScrapeComplete = true;
                AddInfoStatus.LastOpMessage = "Campaign Registration Info Tab search returned null.";
                return false;
            }

            candidate.Affiliation = campaignRegInfo[5].ChildNodes[0].InnerText;

            AddInfoStatus.ScrapeComplete = true;
            AddInfoStatus.LastPageCompleted = 1;

            return true;
        }

        private static void ParseOfficeName(string pageTxt, Candidate candidate)
        {
            var array = pageTxt.Split('\n');
            candidate.OfficeName = array[0];
            if (array.Length == 1) return;

            switch (Convert.ToInt32(candidate.OfficeTypeId))
            {
                case 10: //  "Governor"
                    break;

                case 20: //  "Lieutenant Governor"
                    break;

                case 30: //  "Attorney General"
                    break;

                case 40: //  "Secretary Of State"
                    break;

                case 50: //  "Commissioner of Insurance"
                    break;

                case 60: //  "Commissioner of Agriculture"
                    break;

                case 70: //  "State School Superintendent"
                    break;

                case 80: //  "Commissioner of Labor"
                    break;

                case 100: //  "Public Service Commissioner"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 120: //  "State Senate"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 130: //  "State Representative"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 150: //  "Supreme Court"
                    break;

                case 160: //  "Court of Appeals"
                    break;

                case 170: //  "Judge Superior Court"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 180: //  "District Attorney"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 199: //  "Other State Office"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 200: //  "Clerks of Superior Court"
                    CheckNameCounty(candidate, array);
                    break;

                case 210: //  "Coroners"
                    CheckNameCounty(candidate, array);
                    break;

                case 220: //  "County Commissioner"
                    CheckNameCounty(candidate, array);
                    break;

                case 230: //  "Court Tax Collector"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 240: //  "Judge of Civil Court"
                    candidate.OfficeArea = array[1].Trim();
                    break;

                case 250: //  "Judge of Probate Court"
                    CheckNameCounty(candidate, array);
                    break;

                case 260: //  "Judges of Recorders"
                    CheckNameCounty(candidate, array);
                    break;

                case 270: //  "Magistrates"
                    CheckNameCounty(candidate, array);
                    break;

                case 280: //  "School Board Member"
                    CheckNameCounty(candidate, array);
                    break;

                case 290: //  "Sheriff"
                    CheckNameCounty(candidate, array);
                    break;

                case 300: //  "Solicitor"
                    CheckNameCounty(candidate, array);
                    break;

                case 310: //  "State Court Judge"
                    CheckNameCounty(candidate, array);
                    break;

                case 320: //  "Surveyor"
                    CheckNameCounty(candidate, array);
                    break;

                case 330: //  "Tax Assessor"
                    CheckNameCounty(candidate, array);
                    break;

                case 340: //  "Tax Commissioner"
                    CheckNameCounty(candidate, array);
                    break;

                case 350: //  "Tax Receiver"
                    CheckNameCounty(candidate, array);
                    break;

                case 499: //  "Other County Office"
                    CheckNameCounty(candidate, array);
                    break;

                case 500: //  "Aldermen"  ???
                    CheckNameCity(candidate, array);
                    break;

                case 510: //  "Clerk of Municipal Court"   ???
                    CheckNameCity(candidate, array);
                    break;

                case 520: //  "Councilman"
                    CheckNameCity(candidate, array);
                    break;

                case 530: //  "Judge of Municipal Court"   ???
                    CheckNameCity(candidate, array);
                    break;

                case 540: //  "Marshall of Municipal Court"   ???
                    CheckNameCity(candidate, array);
                    break;

                case 550: //  "Mayor"
                    CheckNameCity(candidate, array);
                    break;

                case 599: //  "Other Municipal Office"
                    CheckNameCity(candidate, array);
                    break;

                default:
                    break;
            }
        }

        private static void CheckNameCounty(Candidate candidate, string[] array)
        {
            if (array[1].Contains("County:"))
            {
                candidate.County = array[1].Replace("County:", string.Empty).Trim();
            }
            else
            {
                candidate.OfficeArea = array[1].Trim();

                if (array.Length > 2)
                {
                    candidate.County = array[2].Replace("County:", string.Empty).Trim();
                }
            }
        }

        private static void CheckNameCity(Candidate candidate, string[] array)
        {
            if (array[1].Contains("City:"))
            {
                candidate.City = array[1].Replace("City:", string.Empty).Trim();
            }
            else
            {
                candidate.OfficeArea = array[1].Trim();
                if (array.Length > 2)
                {
                    candidate.City = array[2].Replace("City:", string.Empty).Trim();
                }
            }
        }
    }
}
