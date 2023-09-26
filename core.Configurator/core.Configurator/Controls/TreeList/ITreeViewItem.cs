using System.Collections.ObjectModel;

namespace mop.Configurator.Controls
{
    public interface ITreeViewItem<TTreeViewItem> : ITreeViewItem where TTreeViewItem : ITreeViewItem<TTreeViewItem>
    {
        TTreeViewItem Parent { get; set; }
        bool HasItems { get; }
        ObservableCollection<TTreeViewItem> Parameters { get; set; }
        ObservableCollection<TTreeViewItem> Attributes { get; set; }
    }
    public interface ITreeViewItem
    {
        string Name { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string Value { get; set; }
        string XPath { get; set; }
        bool? IsChecked { get; set; }
        bool IsVisible { get; set; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        bool IsEditable { get; set; }
        bool IsCData { get; set; }
        bool IsRequired { get; set; }
    }
}
