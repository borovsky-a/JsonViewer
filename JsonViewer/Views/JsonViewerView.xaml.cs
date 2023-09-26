using JsonViewer.Model;
using System.Windows.Controls;

namespace JsonViewer.Views
{
    /// <summary>
    /// Interaction logic for JsonViewerView.xaml
    /// </summary>
    public partial class JsonViewerView : UserControl
    {
        public JsonViewerView()
        {
            DataContextChanged += JsonViewerView_DataContextChanged;
            InitializeComponent();
        }
        private void JsonViewerView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            ((JsonItemsViewModel)e.NewValue).View = tree;
        }     
    }
}
