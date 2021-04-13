using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using log4net.Config;
using PageScrape;


namespace ScrapeConsole
{
    // GA Gov Transparency and Campaign Finance Commission Web Scrape
    // Search form at http://media.ethics.ga.gov/search/Campaign/Campaign_ByOffice.aspx?
    // First page is a get to: http://media.ethics.ga.gov/search/Campaign/Campaign_OfficeSearchResults.aspx?ElectionYear=2018&County=&City=&OfficeTypeID=120&District=&Division=&FilerID=&OfficeName=State%20Senate&Circuit=
    public partial class FormMain : Form
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string DnsNotResolved = "The remote name could not be resolved";
        private static string LastStat = string.Empty;
        private List<Candidate> _candidateList;

        #region Form

        // ++++++++++++++++++++++ Form declaration and load ++++++++++++++++++++++

        public FormMain()
        {
            InitializeComponent();
            XmlConfigurator.Configure();
            Log.Debug("-------------- Started program ScrapeConsole"); 

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Tell backgroundWorker to support cancellations
            backgroundWorkerScrape.WorkerSupportsCancellation = true;

            // Tell backgroundWorker to report progress
            backgroundWorkerScrape.WorkerReportsProgress = true;

            // Fill combo with years
            var thisYear = DateTime.Now.Year;
            var savedYear = Properties.Settings.Default.YearToScrape;
            var yearIdx = 0;

            for (var year = thisYear - 5; year < thisYear + 3; year++)
            {
                cboYear.Items.Add(new CtrlListItem(year.ToString(),year));

                if (savedYear.Equals(year))
                {
                    cboYear.SelectedIndex = yearIdx;
                }
                yearIdx++;
            }

            tbCsvFilePath.Text = Properties.Settings.Default.CsvFilePath;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cboYear.SelectedItem == null)
            {
                Properties.Settings.Default.YearToScrape = DateTime.Now.Year;
            }
            else
            {
                Properties.Settings.Default.YearToScrape = ((CtrlListItem)cboYear.SelectedItem).ItemData;
            }

