using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace JsonViewer.Model
{
    public sealed partial class JsonItem : ObservableObject
    {

        public JsonItem()
        {
            Nodes = new List<JsonItem>();
        }

        [ObservableProperty]
        private bool _isMatch;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isVisible = true;

        public JsonItem? Parent { get; set; }

        public IList<JsonItem> Nodes { get; set; }

        public JsonItemType ItemType { get; set; }

        public string Name { get; set; } = "root";

        public string? Value { get; set; }

        public int Index { get; set; }          

        public override string ToString()
        {
            return Name.ToString();
        }               
    }
}
