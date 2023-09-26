using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JsonViewer.Controls
{
    public class VirtualizingStackPanelEx :    VirtualizingStackPanel
    {
        public void BringIntoView(int index)
        {

            this.BringIndexIntoView(index);
        }
    }
}
