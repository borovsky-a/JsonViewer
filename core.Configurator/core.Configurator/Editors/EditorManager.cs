using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace mop.Configurator
{
    public class EditorManager : ObservableCollection<EditorBase>
    {
        private string _value;
        private Parameter _parameter;
        public EditorManager()
            :base()
        {
        }   

        public EditorManager(IEnumerable<EditorBase> collection) 
            : base(collection)
        {
        }

        public bool ShowAll { get; set; } = true;
        public Parameter Parameter 
        {
            get { return _parameter; }
            set
            {
                if(_parameter != value)
                {
                    _parameter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Parameter)));
                }
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if(_value != value)
                {
                    _value = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }            

        public new void Add(EditorBase item)
        {
            item.Source = this;
            base.Add(item);
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Value):
                    foreach (var item in this)
                    {
                        item.OnPropertyChanged(nameof(item.Value));
                    }                   
                    break;
                default:
                   break;
            }            
        }
    }
}
