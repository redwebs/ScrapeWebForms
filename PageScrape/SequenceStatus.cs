using System.Text;

namespace PageScrape
{
    public class SequenceStatus
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SequenceStatus()
        {
            SbLog = new StringBuilder();
        }

        public FormSearch TheFormSearch { get; set; }

        public bool SequenceComplete { get; set; } = false;

        public bool SequenceFail { get; set; } = false;

        public int TotalSequences { get; set; }

        public int LastSequenceIdCompleted { get; set; }

        private string _message = string.Empty;

        public string LastOpMessage
        {
            get => _message;

            set

            {
                _message = value;
                if (InternalLoggingOn)
                {
                    SbLog.AppendLine(_message);
                }
                if (LoggingOn)
                {
                    Log.Debug(_message);
                }
            }
        }

        public StringBuilder SbLog { get; set; }

        public bool InternalLoggingOn { get; set; } = true;

        public bool LoggingOn { get; set; } = true;

        public override string ToString()
        {
            return SbLog.ToString();
        }
    }
}