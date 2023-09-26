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
    /// Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView : UserControl
    {
        public ParametersView()
        {
            InitializeComponent();
            var dc = DataContext;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dc =(ParametersViewModel) DataContext;
            dc.OnPropertyChanged("Parameters");
        }
    }
}
