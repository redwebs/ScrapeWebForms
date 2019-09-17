using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ScrapeConsole
{
    public static class FileHelper
    {

        #region XML

        private static string RemoveNamespace(string protocolData)
        {
            if (!protocolData.Contains("xsi")) return protocolData;

            var start = protocolData.IndexOf("xsi", StringComparison.Ordinal);
            var length = protocolData.IndexOf(">", start, StringComparison.Ordinal);
            protocolData = protocolData.Remove(start, length - start);
            return protocolData;
        }

        public static XmlReader GetReaderAtNode(string filePath, string nodeName)
        {
            var reader = new XmlTextReader(filePath);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name.Equals(nodeName))
                    {
                        return reader;
                    }
                }
            }
            return null;
        }

        #endregion

        #region File IO

        public static string ReadFile(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            var sr = File.OpenText(filePath);
            var fileText = sr.ReadToEnd();
            sr.Close();
            return fileText;
        }

        public static void StringToFile(StringBuilder saveData, string filePath)
        {
            if (filePath.Length.Equals(0)) return;

            // create a writer and open the file
            TextWriter tw = new StreamWriter(filePath, false);

            // write a line of text to the file
            tw.WriteLine(saveData.ToString());

            // close the stream
            tw.Close();
        }

        public static void CopyToClipboard(string clipInfo)
        {
            System.Windows.Forms.Clipboard.SetDataObject(clipInfo, true);
        }

        public static string GetFromClipboard()
        {
            var iData = Clipboard.GetDataObject();
            if (iData == null) return string.Empty;

            // Determine whether the data is in a text format.
            if (!iData.GetDataPresent(DataFormats.Text)) return string.Empty;

            return iData.GetData(DataFormats.Text).ToString();
        }

        /// <summary>
        /// Opens ofDialog to get a filename
        /// </summary>
        /// <param name="ofDialog">An open file dialog object.</param>
        /// <param name="initialDir">The directory to start in.</param>
        /// <param name="fileName">The default filename to be displayed.</param>
        /// <param name="defExtension">The Default File extension without the leading period.</param>
        /// <param name="filter">The set of pairs of filters, separated with "|". Each pair consists of a description|file spec.</param>
        /// <param name="title">Dialog title.</param>
        /// <param name="chkExists">If true, insures the file exists.</param>
        /// <param name="error">An output error message string.</param>
        /// <returns>Full path of file.</returns>
        /// 
        public static string GetFileName(OpenFileDialog ofDialog, string initialDir, string fileName,
            string defExtension, string filter, string title, bool chkExists, out string error)
        {
            error = string.Empty;

            try
            {
                // Check to ensure that the selected file exists.  Dialog box displays a warning otherwise.

                ofDialog.InitialDirectory = initialDir;
                ofDialog.FileName = fileName;
                ofDialog.CheckFileExists = chkExists;

                ofDialog.DefaultExt = defExtension;

                // Return the file linked to the LNK file.

                ofDialog.DereferenceLinks = true;
                ofDialog.Filter = filter;
                ofDialog.Multiselect = false;

                // Restore the original directory when done selecting a file? 
                //  False: the current directory changes
                //	True:  put the current folder back to original

                ofDialog.RestoreDirectory = false;

                // Show the Help button and Read-Only checkbox?

                ofDialog.ShowHelp = true;
                ofDialog.ShowReadOnly = false;

                ofDialog.Title = title;

                // Only accept valid Win32 file names?
                ofDialog.ValidateNames = true;

                if (ofDialog.ShowDialog() == DialogResult.OK)
                {
                    return ofDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                error = "Cannot open file: " + ex.Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Opens fbDialog to get a folder name
        /// </summary>
        /// <param name="fbDialog">An ope FolderBrowserDialog</param>
        /// <param name="initialDir">Directory to select in the tree.</param>
        /// <param name="title">Dialog title.</param>
        /// <param name="error">An output error message string.</param>
        /// <returns>Path of folder.</returns>
        /// 
        public static string GetFolderName(FolderBrowserDialog fbDialog, string initialDir, string title, out string error)
        {
            error = string.Empty;

            try
            {
                // Use this to force selection at or below a system type folder
                // FolderBrowserDialogEx.SetRootFolder(fbDialog , FolderBrowserDialogEx.CsIdl.AppData);

                fbDialog.ShowNewFolderButton = true;
                fbDialog.Description = title;
                fbDialog.SelectedPath = initialDir;

                if (fbDialog.ShowDialog() == DialogResult.OK)
                {
                    return fbDialog.SelectedPath;
                }

            }
            catch (Exception ex)
            {
                error = "Error browsing folders: " + ex.Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates temporary file name.
        /// </summary>
        /// <param name="fileExtension">Extension of filename.</param>
        /// <returns>Full temporary file path.</returns>
        /// <remarks></remarks>
        public static string GenerateTempFileName(string fileExtension)
        {
            //Generate temporary file name. 
            var exportFileName = System.IO.Path.GetTempFileName() + fileExtension;
            return exportFileName.Replace(".tmp", "");
        }

        #endregion File IO
    }
}
