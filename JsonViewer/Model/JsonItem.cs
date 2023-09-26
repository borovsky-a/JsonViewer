using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JsonViewer.Model
{
    public sealed class JsonItem : ObservableObject
    {
        private bool _isExpanded;
        private bool _isSelected;

        public JsonItem()
        {
            Nodes = new ObservableCollection<JsonItem>();
        }

        public JsonItem Parent { get; set; }

        public ICollection<JsonItem> Nodes { get; set; }

        public JsonItemType ItemType { get; set; }

        public string Name { get; set; } = "root";

        public string Value { get; set; }

        public int Index { get; set; }

        public bool IsVisible { get; set; }

        public bool IsMatch {  get; set; }

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
            return Index.ToString();
        }               
    }
}
