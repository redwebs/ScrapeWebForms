using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScrapeConsole
{
    public static class Utils
    {
        public static string GetInnerExceptions(Exception ex)
        {
            var statusMsg = string.Empty;

            if (!ex.Message.Contains("See the inner exception for details"))
            {
                statusMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    statusMsg += ex.InnerException.Message;
                }
            }
            else
            {
                if (ex.InnerException != null)
                {
                    if (!ex.InnerException.Message.Contains("See the inner exception for details"))
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            statusMsg = ex.InnerException.InnerException.Message;
                        }
                    }
                    else
                    {
                        if (ex.InnerException.InnerException != null)
                        {
                            statusMsg += ex.InnerException.InnerException.Message;
                        }
                    }
                }
            }

            return statusMsg;
        }

        public static Tuple<bool, int> StringToInt(string intString)
        {
            var theInt = int.MinValue;

            if (string.IsNullOrEmpty(intString))
            {
                return new Tuple<bool, int>(false, theInt);
            }

            if (!int.TryParse(intString.Trim(), out theInt))
            {
                return new Tuple<bool, int>(false, theInt);
            }

            return new Tuple<bool, int>(true, theInt);
        }

        public static Tuple<bool, decimal> StringToDecimal(string decString)
        {
            var theDecimal = decimal.MinValue;

            if (string.IsNullOrEmpty(decString))
            {
                return new Tuple<bool, decimal>(false, theDecimal);
            }

            if (!decimal.TryParse(decString.Trim(), out theDecimal))
            {
                return new Tuple<bool, decimal>(false, theDecimal);
            }

            return new Tuple<bool, decimal>(true, theDecimal);
        }

        public static Tuple<bool, double> StringToDouble(string dblString)
        {
            var theDouble = double.MinValue;

            if (string.IsNullOrEmpty(dblString))
            {
                return new Tuple<bool, double>(false, theDouble);
            }

            if (!double.TryParse(dblString.Trim(), out theDouble))
            {
                return new Tuple<bool, double>(false, theDouble);
            }

            return new Tuple<bool, double>(true, theDouble);
        }

        public static string FilenameWithDateTime(string prefix, string extension)
        {
            const string format = "yyMMdd_HHmmss";
            var dt = DateTime.Now;
            return $"{prefix}{dt.ToString(format)}.{extension}";
        }

        public static string FilenameWithDate(string prefix, string extension)
        {
            const string format = "yyyy-MM-dd";
            var dt = DateTime.Now;
            return $"{prefix}{dt.ToString(format)}.{extension}"; 
        }

        public static string SplitAndPipeDelimit(string textToSplit)
        {
            var array = textToSplit.Split('\n');
            var pipeString = array.Aggregate(string.Empty, (current, t) => current + $"{t}|");
            if (pipeString.Length > 1)
            {
                return pipeString.Remove(pipeString.Length - 1);
            }

            return pipeString;
        }

        public static string GetExecutingDirectory()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return "Unknown Assembly";

            var location = new Uri(assembly.GetName().CodeBase);
            var info = new FileInfo(location.AbsolutePath).Directory;
            return info != null ? info.FullName : "Unknown Directory";
        }

        private static string ExceptionInfo(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Exception: {ex.Message}");

            var exIterate = ex.InnerException;
            var level = 1;

            while (exIterate != null)
            {
                sb.AppendLine($"Inner{level++}: {exIterate.Message}");
                exIterate = exIterate.InnerException;
            }

            return sb.ToString();
        }
    }
}
