using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace mop.Configurator.Editors
{
    public class EnumEditor : EditorBase
    {
        static Dictionary<string, List<string>> _cache = new Dictionary<string, List<string>>();
        private bool _isEmpty;
        private string _internalError;
        private string _selected;

        public ObservableCollection<string> Enums
        {
            get { return InitList(); }            
        }
        public override string[] SuitableNames { get; set; } = new[] { "bool", "enum" };
        public override int Sequence { get; set; } = 6;
        public override string Header { get; set; } = "Перечисление";

        public string InternalError
        {
            get { return _internalError; }
            set
            {
                if (_internalError != value)
                {
                    _internalError = value;
                    OnPropertyChanged(nameof(InternalError));
                }
            }
        }
       
        public bool IsEmpty
        {
            get { return _isEmpty; }
            set
            {
                if(_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }
        public override string this[string columnName]
        {
            get
            {
                var error = base[columnName];
                return Error = error;             
            }
        }
        

        public override bool IsVisible
        {
            get { return base.IsVisible && (Parameter.EditorType.Equals("bool", StringComparison.InvariantCultureIgnoreCase) || !Source.Where(o=> o != this).SelectMany(o=> o.SuitableNames).Where(o=> o.Equals(Parameter.EditorType, StringComparison.InvariantCultureIgnoreCase)).Any()); }
            set
            {
                base.IsVisible = value;
            }
        }
        private ObservableCollection<string> InitList()
        {
            var result = new ObservableCollection<string>();
            var valueType = Parameter.ValueType;
            if (!string.IsNullOrEmpty(valueType))
            {
                try
                {
                    if (!_cache.TryGetValue(valueType, out List<string> values))
                    {
                        values = new List<string>();
                        if (valueType.Equals("bool", StringComparison.InvariantCultureIgnoreCase))
                        {
                            values.Add("0");
                            values.Add("1");
                            values.Add("False");
                            values.Add("True");
                            _cache["bool"] = values;
                        }
                        else
                        {
                            var doc = Parameter.ConfigurationProvider.XDocument;
                            var typeElement = doc.Descendants("Type")
                                .Where(o => o.Attribute("Name")?.Value != null)
                                .Where(o => o.Attribute("CSTypeName")?.Value != null)
                                .Where(o => o.Attribute("SerializationType")?.Value != null)
                                .Where(o => o.Attribute("Name").Value == valueType)
                                .FirstOrDefault();
                            if (typeElement != null)
                            {
                                var serializationType = typeElement.Attribute("SerializationType").Value;
                                var csTypeName = typeElement.Attribute("CSTypeName").Value;
                                if (csTypeName != null)
                                {
                                    var type = Type.GetType(csTypeName);
                                    if (serializationType.Equals("Enum", StringComparison.InvariantCultureIgnoreCase) && type.IsEnum)
                                    {
                                        foreach (var e_name in Enum.GetNames(type))
                                        {
                                            values.Add(e_name);
                                        }
                                        _cache[valueType] = values;
                                    }
                                }
                            }
                        }
                    }
                    values.ForEach(o => result.Add(o));
                }
                catch (Exception ex)
                {
                    InternalError = $"При создании редактора для типа перечисления произошла ошибка.  Название: {Parameter.Parent?.DisplayName} Тип: {Parameter.ValueType} Ошибка: {ex}";
                }    
            }
            IsEmpty = result.Count == 0;
            return result;           
        }
    }
}
