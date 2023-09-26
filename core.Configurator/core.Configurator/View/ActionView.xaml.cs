using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mop.Configurator.View
{
    /// <summary>
    /// Interaction logic for ActionView.xaml
    /// </summary>
    public partial class ActionView : UserControl
    {
        public ActionView()
        {
            InitializeComponent();
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var datacontext = DataContext as ActionViewModel;
                datacontext.Filter = ((TextBox)sender).Text;
            }
        }
    }
}
