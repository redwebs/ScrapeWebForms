using System;
using System.Text;

namespace PageScrape
{
    public class Candidate
    {
        private const string CampaignByOfficeUrl = "http://media.ethics.ga.gov/search/Campaign/Campaign_ByOffice.aspx";

        private const string BaseUrl = "http://media.ethics.ga.gov/search/Campaign/Campaign_Name.aspx?NameID=";

        private const string BaseUrlFilerId = "&FilerID=";

        private const string BaseUrlEnd = "&Type=candidate";

        public string NameId { get; set; }

        public string FilerId { get; set; }

        public string CandidateName { get; set; }

        public int Year { get; set; }

        public DateTime DoiFiled { get; set; }

        public string OfficeTypeId { get; set; } = string.Empty;

        public string OfficeName { get; set; } = string.Empty;

        public string OfficeArea { get; set; } = string.Empty;

        public string County { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Affiliation { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string InfoUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(NameId) && !string.IsNullOrEmpty(FilerId))
                {
                    return $"{BaseUrl}{NameId}{BaseUrlFilerId}{FilerId}{BaseUrlEnd}";
                }

                return CampaignByOfficeUrl;
            }

        }

        public string Notes { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(" Name: ");
            sb.Append(CandidateName);
            sb.Append(", DOI Filed ");
            sb.Append(DoiFiled == DateTime.MinValue ? "none" : $"{DoiFiled}");
            sb.Append(", Year ");
            sb.Append(Year);
            sb.Append(", NameId ");
            sb.Append(NameId);
            sb.Append(", FilerId ");
            sb.Append(FilerId);
            sb.Append(", OfficeName ");
            sb.Append(OfficeName);
            sb.Append(", DistPostCkt ");
            sb.Append(OfficeArea);
            sb.Append(", County ");
            sb.Append(County);
            sb.Append(", City ");
            sb.Append(City);
            sb.Append(", OfficeTypeId ");
            sb.Append(OfficeTypeId);
            sb.Append(", Affiliation ");
            sb.Append(Affiliation);
            sb.Append(", Status ");
            sb.Append(Status);
            sb.Append(", Notes ");
            sb.Append(Notes);
            sb.Append(", InfoUrl ");
            sb.Append(InfoUrl);

            return sb.ToString();
        }

        public string CsvHeader()
        {
            var sb = new StringBuilder();
            sb.Append("\"Name\",");
            sb.Append("\"DOI Filed\",");
            sb.Append("\"Year\",");
            sb.Append("\"NameId\",");
            sb.Append("\"FilerId\",");
            sb.Append("\"OfficeName\",");
            sb.Append("\"DistPostCkt\",");
            sb.Append("\"County\",");
            sb.Append("\"City\",");
            sb.Append("\"OfficeTypeId\",");
            sb.Append("\"Affiliation\",");
            sb.Append("\"Status\",");
            sb.Append("\"Notes\",");
            sb.Append("\"InfoUrl\"");
            return sb.ToString();
        }

        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.Append($"\"{CandidateName}\",");
            sb.Append(DoiFiled == DateTime.MinValue ? "\"\"," : $"\"{DoiFiled}\",");
            sb.Append($"\"{Year}\",");
            sb.Append($"\"{NameId}\",");
            sb.Append($"\"{FilerId}\",");
            sb.Append($"\"{OfficeName}\",");
            sb.Append($"\"{OfficeArea}\",");
            sb.Append($"\"{County}\",");
            sb.Append($"\"{City}\",");
            sb.Append($"\"{OfficeTypeId}\",");
            sb.Append($"\"{Affiliation}\",");
            sb.Append($"\"{Status}\",");
            sb.Append($"\"{Notes}\",");
            sb.Append($"\"{InfoUrl}\"");
            return sb.ToString();
        }
    }
}
