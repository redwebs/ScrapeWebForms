using System.Text;

namespace PageScrape
{
    // http://media.ethics.ga.gov/search/Campaign/Campaign_OfficeSearchResults.aspx?ElectionYear=2018&County=&City=&OfficeTypeID=120&District=&Division=&FilerID=&OfficeName=State%20Senate&Circuit=
    public class FormSearch
    {
        public string ElectionYear { get; set; } = string.Empty;
        public string OfficeTypeId { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Circuit { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FilerId { get; set; } = string.Empty;

        public FormSearch()
        {
        }

        public FormSearch(string year, int officeTypeId, string officeName)
        {
            ElectionYear = year;
            OfficeName = officeName;
            OfficeTypeId = officeTypeId.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.AppendLine(ElectionYear);
            sb.Append("OfficeTypeId = ");
            sb.AppendLine(OfficeTypeId);
            sb.Append("OfficeName   = ");
            sb.AppendLine(OfficeName);
            sb.Append("District     = ");
            sb.AppendLine(District);
            sb.Append("Circuit      = ");
            sb.AppendLine(Circuit);
            sb.Append("Division     = ");
            sb.AppendLine(Division);
            sb.Append("County       = ");
            sb.AppendLine(County);
            sb.Append("City         = ");
            sb.AppendLine(City);
            sb.Append("FilerId      = ");
            sb.AppendLine(FilerId);
            sb.AppendLine("--");

            return sb.ToString();
        }

        public string ToSingleLine()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.Append(ElectionYear);
            sb.Append(", OfficeTypeId = ");
            sb.Append(OfficeTypeId);
            sb.Append(", OfficeName = ");
            sb.Append(OfficeName.Replace("%20", " "));
            sb.Append(", District = ");
            sb.Append(District);
            sb.Append(", Circuit = ");
            sb.Append(Circuit);
            sb.Append(", Division = ");
            sb.Append(Division);
            sb.Append(", County = ");
            sb.Append(County);
            sb.Append(", City = ");
            sb.Append(City);
            sb.Append(", FilerId = ");
            sb.AppendLine(FilerId);

            return sb.ToString();
        }
    }
}
