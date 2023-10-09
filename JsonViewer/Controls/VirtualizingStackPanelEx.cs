using System.Windows.Controls;

namespace JsonViewer.Controls
{
    public sealed class VirtualizingStackPanelEx :    
        VirtualizingStackPanel
    {
        public void BringIntoView(int index)
        {
            BringIndexIntoView(index);
        }
    }
}
