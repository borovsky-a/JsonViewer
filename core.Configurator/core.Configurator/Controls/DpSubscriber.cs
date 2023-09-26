using System;
using System.Windows;
using System.Windows.Data;

namespace mop.Configurator.Controls
{
    public class DpSubscriber : DependencyObject, IDisposable
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(DpSubscriber),
            new PropertyMetadata(null, Notify));

        private Binding _binding;

        public DpSubscriber(DependencyObject source, DependencyProperty property)
        {
            _binding = new Binding
            {
                Source = source,
                Path = new PropertyPath(property),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(this, ValueProperty, _binding);
        }

        public event DependencyPropertyChangedEventHandler ValueChanged;
        protected virtual void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(_binding.Source, e);
        }
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static void Notify(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DpSubscriber)d).OnValueChanged(e);
        }
        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
            _binding = null;
        }
    }
}
