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

namespace mop.Configurator.Editors
{
    /// <summary>
    /// Interaction logic for PasswordEditorView.xaml
    /// </summary>
    public partial class PasswordEditorView : UserControl
    {
        public PasswordEditorView()
        {
            InitializeComponent();
           
        }

     
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);           
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = DataContext as PasswordEditor;
            if(model != null)
            {
                passwordField.Password = model.Value;            
            }                
        }
        private void passwordField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var model = DataContext as PasswordEditor;
            if (model != null)
            {
                model.Value = passwordField.Password;
                var validationResult = model["Value"];
                passwordFieldError.Text = validationResult;
            }              
        }
    }
}
