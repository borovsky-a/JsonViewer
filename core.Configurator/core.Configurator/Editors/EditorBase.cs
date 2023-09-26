using System.ComponentModel;

namespace mop.Configurator
{
    public abstract class EditorBase : BaseViewModel, IDataErrorInfo
    {
        private bool _isVisible;
        private string _error;
        public virtual void Initialize()
        {
            Value = Parameter.Value;            
        }

        public virtual void Save()
        {
            if (IsValid)
                Parameter.Value = Value;
        }

        public virtual bool IsDefault { get; set; }
        public abstract string[] SuitableNames { get; set; }
        public abstract int Sequence { get; set; }
        public abstract string Header { get; set; }
        public Parameter Parameter
        {
            get { return Source?.Parameter; }
        }
        internal EditorManager Source { get; set; }
        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public virtual string Value
        {
            get { return Source.Value; }
            set
            {
                if (Source.Value != value)
                {
                    Source.Value = value;
                }
            }
        }       

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public virtual bool IsValid
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        public virtual string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                if (Parameter.IsRequired && string.IsNullOrEmpty(Value))
                    error = "Значение должно быть заполнено";
                return Error = error;
            }
        }

        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }       
    }
}
