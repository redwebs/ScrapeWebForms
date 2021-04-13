
using System;

namespace ScrapeConsole
{
    public class CtrlListItem
    {
        protected string Text;
        protected int ItemDataVal;

        public CtrlListItem(string text, int itemData)
        {
            Text = text;
            ItemDataVal = itemData;
        }

        public CtrlListItem(string text)
        {
            Text = text;
            if (!int.TryParse(text, out ItemDataVal))
            {
                throw new ArgumentException($"{text} cannot be parsed to an integer.");
            };
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
