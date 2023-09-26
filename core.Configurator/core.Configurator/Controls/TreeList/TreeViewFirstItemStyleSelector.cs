using System.Windows;
using System.Windows.Controls;
namespace mop.Configurator.Controls
{
    public class TreeViewFirstItemStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var style = base.SelectStyle(item, container);
            //if (style == null)
            //    style = new Style();
            //style.Setters.Add(new Setter(ContentControl.VisibilityProperty, new Binding("IsVisibleValue")));
            return style;
        }
    }
}
