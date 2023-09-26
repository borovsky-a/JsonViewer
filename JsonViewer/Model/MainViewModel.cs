using CommunityToolkit.Mvvm.ComponentModel;

namespace JsonViewer.Model
{
    public sealed class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
             JsonViewer = new JsonItemsViewModel();
        }

        public JsonItemsViewModel JsonViewer { get; }
    }
}
