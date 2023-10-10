using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JsonViewer.Model
{
    public sealed class JsonItem : ObservableObject
    {
        private bool _isMatch;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isVisible = true;

        public JsonItem()
        {
            Nodes = new List<JsonItem>();
        }
        public JsonItem Parent { get; set; }

        public IList<JsonItem> Nodes { get; set; }

        public JsonItemType ItemType { get; set; }

        public string Name { get; set; } = "root";

        public string Value { get; set; }

        public int Index { get; set; }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsMatch
        {
            get => _isMatch;
            set => SetProperty(ref _isMatch, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public override string ToString()
        {
            return Name.ToString();
        }               
    }
}
