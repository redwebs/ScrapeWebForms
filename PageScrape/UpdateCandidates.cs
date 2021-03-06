﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PageScrape
{
    public static class UpdateCandidates
    {
        #region Properties

        #endregion

        #region Variables

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // Main page "http://media.ethics.ga.gov/search/Campaign/Campaign_ByOffice.aspx";

        private const string OfficeSearchResultsUrl = "https://media.ethics.ga.gov/search/Campaign/Campaign_OfficeSearchResults.aspx";

        private static readonly NameValueCollection HiddenArguments = new NameValueCollection();

        public static List<Candidate> Candidates = new List<Candidate>();

        private static HttpResponseMessage _httpRespMsg;

        private static string _aspnetSessionId = "Not set";

        public static readonly ScrapeStatus CurrentStatus = new ScrapeStatus
        {
            TotalPages = -1,
            LastPageCompleted = 0,
            ScrapeComplete = false,
            LastOpMessage = "Static initializer",
            LoggingOn = true,
            InternalLoggingOn = true
        };

        public static long BytesReceived { get; set; }

        #endregion Variables

        #region Constants

        #endregion

        #region First page and Subsequent

        public static bool ReadFirstPage(FormSearch formSearch)
        {
            const string tgtNoResults = "//*[@id=\"ctl00_ContentPlaceHolder1_lblMessage\"]";
            const string tgtTable = "/html/body/form/table/tr[2]/td/table/tr/td[2]/div/div/div/table/tr";
            const string tgtFooter = "//*[@id=\"ctl00_ContentPlaceHolder1_pSection\"]";
            const string tgtHiddenFields = "/html/body/form/input";

            var officeNameAndId = $"Office {formSearch.OfficeName}-{formSearch.OfficeTypeId}";

            ResetStatus(true, true);        // sets TotalPages = -1
            CurrentStatus.TheUri = CreateUriWithQueryString(formSearch);
            CurrentStatus.Url = CurrentStatus.TheUri.OriginalString;

            try
            {
                _httpRespMsg = GetSearchPage(CurrentStatus.TheUri, HttpMethod.Get).Result;
                Log.Debug(ListHeaderValues());
                _aspnetSessionId = GetSessionId(_httpRespMsg);
                Log.Debug($"ASPNET_SessionId = {_aspnetSessionId}");
            }
            catch (Exception ex)
            {
                CurrentStatus.LastOpMessage = $"ReadFirstPage {officeNameAndId} exception calling GetSearchPage: {Utils.ExceptionInfo(ex)}";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.TotalPages = -2;
                return false;

            }

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"ReadFirstPage {officeNameAndId} could not retrieve URL, StatusCode: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.TotalPages = -1;
                return false;

            }
            var contentString = _httpRespMsg.Content.ReadAsStringAsync().Result;
            BytesReceived += contentString.Length;

            if (string.IsNullOrEmpty(contentString))
            {
                CurrentStatus.LastOpMessage = "ReadFirstPage {officeNameAndId} received null content";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.TotalPages = -1;
                return false;
            }

            // CurrentStatus.LastOpMessage = "ReadFirstPage received document length = " + contentString.Length;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contentString);

            // Check for "Search Returned No Results"

            var noResultsNodes = htmlDoc.DocumentNode.SelectNodes(tgtNoResults);

            if (noResultsNodes != null)
            {
                if (noResultsNodes[0].InnerHtml.Contains("Search Returned No Results."))
                {
                    CurrentStatus.LastOpMessage = $"Search Returned No Candidates for {officeNameAndId}";
                    CurrentStatus.TotalPages = 0;
                    CurrentStatus.TotalCandidates = 0;
                    CurrentStatus.ScrapeComplete = true;
                    return false;
                }
            }

            // Get Hidden Input fields from the HTML Head

            var headNodes = htmlDoc.DocumentNode.SelectNodes(tgtHiddenFields);
            StoreHidden(headNodes);
            // if you use HtmlWeb.Load(URL), run StoreHidden twice from two divs at this level

            // CurrentStatus.LastOpMessage = "StoreHidden: Arguments Added: " + HiddenArguments.Count;
            // PrintKeysAndValues(HiddenArguments);

            // Get page info from the footer

            var footerNodes = htmlDoc.DocumentNode.SelectNodes(tgtFooter);

            if (footerNodes == null)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastOpMessage = $"ReadFirstPage: FooterNodes search returned null for {officeNameAndId}";
                return false;
            }
            var footerNodesIh = footerNodes[0].InnerHtml;
            var pageAndCount = ScrapHelp.GetCurrentPageAndCount(footerNodesIh);
            // CurrentStatus.LastOpMessage = "Page: " + pageAndCount.Item1 + ", PageCount: " + pageAndCount.Item2;
            CurrentStatus.TotalPages = pageAndCount.Item2;
            var hasPageSelectorRow = pageAndCount.Item2 > 1;   // If there is only a single page there is no Page selector row
            CurrentStatus.LastOpMessage = $"ReadFirstPage: Page count for {officeNameAndId} = {pageAndCount.Item2}.";

            // Get candidate data from the table

            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtTable);

            if (nodes == null)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastOpMessage = $"ReadFirstPage: Data table search returned null for {officeNameAndId}.";
                return false;
            }
            
            var htmlDocTh = new HtmlDocument();
            htmlDocTh.LoadHtml(nodes[0].InnerHtml);

            var htmlBodyTh = htmlDocTh.DocumentNode.SelectNodes("//th");

            // Check if the column headers have changed
            const string thNames = "ActionCandidate NameElection YearDOI Filed";
            var checkStr = htmlBodyTh.Aggregate(string.Empty, (current, th) => current + th.InnerText);  // was InnerHtml for HtmlWeb.Load

            if (thNames != checkStr)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastOpMessage = $"ReadFirstPage: {officeNameAndId}, Table header mismatch, should be: {thNames} but is: {checkStr}"; 
                return false;
            }

            // Go through the table nodes

            CurrentStatus.LastOpMessage = $"ReadFirstPage: Calling ProcessTable for {officeNameAndId}, nodes.count = {nodes.Count}, hasPageSelectorRow = {hasPageSelectorRow}.";
            var rows = ProcessTable(formSearch, nodes, 1, hasPageSelectorRow);

            CurrentStatus.LastOpMessage = $"ReadFirstPage read page 1 for {officeNameAndId} with candidate count {rows}";
            CurrentStatus.LastPageCompleted = 1;

            if (CurrentStatus.TotalPages == 1)
            {
                CurrentStatus.ScrapeComplete = true;
                return false; // Last page, don't continue
            }
            // More pages to do.
            return true;
        }

        /// <summary>
        /// Returns number of candidate rows processed
        /// </summary>
        /// <param name="formSearch">FormSearch object to set querystring</param>
        /// <param name="nodes">The collection of HTML nodes</param>
        /// <param name="pageNumber">The current page number of the query results</param>
        /// <param name="hasPageSelectorRow"></param>
        /// <returns></returns>
        private static int ProcessTable(FormSearch formSearch, HtmlNodeCollection nodes, int pageNumber, bool hasPageSelectorRow = true)
        {
            var rowIndex = 0;   // Start with first candidate data row
            var lastCandidateRowIdx = nodes.Count - (hasPageSelectorRow ? 2 : 1);

            foreach (var node in nodes)
            {
                if (rowIndex == 0)
                {
                    rowIndex++;
                    continue;
                } // Already checked headers

                if (rowIndex == lastCandidateRowIdx + 1)
                {
                    // never gets here if no hasPageSelectorRow: ie single page
                    return lastCandidateRowIdx;
                } // Last row not needed

                var candidateNode = node.InnerHtml;
                var nameIdIdx =
                    candidateNode.IndexOf("NameID=", StringComparison.Ordinal) +
                    7; // .....&NameID=26758&FilerID=C2017000427&Type=candidate
                var truncatedInner = candidateNode.Substring(nameIdIdx); // 26758&FilerID=C2017000427&Type=candidate.....

                var nameIdEndIdx =
                    truncatedInner.IndexOf("&", StringComparison.Ordinal) - 1; // 26758    &FilerID=C2017000427&Type=candidate
                var filerIdStIdx =
                    truncatedInner.IndexOf("FilerID=", StringComparison.Ordinal) +
                    8; // 26758&FilerID=    C2017000427&Type=candidate
                var filerIdEndIdx =
                    truncatedInner.IndexOf("Type=", StringComparison.Ordinal) -
                    5; // 26758&FilerID=C2017000427    &Type=candidate

                var nameId = truncatedInner.Substring(0, nameIdEndIdx + 1);
                var filerId = truncatedInner.Substring(filerIdStIdx, filerIdEndIdx - filerIdStIdx);

                var candidate = new Candidate {NameId = nameId, FilerId = filerId, OfficeTypeId = formSearch.OfficeTypeId, OfficeName = formSearch.OfficeName};

                var last3TdStr = truncatedInner.Substring(truncatedInner.IndexOf("<td", StringComparison.Ordinal));
                var htmlDocTr = new HtmlDocument();

                htmlDocTr.LoadHtml("<tr>" + last3TdStr.Trim() + "</tr");
                var trBody = htmlDocTr.DocumentNode.SelectNodes("//tr");

                var tdCounter = 0;

                foreach (var td in trBody.Descendants())
                {
                    var tdText = td.InnerText.Trim();

                    //  Was idx 0,5,10 for HtmlWeb.Load
                    switch (tdCounter++)
                    {
                        case 3:
                            candidate.CandidateName = tdText;
                            break;

                        case 7:

                            var year = 0;

                            if (!Int32.TryParse(tdText, out year))
                            {
                                candidate.Notes += "Empty Election Year.";

                                if (tdText != string.Empty)
                                {
                                    CurrentStatus.LastOpMessage = $"Non-int year text: {tdText} for candidate {candidate.CandidateName}.";
                                }
                            }
                            candidate.Year = year;
                            break;

                        case 13:
                        case 16:

                            if (!DateTime.TryParse(tdText, out var dateDoiFiled))
                            {
                                candidate.Notes += "Empty DOI date.";

                                if (tdText != string.Empty)
                                {
                                    CurrentStatus.LastOpMessage = $"Non-Date DOI text: {tdText} for candidate {candidate.CandidateName}.";
                                }
                            }
                            candidate.DoiFiled = dateDoiFiled;
                            break;

                        default:
                            // Log.Info($"Process table encountered unknown tdCounter value: {tdCounter}");
                            break;
                    }
                }
                rowIndex++;

                // Get the additional data: office district, affiliation, status
                AdditionalInfo.ReadThePage(candidate);

                Candidates.Add(candidate);
            }

            return (hasPageSelectorRow ? rowIndex -1 : rowIndex -2);
        }

        public static bool ReadSubsequentPage(FormSearch formSearch)
        {
            var pageNumber = CurrentStatus.LastPageCompleted + 1;
            var officeNameAndId = $"Office {formSearch.OfficeName}-{formSearch.OfficeTypeId}, Pg {pageNumber}";

            var contentString = PostIt(CurrentStatus.TheUri, pageNumber).Result;

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"ReadSubsequentPage {officeNameAndId} call returned Status Code: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return false;
            }

            if (string.IsNullOrEmpty(contentString))
            {
                CurrentStatus.LastOpMessage = $"ReadSubsequentPage {officeNameAndId} received null content";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return false;
            }

            //CurrentStatus.LastOpMessage = "ReadSubsequentPage received document length = " + contentString.Length;

            var pipeData = contentString.Split('|');
            StorePostData(pipeData);

            const string tgtTable = "/div/div/table/tr";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pipeData[3]);

            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtTable);

            if (nodes == null)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastOpMessage = $"ReadSubsequentPage {officeNameAndId} Data table search returned null nodes.";
                CurrentStatus.LastPageCompleted++;
                return false;
            }
            var rows = ProcessTable(formSearch, nodes, pageNumber);

            //CurrentStatus.LastOpMessage = $"ReadSubsequentPage read page {pageNumber} with candidate count " + (rows - 2);
            CurrentStatus.TotalCandidates += (rows - 2);
            CurrentStatus.LastPageCompleted++;

            if (CurrentStatus.TotalPages == pageNumber)
            {
                CurrentStatus.ScrapeComplete = true;
                return false; // Last page, don't continue
            }
            // More pages to do.
            return true;
        }

        private static void StorePostData(string[] theData)
        {
            HiddenArguments.Clear();

            var dataLength = theData.Length - 2;  // text ends with a pipe

            for (var idx = 4; idx < dataLength; idx = idx + 4)
            {
                var item = new PostResponseItem
                {
                    Length = theData[idx],
                    Destination = theData[idx + 1],
                    Id = theData[idx + 2],
                    Content = theData[idx + 3]
                };
                // Log.Debug($"Arg[{idx}]: {item.Length}, {item.Destination} {item.Id}, {item.Content}");

                if (item.Destination == "hiddenField")
                {
                    HiddenArguments.Add(item.Id, item.Content);
                }
            }
            /*  Non Hidden Fields
                Arg[32]: 0, asyncPostBackControlIDs , 
                Arg[36]: 0, postBackControlIDs , 
                Arg[40]: 39, updatePanelIDs , tctl00$ContentPlaceHolder1$UpdatePanel1
                Arg[44]: 0, childUpdatePanelIDs , 
                Arg[48]: 38, panelsToRefreshIDs , ctl00$ContentPlaceHolder1$UpdatePanel1
                Arg[52]: 2, asyncPostBackTimeout , 90
                Arg[56]: 144, formAction , Campaign_OfficeSearchResults.aspx?ElectionYear=2018&County=&City=&OfficeTypeID=120&District=&Division=&FilerID=&OfficeName=State+Senate&Circuit=
                Arg[60]: 83, pageTitle , Campaign Reports Search 
                */
        }

        #endregion

        #region Network Calls

        public static async Task<HttpResponseMessage> GetSearchPage(Uri uri, HttpMethod method)
        {
            var request = new HttpRequestMessage {RequestUri = uri, Method = method};
            return await NetHttpClient.Client.SendAsync(request);
        }

        private static async Task<string> PostIt(Uri uri, int pageNum)
        {
            var formDataList = FormDataList(pageNum);
            Log.Debug(ListKeyValuePairs(formDataList, "FormData"));

            var formContent = new FormUrlEncodedContent(formDataList);

            var request = new HttpRequestMessage {RequestUri = uri, Method = HttpMethod.Post, Content = formContent};

            SetRequestHeaders(request);

            //Log.Debug(ListRequestHeaders(request));
            //Log.Debug(ListHeaderValues());

            _httpRespMsg = await NetHttpClient.Client.SendAsync(request);

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"PostIt could not retrieve URL, StatusCode: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                return string.Empty;
            }

            var stringContent = await _httpRespMsg.Content.ReadAsStringAsync();
            BytesReceived += stringContent.Length;

            return stringContent;
        }

        #endregion

        #region Post Headers and Form

        private static List<KeyValuePair<string, string>> FormDataList(int pageNum)
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ScriptManager1",
                    "ctl00$ContentPlaceHolder1$UpdatePanel1|ctl00$ContentPlaceHolder1$Search_List"),
                new KeyValuePair<string, string>("ctl00_ContentPlaceHolder1_ScriptManager1_HiddenField",
                    ";;AjaxControlToolkit, Version=3.0.20820.16598, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e:en-US:707835dd-fa4b-41d1-89e7-6df5d518ffb5:e2e86ef9:9ea3f0e2:9e8e87e9:1df13a87:4c9865be:a6a5a927"),
                new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ContentPlaceHolder1$Search_List"),
                new KeyValuePair<string, string>("__EVENTARGUMENT", $"Page${pageNum}"),
                new KeyValuePair<string, string>("__VIEWSTATE", HiddenArguments["__VIEWSTATE"]),
                new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", HiddenArguments["__VIEWSTATEGENERATOR"]),
                new KeyValuePair<string, string>("__VIEWSTATEENCRYPTED", HiddenArguments["__VIEWSTATEENCRYPTED"]),
                new KeyValuePair<string, string>("__PREVIOUSPAGE", HiddenArguments["__PREVIOUSPAGE"]),
                new KeyValuePair<string, string>("__EVENTVALIDATION", HiddenArguments["__EVENTVALIDATION"]),
                new KeyValuePair<string, string>("__ASYNCPOST", "true")

            };
        }

        private static void SetRequestHeaders(HttpRequestMessage request)
        {
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Headers.Add("accept-encoding", "gzip, deflate, br");
            request.Headers.Add("accept", "*/*");
            request.Headers.Add("accept-language", "en-US,en;q=0.9");
            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("cookie", $"ASP.NET_SessionId={_aspnetSessionId};path=/;HttpOnly");
            request.Headers.Add("origin", "https://media.ethics.ga.gov");
            request.Headers.Add("referer", CurrentStatus.TheUri.AbsoluteUri);
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-gpc", "1");
            request.Headers.Add("x-microsoftajax", "Delta=true");
            request.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
        }

        #endregion

        #region Utils

        private static string GetSessionId(HttpResponseMessage httpRespMsg)
        {
            // Get Cookie from this string:
            // Set-Cookie:	 | ASP.NET_SessionId=l1ago0vqldihpf45rj1yak55; path=/; HttpOnly
            foreach (var pair in httpRespMsg.Headers)
            {
                if (pair.Key.Equals("Set-Cookie"))
                {
                    var val = pair.Value.First();
                    // Log.Debug($"Set-Cookie = {val}");
                    var semiFirstIdx = val.IndexOf(";", StringComparison.Ordinal);
                    var equalsIdx = val.IndexOf("=", StringComparison.Ordinal);
                    // Log.Debug($"GetSessionId {val}, eq={equalsIdx}, semi={semiFirstIdx}");
                    return val.Substring(equalsIdx + 1, semiFirstIdx - equalsIdx - 1);
                }
            }

            return string.Empty;
        }

        private static void StoreHidden(HtmlNodeCollection headNodes)
        {
            var lastName = string.Empty;
            var attributeName = string.Empty;
            var attributeValue = string.Empty;

            foreach (var node in headNodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "id":
                            attributeName = attribute.Value;
                            break;
                        case "value":
                            attributeValue = attribute.Value;
                            break;
                    }
                }

                if (attributeName == lastName) continue;

                lastName = attributeName;
                HiddenArguments.Add(attributeName, attributeValue);
            }
        }

        private static Uri CreateUriWithQueryString(FormSearch formSearch)
        {
            //http://media.ethics.ga.gov/search/Campaign/Campaign_OfficeSearchResults.aspx?
            //ElectionYear=2018&County=&City=&OfficeTypeID=120&District=&Division=&FilerID=&OfficeName=State%20Senate&Circuit=

            var sb = new StringBuilder(OfficeSearchResultsUrl);
            sb.Append("?ElectionYear=");
            sb.Append(formSearch.ElectionYear);
            sb.Append("&County=");
            sb.Append(formSearch.County);
            sb.Append("&City=");
            sb.Append(formSearch.City);
            sb.Append("&OfficeTypeID=");
            sb.Append(formSearch.OfficeTypeId);
            sb.Append("&District=");
            sb.Append(formSearch.District);
            sb.Append("&Division=");
            sb.Append(formSearch.Division);
            sb.Append("&FilerID=");
            sb.Append(formSearch.FilerId);
            sb.Append("&OfficeName=");
            sb.Append(formSearch.OfficeName.Replace(" ", "%20"));
            sb.Append("&Circuit=");
            sb.Append(formSearch.Circuit);

            var url = System.Web.HttpUtility.UrlPathEncode(sb.ToString());
            return new Uri(url);
        }
        private static void ResetStatus(bool loggingOn, bool intLoggingOn)
        {
            HiddenArguments.Clear();

            CurrentStatus.TotalPages = -1;
            CurrentStatus.TotalCandidates = -1;
            CurrentStatus.LastPageCompleted = 0;
            CurrentStatus.ScrapeComplete = false;
            CurrentStatus.LoggingOn = false;  // avoid output on last msg chg
            CurrentStatus.LastOpMessage = string.Empty;
            CurrentStatus.LoggingOn = true;
            CurrentStatus.SbLog.Clear();
            CurrentStatus.InternalLoggingOn = intLoggingOn;
            CurrentStatus.LoggingOn = loggingOn;
        }
        
        #endregion

        #region ValueDisplay

        private static string ListKeyValuePairs(List<KeyValuePair<string, string>> keyValuePairs, string dataName)
        {
            var sb = new StringBuilder($"Key-Value Pairs for {dataName}:\n");

            foreach (var pair in keyValuePairs)
            {
                sb.Append(pair.Key);
                sb.Append(":\t");
                sb.Append(pair.Value);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        private static string ListHeaderValues()
        {
            var sb = new StringBuilder("Key-Value Pairs for HttpResponse Headers:\n");

            foreach (var pair in _httpRespMsg.Headers)
            {
                sb.Append(pair.Key);
                sb.Append(":\t");

                foreach (var headerVal in pair.Value)
                {
                    sb.Append(" | ");
                    sb.Append(headerVal);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private static string ListRequestHeaders(HttpRequestMessage request)
        {
            var sb = new StringBuilder($"Key-Value Pairs for HTTPRequestMessage Header:\n");

            foreach (var pair in request.Headers)
            {
                sb.Append(pair.Key);
                sb.Append(":\t");

                foreach (var val in pair.Value)
                {
                    sb.Append(val);
                    sb.Append("\n");

                }
            }

            return sb.ToString();
        }

        public static void PrintKeysAndValues(NameValueCollection myCol)
        {
            foreach (string s in myCol.AllKeys)
            {
                Log.Debug($"{s} {myCol[s]}");
            }
        }

        #endregion

    }
}
/*
 * log4net levels:
All – Log everything.
Debug.
Info.
Warn.
Error.
Fatal.
Off – Don't log anything.
Feb 9, 2017
 */
