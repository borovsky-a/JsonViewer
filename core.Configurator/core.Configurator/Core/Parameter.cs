using mop.Configurator.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace mop.Configurator
{
    public class Parameter : BaseViewModel, ITreeViewItem<Parameter>, INotifyPropertyChanging
    {
        private bool _isVisible = true;
        private bool _isEditable = true;
        private string _value;
        private bool? _isChecked = false;
        private string _name;
        private bool _isSelected;
        private bool _isExpanded;
        Parameter _parent;
        private bool _isNameMatch;
        private bool _isValueMatch;
        private bool _isDescriptionMatch;

       

        public Parameter(ConfigurationProvider configurationProvider)
        {
            Attributes = new ObservableCollection<Parameter>();
            Parameters = new ObservableCollection<Parameter>();
            Parameters.CollectionChanged += CurrentParameters_CollectionChanged;
            ConfigurationProvider = configurationProvider;
        }

        public virtual string OldValue { get; private set; }   
        public Parameter Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        public ObservableCollection<Parameter> Attributes { get; set; }
        public ObservableCollection<Parameter> Parameters { get; set; }

        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public virtual string DisplayName
        {
            get
            {
                var attribute = GetDisplayNameAttribute();
                return attribute == null ? Name : attribute.Value;
            }
            set
            {
                var attribute = GetDisplayNameAttribute();
                if (attribute == null)
                    Attributes.Add(attribute = new Parameter(ConfigurationProvider) { Name = "Name" });
                attribute.Value = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public virtual bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public virtual bool IsVisible
        {
            get { return _isVisible && GetIsVisible(); }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public virtual bool IsEditable
        {
            get { return _isEditable && GetCanEdit(); }
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public virtual bool IsRequired
        {
            get { return GetRequiredValue(); }
            set { SetRequiredValue(value); }
        }

        public virtual bool HasItems
        {
            get { return Parameters.Count > 0; }
        }

        public virtual string Description
        {
            get { return GetDescriptionValue(); }
            set { SetDescriptionValue(value); }
        }

        public virtual string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    OnPropertyChanging(nameof(Value));
                    OldValue = _value;
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }
        public virtual string DisplayValue
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                {
                    return Value;
                }
                return Value.Split(Environment.NewLine.ToArray()).FirstOrDefault();
            }            
        }
        public bool IsCData { get; set; }

        public string XPath { get; set; }

        public virtual string EditorType
        {
            get
            {
                var viewType = Attributes.FirstOrDefault(o => o.Name.Equals("EditorType", StringComparison.InvariantCultureIgnoreCase));
                if (viewType == null && Parent != null)
                {
                    viewType = Parent.Attributes.FirstOrDefault(o => o.Name.Equals("EditorType", StringComparison.InvariantCultureIgnoreCase));
                }
                if (viewType != null && !string.IsNullOrEmpty(viewType.Value))
                    return viewType.Value;
                if (Name == "StringValue" || Name == "DefaultStringValue")
                    return "string";
                if (Name == "IntValue" || Name == "DefaultIntValue")
                    return "number";
                return viewType?.Name ?? "string";
            }
        }

        public virtual string ValueType
        {
            get
            {
                var valueType = Attributes.FirstOrDefault(o => o.Name.Equals("ValueTypeName", StringComparison.InvariantCultureIgnoreCase));
                if (valueType == null && Parent != null)
                {
                    valueType = Parent.Attributes.FirstOrDefault(o => o.Name.Equals("ValueTypeName", StringComparison.InvariantCultureIgnoreCase));
                }
                if (valueType == null || string.IsNullOrEmpty(valueType.Value))
                    return "Нет данных";
                return valueType.Value;
            }
        }

        public Visibility IsVisibleValue => IsVisible == true ? Visibility.Visible : Visibility.Collapsed;

        public bool IsNameMatch
        {
            get { return _isNameMatch; }
            set
            {
                if (_isNameMatch != value)
                {
                    _isNameMatch = value;
                    OnPropertyChanged(nameof(IsNameMatch));
                }
            }
        }

        public bool IsValueMatch
        {
            get { return _isValueMatch; }
            set
            {
                if (_isValueMatch != value)
                {
                    _isValueMatch = value;
                    OnPropertyChanged(nameof(IsValueMatch));
                }
            }
        }

        public bool IsDescriptionMatch
        {
            get { return _isDescriptionMatch; }
            set
            {
                if (_isDescriptionMatch != value)
                {
                    _isDescriptionMatch = value;
                    OnPropertyChanged(nameof(IsDescriptionMatch));
                }
            }
        }

        public ConfigurationProvider ConfigurationProvider { get; }

        public void Save()
        {
            var doc = ConfigurationProvider.XDocument;
            var elem = doc.XPathSelectElement(XPath);
            if (elem != null && Value != null)
            {
                if (IsCData == true)
                {
                    elem.ReplaceNodes(new XCData(Value));
                }
                else
                    elem.Value = Value;
            }
        }

        public void ToggleVisible()
        {
            if (_isVisible == true)
                HideDependentParameters();
            else ShowDependentParameters();
        }

        public virtual void HideDependentParameters()
        {
            IsVisible = false;
        }

        public virtual void ShowDependentParameters()
        {
            IsVisible = true;
            if (Parent != null && Parent.IsVisible == false)
                Parent.ShowDependentParameters();
        }

        protected virtual bool GetIsVisible()
        {
            var isVisibleAttribute = Attributes.FirstOrDefault(o => o.Name.Equals("IsVisible", StringComparison.InvariantCultureIgnoreCase));
            if (isVisibleAttribute != null && isVisibleAttribute.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (ConfigurationProvider.HiddenElements.Any(o => o == Name))
                return false;

            var endWithElements = ConfigurationProvider.HiddenElements.Where(o => o.StartsWith("*")).Select(o => o.TrimStart('*'));
            if (endWithElements.Any(o => o.Contains(Name) || Name.EndsWith(o)))
                return false;

            var startWithElems = ConfigurationProvider.HiddenElements.Where(o => o.EndsWith("*")).Select(o => o.TrimEnd('*'));
            if (startWithElems.Any(o => Name.StartsWith(o)))
                return false;
            
            return true;
        }

        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Parent))
            {
                var parent = Parent;
                if (parent != null)
                    parent.Parameters.CollectionChanged += Parameters_CollectionChanged;
            }
            else if (propertyName == nameof(IsVisible))
            {
                OnPropertyChanged(nameof(IsVisibleValue));
            }
            else if (propertyName == nameof(IsExpanded))
            {
                if (ConfigurationProvider.IgnoreFilterFlag == true && IsExpanded == true)
                {
                    foreach (var item in Parameters)
                    {
                        item.IsVisible = true;
                    }
                }
            }
            else if (propertyName == nameof(IsChecked))
            {
                if (ConfigurationProvider.UseCheckBoxes)
                {
                    var value = IsChecked;
                    if (value.HasValue)
                    {
                        foreach (var item in Parameters)
                        {
                            item.IsChecked = value;
                        }
                    }
                    if (Parent != null)
                    {
                        if (Parent.Parameters.All(o => o.IsChecked == true))
                            Parent.IsChecked = true;
                        else if (Parent.Parameters.All(o => o.IsChecked == false))
                            Parent.IsChecked = false;
                        else Parent.IsChecked = null;
                    }
                }
            }
        }

        protected virtual bool GetCanEdit()
        {
            if (Parameters.Count > 0)
                return false;

            var isReadOnlyAttribute = Attributes.FirstOrDefault(o => o.Name.Equals("IsReadOnly", StringComparison.InvariantCultureIgnoreCase));
            if (isReadOnlyAttribute != null && isReadOnlyAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        public override string ToString()
        {
            return Name;
        }

       

        private string GetDescriptionValue()
        {
            var attribute = Attributes.FirstOrDefault(o => o.Name.Equals("Description", StringComparison.InvariantCultureIgnoreCase));
            if (attribute == null && Parent != null)
                attribute = Parent.Attributes.FirstOrDefault(o => o.Name.Equals("Description", StringComparison.InvariantCultureIgnoreCase));
            return attribute?.Value;
        }

        private void SetDescriptionValue(string value)
        {
            var attribute = Attributes.FirstOrDefault(o => o.Name.Equals("Description", StringComparison.InvariantCultureIgnoreCase));
            if (attribute == null)
                Parameters.Add(attribute = new Parameter(ConfigurationProvider) { Name = "Description" });
            attribute.Value = value;
            OnPropertyChanged(nameof(Description));
        }

        private bool GetRequiredValue()
        {
            var attribute = Attributes.FirstOrDefault(o => o.Name.Equals("IsRequired", StringComparison.InvariantCultureIgnoreCase));
            if (attribute == null && Parent != null)
                attribute = Parent.Attributes.FirstOrDefault(o => o.Name.Equals("IsRequired", StringComparison.InvariantCultureIgnoreCase));
            if (attribute == null)
                return false;
            if (bool.TryParse(attribute.Value, out bool result))
                return result;
            return false;
        }

        private void SetRequiredValue(bool value)
        {
            var attribute = Attributes.FirstOrDefault(o => o.Name.Equals("IsRequired", StringComparison.InvariantCultureIgnoreCase));
            if (attribute == null)
                Parameters.Add(attribute = new Parameter(ConfigurationProvider) { Name = "IsRequired" });
            attribute.Value = value.ToString();
            OnPropertyChanged(nameof(IsRequired));
        }

        private Parameter GetDisplayNameAttribute()
        {
            var nameAttribute = Attributes.FirstOrDefault(o => o.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
            if (nameAttribute != null)
                return nameAttribute;
            nameAttribute = Attributes.FirstOrDefault(o => o.Name.Equals("ProgramName", StringComparison.InvariantCultureIgnoreCase));
            if (nameAttribute != null)
                return nameAttribute;
            return null;
        }

        private void Parameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                var thisItem = e.OldItems.Cast<Parameter>().FirstOrDefault(o => o == this);
                if (thisItem != null && Parent != null && Parent.Parameters != null)
                {
                    Parameters.CollectionChanged -= CurrentParameters_CollectionChanged;
                    _parent.Parameters.CollectionChanged -= Parameters_CollectionChanged;
                }
            }
        }

        private void CurrentParameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var oldItems = e.OldItems;
            //var newItems = e.NewItems;

            //if (oldItems != null)
            //{
            //    foreach (Parameter item in oldItems)
            //    {

            //        if (ConfigurationProvider.ParameterCache.TryGetValue(item.XPath, out Parameter parameter))
            //        {
            //            ConfigurationProvider.ParameterCache.Remove(item.XPath);
            //        }
            //    }
            //}

            //if (newItems != null)
            //{
            //    foreach (Parameter item in newItems)
            //    {
            //        if (!string.IsNullOrEmpty(item.XPath))
            //            ConfigurationProvider.ParameterCache[item.XPath] = item;
            //    }
            //}
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public string GetXPath()
        {
            var xElement1 = ConfigurationProvider.XDocument.XPathSelectElement(XPath).Parent;
            var xElement = ConfigurationProvider.XDocument.XPathSelectElement(XPath).ToString();
            return xElement;
        }
        public virtual void OnPropertyChanging(string propertyName)
        {
            if(propertyName!= null)
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
    }
}
