using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace mop.Configurator
{
    public class ParameterCollection : ObservableCollection<Parameter>
    {
        public new void Add(Parameter value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Ошибка добавления элемента конфигурации. Передано пустое значение.");
            var parent = value.Parent;
            if (parent == null)
                base.Add(value);
            else parent.Parameters.Add(value);
        }      

        public virtual Parameter GetParameterByPath(string path)
        {
            foreach (var item in this)
            {
                var result = GetParameterByPath(item, path);
                if (result == null)
                    continue;
                return result;
            }
            return null;
        }

        protected virtual Parameter GetParameterByPath(string[] segments)
        {
            var path = "/" + string.Join("/", segments.ToArray());
            return GetParameterByPath(path);
        }

        protected virtual Parameter GetParameterByPath(Parameter parameter, string path)
        {
            if (parameter.XPath == path)
                return parameter;
            foreach (var item in parameter.Parameters)
            {
                var result = GetParameterByPath(item, path);
                if (result == null)
                    continue;
                return result;
            }
            return null;
        }
    }
}
