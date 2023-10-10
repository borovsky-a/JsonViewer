
namespace JsonViewer.Model
{
    public sealed class MainViewModel
    {
        public MainViewModel()
        {
             JsonViewer = new JsonItemsViewModel();
        }

        public JsonItemsViewModel JsonViewer { get; }
    }
}
