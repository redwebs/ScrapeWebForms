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

        private const string ClientSecretPath = "C:\\Users\\fred\\Dropbox\\GoogleClientSecret1.json";
        private const string ClientSecretPathSheets = "C:\\Users\\fred\\Dropbox\\GoogSh4088cred.json";

        private List<Candidate> _candidateList;

        #region Form Init

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
        }

        #endregion Form Init

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

            RunAllQueries(year, sendingWorker);

            var output = new ScrapeResult
            {
                Candidates = UpdateCandidates.Candidates,
                ElapsedTime = timer.Elapsed.ToString()
            };

            e.Result = output;
            
        }

        private void BackgroundWorkerScrape_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update UI
            var status = (ScrapeUserStatus) e.UserState;
            progressBar1.Maximum = status.OfficeCount;
            progressBar1.Increment(status.OfficesSearched);
            tbStatus.Text = $"Candidate: {status.Candidate}"; 
            txtLog.Text +=  $"Candidate: {status.Candidate}, Elapsed Time {status.ElapsedTime}";

            if (!string.IsNullOrEmpty(status.Message))
            {
                txtLog.Text += status.Message;
            }

            if (status.Cancelled)
            {
                txtLog.Text += "User cancelled the scrape.";
            }
        }

        private void BackgroundWorkerScrape_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // An error occurred
                tbStatus.Text += Environment.NewLine + " An error occurred: " + e.Error.Message;
            }
            else if (e.Cancelled)
            {
                // The process was cancelled
                tbStatus.Text += Environment.NewLine + " Job cancelled.";
            }
            else
            {
                // The process finished
                var result = (ScrapeResult) e.Result;
                _candidateList = (List<Candidate>)result.Candidates;
                tbStatus.Text = $" Job finished, {_candidateList.Count} Candidates, Elapsed Time: {result.ElapsedTime}";
            }

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

        private static void RunAllQueries(int year, BackgroundWorker bgWorker)
        {
            var path = $"{Utils.GetExecutingDirectory()}\\Data\\OfficeNames-Ids.json";

            var program = new ScrapeSequence(path);

            var cnt = program.RunAllQueries(year, bgWorker);
        }

        #endregion Scrape Testing and development


        // +++++++++++++++++++++++ UI Events +++++++++++++++++++++++++++++++++++

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var list = new List<Candidate>();

            var arrObjects = new object[] {2019, list };        // Declare the array of objects

            if (backgroundWorkerScrape.IsBusy) return;

            btnStart.Enabled = false;                           // Disable the Start button
            txtLog.Text = "Starting new scrape.";

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
                txtLog.Text += "Empty candidate list";
                return;
            }
            if (_candidateList.Count == 0)
            {
                txtLog.Text += "No candidates in list";
                return;
            }

            var dummy = new Candidate();

            var sb = new StringBuilder();

            sb.AppendLine(dummy.CsvHeader());

            foreach (var candidate in _candidateList)
            {
                sb.AppendLine(candidate.ToCsv());
            }

            var path = $"C:{tbCsvFilePath.Text}\\{Utils.FilenameWithDateTime("Candidates", "csv")}";
            FileHelper.StringToFile(sb, path);

            txtLog.Text += $"CSV file written to {path}";
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
    }

    public class ScrapeResult
    {
        public List<Candidate> Candidates { get; set; }
        public string ElapsedTime { get; set; }
    }
}
