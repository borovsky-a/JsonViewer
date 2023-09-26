using System.Windows;
using System.Windows.Controls;

namespace mop.Configurator.Controls
{
    public class TreeListView : TreeView
    {

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            "Columns",
            typeof(GridViewColumnCollection),
            typeof(TreeListView),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty AllowsColumnReorderProperty = DependencyProperty.Register(
            "AllowsColumnReorder",
            typeof(bool),
            typeof(TreeListView),
            new UIPropertyMetadata(null));

        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        public override void BeginInit()
        {
            base.BeginInit();
        }

        public TreeListView()
        {
            Columns = new GridViewColumnCollection();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public bool AllowsColumnReorder
        {
            get { return (bool)GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }
    }
}