            Properties.Settings.Default.CsvFilePath = tbCsvFilePath.Text;
            Properties.Settings.Default.Save();
        }


        #endregion Form

        #region Background Worker Events

        // +++++++++++++++++++++++ Background worker events +++++++++++++++++++++++

        private void BackgroundWorkerScrape_DoWork(object sender, DoWorkEventArgs e)
        {
            var sendingWorker = (BackgroundWorker)sender;   // Capture the BackgroundWorker that fired the event
            var arrObjects = (object[])e.Argument;          // Collect the array of objects the we received from the main thread
            var year = (int)arrObjects[0];                  // Get the numeric value from inside the objects array, don't forget to cast
            var timer = Stopwatch.StartNew();

            // Test functions:
            //  RunSingleQuery();
            //  TestAdditionalInfo();

            var scrapeResult = new ScrapeResult
            {
                Candidates = UpdateCandidates.Candidates,
            };

            var seqStatus = RunAllQueries(year, sendingWorker);
            scrapeResult.SequenceStat = seqStatus;
            scrapeResult.ElapsedTime = timer.Elapsed.ToString();
            e.Result = scrapeResult;
        }

        private void BackgroundWorkerScrape_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update UI
            var status = (ScrapeUserStatus) e.UserState;
            progressBar1.Maximum = status.OfficeCount;
            progressBar1.Increment(status.OfficesSearched);
            tbStatus.Text = $"Candidate: {status.Candidate}";

            var thisStat = $"{status.ElapsedTime} Candidate: {status.Candidate}";

            if (thisStat != LastStat)
            {
                AppendLogBox(thisStat);
                LastStat = thisStat;
            }

            if (!string.IsNullOrEmpty(status.Message))
            {
                AppendLogBox(status.Message);
            }

            if (status.Cancelled)
            {
                AppendLogBox($"User cancelled the scrape.");
            }
        }

        private void BackgroundWorkerScrape_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (ScrapeResult)e.Result;

            if (e.Error != null)
            {
                // An error occurred
                tbStatus.Text = $"An error occurred: {e.Error.Message}";
            }
            else if (e.Cancelled)
            {
                // The process was cancelled
                tbStatus.Text = " Job cancelled.";
            }
            else
            {
                if (result.SequenceStat.SequenceFail)
                {
                    var lastMsg = result.SequenceStat.LastOpMessage;

                    if (lastMsg.Contains(DnsNotResolved))
                    {
                        tbStatus.Text = $" Job aborted, DNS lookup failed for site, check Internet connection. Elapsed Time: {result.ElapsedTime}";
                    }
                    else
                    {
                        // Some other fatal error
                        tbStatus.Text = $" Job aborted, {lastMsg} Candidates, Elapsed Time: {result.ElapsedTime}";
                    }

                }
                else
                {
                    // The process finished
                    _candidateList = (List<Candidate>) result.Candidates;
                    tbStatus.Text = $" Job finished, {_candidateList.Count} Candidates, Elapsed Time: {result.ElapsedTime}";
                    dataGridViewScrape.DataSource = _candidateList;
                }
            }
            AppendLogBox($"Total Bytes Read: {result.SequenceStat.BytesReceived:###,###}");
            AppendLogBox(tbStatus.Text);
            btnStart.Enabled = true;
        }

        #endregion Background Worker Events

        // ++++++++++++++++++++++ Background Worker Functions +++++++++++++++++++++

        #region Scrape Testing and Development

        private void TestAdditionalInfo()
        {
            // Test function to get additional info for a single office

            var candidate = new Candidate
            {
                NameId = "11068",
                FilerId = "C2018001171",
                OfficeTypeId = "130",
                OfficeName = "Madison Fain Barton"
            };

            backgroundWorkerScrape.ReportProgress(1, "Begin TestAdditionalInfo : " + Environment.NewLine );

            if (!AdditionalInfo.ReadThePage(candidate))
            {
                backgroundWorkerScrape.ReportProgress(1, AdditionalInfo.AddInfoStatus.LastOpMessage);
                return;
            }
            
            backgroundWorkerScrape.ReportProgress(2, AdditionalInfo.AddInfoStatus.LastOpMessage + Environment.NewLine);
        }

        private void RunSingleQuery()
        {
            // Test function to scrape a single office

            var search = new FormSearch
            {
                ElectionYear = "2018",
                OfficeTypeId = "120",
                OfficeName = "State%20Senate",
                District = "",
                Circuit = "",
                Division = "",
                County = "",
                City = "",
                FilerId = ""
            };

            backgroundWorkerScrape.ReportProgress(1, "Begin new search with FormSearch : " + Environment.NewLine + search);

            if (!UpdateCandidates.ReadFirstPage(search))
            {
                backgroundWorkerScrape.ReportProgress(1, UpdateCandidates.CurrentStatus.LastOpMessage);
                return;
            }

            var candidates = UpdateCandidates.Candidates;

            backgroundWorkerScrape.ReportProgress(2, UpdateCandidates.CurrentStatus.LastOpMessage + Environment.NewLine);

            while (UpdateCandidates.CurrentStatus.LastPageCompleted < UpdateCandidates.CurrentStatus.TotalPages)
            {
                var finished = UpdateCandidates.ReadSubsequentPage(search);
                backgroundWorkerScrape.ReportProgress(2, UpdateCandidates.CurrentStatus.LastOpMessage + Environment.NewLine);
            }
        }

        private static SequenceStatus RunAllQueries(int year, BackgroundWorker bgWorker)
        {
            // Does not work on installed version: $"{Utils.GetExecutingDirectory()}\\Data\\OfficeNames-Ids.json";

            var appDataPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\OfficeNames-Ids.json";

            Log.Debug($"appDataPath = {appDataPath}");

            var program = new ScrapeSequence(appDataPath);

            program.RunAllQueries(year, bgWorker);

            return program.SeqStatus;
        }

        #endregion Scrape Testing and development


        // +++++++++++++++++++++++ UI Events +++++++++++++++++++++++++++++++++++

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var list = new List<Candidate>();

            var arrObjects = new object[] {((CtrlListItem)cboYear.SelectedItem).ItemData, list };        // Declare the array of objects

            if (backgroundWorkerScrape.IsBusy)
            {
                AppendLogBox("Cannot start scrape, Background worker is busy.");
                return;
            }

            btnStart.Enabled = false;                           // Disable the Start button
            AppendLogBox("Starting new scrape.");

            backgroundWorkerScrape.RunWorkerAsync(arrObjects);  // Call the background worker, process on a separate thread
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            // Flag the process to be stopped. This will not stop the process. 
            // It will only set the backgroundWorker.CancellationPending property.
            backgroundWorkerScrape.CancelAsync();
            btnStart.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = string.Empty;
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtLog.Text, true);
        }

        private void BtnSaveCsv_Click(object sender, EventArgs e)
        {
            if (_candidateList == null)
            {
                AppendLogBox("Empty candidate list");
                return;
            }
            if (_candidateList.Count == 0)
            {
                AppendLogBox("No candidates in list");
                return;
            }

            var dummy = new Candidate();

            var sb = new StringBuilder();

            sb.AppendLine(dummy.CsvHeader());

            foreach (var candidate in _candidateList)
            {
                sb.AppendLine(candidate.ToCsv());
            }

            var path = $"{tbCsvFilePath.Text}\\{Utils.FilenameWithDateTime("CandidatesEthics", "csv")}";
            FileHelper.StringToFile(sb, path);

            AppendLogBox($"CSV file written to {path}");
        }

        private void BtnSetPath_Click(object sender, EventArgs e)
        {
            var browser = new FolderBrowserDialog();
            string error;
            tbCsvFilePath.Text = FileHelper.GetFolderName(browser, "", "Select CSV save folder", out error);
            if (string.IsNullOrEmpty(tbCsvFilePath.Text))
            {
                tbCsvFilePath.Text = "C:\\Temp";
            }
        }

        private void AppendLogBox(string str)
        {
            txtLog.Text = $"{str}{Environment.NewLine}{txtLog.Text}";
        }

    }
}
