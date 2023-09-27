using System.Windows.Controls;

namespace JsonViewer.Controls
{
    public class VirtualizingStackPanelEx :    VirtualizingStackPanel
    {
        public void BringIntoView(int index)
        {
            BringIndexIntoView(index);
        }
    }
}
