namespace ScrapeConsole
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCopyLog = new System.Windows.Forms.Button();
            this.tbSiteUrl = new System.Windows.Forms.TextBox();
            this.tbCsvFilePath = new System.Windows.Forms.TextBox();
            this.backgroundWorkerScrape = new System.ComponentModel.BackgroundWorker();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnSaveCsv = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblSiteUrl = new System.Windows.Forms.Label();
            this.lblCsvFilePath = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSiteTitle = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnSetPath = new System.Windows.Forms.Button();
            this.cboYear = new System.Windows.Forms.ComboBox();
            this.lblYearToScrape = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 153);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(614, 181);
            this.txtLog.TabIndex = 1;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(646, 153);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(58, 27);
            this.btnClear.TabIndex = 31;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCopyLog
            // 
            this.btnCopyLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyLog.Location = new System.Drawing.Point(646, 191);
            this.btnCopyLog.Name = "btnCopyLog";
            this.btnCopyLog.Size = new System.Drawing.Size(60, 27);
            this.btnCopyLog.TabIndex = 32;
            this.btnCopyLog.Text = "Copy";
            this.btnCopyLog.UseVisualStyleBackColor = true;
            this.btnCopyLog.Click += new System.EventHandler(this.btnCopyLog_Click);
            // 
            // tbSiteUrl
            // 
            this.tbSiteUrl.Location = new System.Drawing.Point(99, 35);
            this.tbSiteUrl.MaxLength = 150;
            this.tbSiteUrl.Name = "tbSiteUrl";
            this.tbSiteUrl.Size = new System.Drawing.Size(478, 20);
            this.tbSiteUrl.TabIndex = 134;
            this.tbSiteUrl.Text = "http://media.ethics.ga.gov/search/Campaign/Campaign_ByOffice.aspx";
            // 
            // tbCsvFilePath
            // 
            this.tbCsvFilePath.Location = new System.Drawing.Point(99, 62);
            this.tbCsvFilePath.MaxLength = 150;
            this.tbCsvFilePath.Name = "tbCsvFilePath";
            this.tbCsvFilePath.Size = new System.Drawing.Size(478, 20);
            this.tbCsvFilePath.TabIndex = 136;
            this.tbCsvFilePath.Text = "C:\\Temp";
            // 
            // backgroundWorkerScrape
            // 
            this.backgroundWorkerScrape.WorkerReportsProgress = true;
            this.backgroundWorkerScrape.WorkerSupportsCancellation = true;
            this.backgroundWorkerScrape.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerScrape_DoWork);
            this.backgroundWorkerScrape.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorkerScrape_ProgressChanged);
            this.backgroundWorkerScrape.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerScrape_RunWorkerCompleted);
            // 
            // tbStatus
            // 
            this.tbStatus.AllowDrop = true;
            this.tbStatus.Location = new System.Drawing.Point(97, 92);
            this.tbStatus.MaxLength = 100;
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.Size = new System.Drawing.Size(480, 20);
            this.tbStatus.TabIndex = 137;
            this.tbStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(584, 101);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(61, 27);
            this.btnStart.TabIndex = 138;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(649, 101);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(68, 27);
            this.btnStop.TabIndex = 139;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // btnSaveCsv
            // 
            this.btnSaveCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveCsv.Location = new System.Drawing.Point(648, 61);
            this.btnSaveCsv.Name = "btnSaveCsv";
            this.btnSaveCsv.Size = new System.Drawing.Size(68, 27);
            this.btnSaveCsv.TabIndex = 140;
            this.btnSaveCsv.Text = "Save CSV";
            this.btnSaveCsv.UseVisualStyleBackColor = true;
            this.btnSaveCsv.Click += new System.EventHandler(this.BtnSaveCsv_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(97, 112);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(480, 23);
            this.progressBar1.TabIndex = 141;
            // 
            // lblSiteUrl
            // 
            this.lblSiteUrl.AutoSize = true;
            this.lblSiteUrl.Location = new System.Drawing.Point(13, 38);
            this.lblSiteUrl.Name = "lblSiteUrl";
            this.lblSiteUrl.Size = new System.Drawing.Size(50, 13);
            this.lblSiteUrl.TabIndex = 142;
            this.lblSiteUrl.Text = "Site URL";
            // 
            // lblCsvFilePath
            // 
            this.lblCsvFilePath.AutoSize = true;
            this.lblCsvFilePath.Location = new System.Drawing.Point(14, 65);
            this.lblCsvFilePath.Name = "lblCsvFilePath";
            this.lblCsvFilePath.Size = new System.Drawing.Size(72, 13);
            this.lblCsvFilePath.TabIndex = 143;
            this.lblCsvFilePath.Text = "CSV File Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 145;
            this.label2.Text = "Site Title";
            // 
            // tbSiteTitle
            // 
            this.tbSiteTitle.Location = new System.Drawing.Point(99, 8);
            this.tbSiteTitle.MaxLength = 150;
            this.tbSiteTitle.Name = "tbSiteTitle";
            this.tbSiteTitle.Size = new System.Drawing.Size(478, 20);
            this.tbSiteTitle.TabIndex = 144;
            this.tbSiteTitle.Text = "GA Gov Transparency and Campaign Finance Commission Web Scrape";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 92);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(74, 13);
            this.lblStatus.TabIndex = 146;
            this.lblStatus.Text = "Scrape Status";
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(12, 116);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(85, 13);
            this.lblProgress.TabIndex = 147;
            this.lblProgress.Text = "Scrape Progress";
            // 
            // btnSetPath
            // 
            this.btnSetPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetPath.Location = new System.Drawing.Point(583, 61);
            this.btnSetPath.Name = "btnSetPath";
            this.btnSetPath.Size = new System.Drawing.Size(61, 27);
            this.btnSetPath.TabIndex = 148;
            this.btnSetPath.Text = "Set Path";
            this.btnSetPath.UseVisualStyleBackColor = true;
            this.btnSetPath.Click += new System.EventHandler(this.BtnSetPath_Click);
            // 
            // cboYear
            // 
            this.cboYear.FormattingEnabled = true;
            this.cboYear.Location = new System.Drawing.Point(611, 30);
            this.cboYear.Name = "cboYear";
            this.cboYear.Size = new System.Drawing.Size(68, 21);
            this.cboYear.TabIndex = 149;
            // 
            // lblYearToScrape
            // 
            this.lblYearToScrape.AutoSize = true;
            this.lblYearToScrape.Location = new System.Drawing.Point(606, 9);
            this.lblYearToScrape.Name = "lblYearToScrape";
            this.lblYearToScrape.Size = new System.Drawing.Size(78, 13);
            this.lblYearToScrape.TabIndex = 150;
            this.lblYearToScrape.Text = "Year to Scrape";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(725, 343);
            this.Controls.Add(this.lblYearToScrape);
            this.Controls.Add(this.cboYear);
            this.Controls.Add(this.btnSetPath);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbSiteTitle);
            this.Controls.Add(this.lblCsvFilePath);
            this.Controls.Add(this.lblSiteUrl);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnSaveCsv);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.tbCsvFilePath);
            this.Controls.Add(this.tbSiteUrl);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCopyLog);
            this.Controls.Add(this.txtLog);
            this.Name = "FormMain";
            this.Text = "Scrape Console";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCopyLog;
        private System.Windows.Forms.TextBox tbSiteUrl;
        private System.Windows.Forms.TextBox tbCsvFilePath;
        private System.ComponentModel.BackgroundWorker backgroundWorkerScrape;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnSaveCsv;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblSiteUrl;
        private System.Windows.Forms.Label lblCsvFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSiteTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button btnSetPath;
        private System.Windows.Forms.ComboBox cboYear;
        private System.Windows.Forms.Label lblYearToScrape;
    }
}

