
namespace ScrapeConsole
{
    public class CtrlListItem
    {
        protected string Text;
        protected int ItemDataVal;

        public CtrlListItem(string text)
        {
            Text = text;
            ItemDataVal = 0;
        }

        public CtrlListItem(string text, int itemData)
        {
            Text = text;
            ItemDataVal = itemData;
        }

        public CtrlListItem(string text, string itemDataString)
        {
            Text = text;
            int.TryParse(itemDataString, out ItemDataVal);
        }

        public int ItemData
        {
            get => ItemDataVal;
            set => ItemDataVal = value;
        }

        public string ItemDataString
        {
            get => ItemData.ToString();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
