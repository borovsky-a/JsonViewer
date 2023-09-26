using mop.Configurator.Log;
using System;
using System.Linq;
using System.Xml.Linq;

namespace mop.Configurator
{
    public class ParameterCollectionBuilder : IParameterCollectionBuilder
    {
        private ConfigurationProvider _configurationProvider;
        public ParameterCollectionBuilder(ConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }      
       
        public ParameterCollection Create(XDocument document)
        {
            if (document == null)
                throw new ConfiguratorException($"Ошибка при обновлении параметров конфигурации. В качастве документа передано пустое значение.");
            var parameters = new ParameterCollection();
            
            foreach (var item in document.Elements())
            {
                Refresh(item, parameters);
            }
            return parameters;
        }
        private void Refresh(XElement item, ParameterCollection parameters, Parameter parent = null)
        {
            if (item.Attributes().Count() == 0 && item.Elements().Count() > 0)
            {
                foreach (var elem in item.Elements())
                {
                    Refresh(elem, parameters, parent);
                }
            }
            else
            {
                var path = item.GetXPath();
                var parameter = new Parameter(_configurationProvider);

                
                foreach (var attr in item.Attributes())
                {

                    var arrtibuteParameter = new Parameter(_configurationProvider);

                    arrtibuteParameter.Name = attr.Name.ToString();
                    arrtibuteParameter.Value = attr.Value;
                    parameter.Attributes.Add(arrtibuteParameter);
                }

                parameter.Name = item.Name.ToString();
                parameter.XPath = path;
                parameter.Parent = parent;

                if (!parameter.IsVisible || (item.Elements().Count() == 0 && parameter.Name == "Parameter"))
                    return;
                if (parameter.DisplayName == "DefaultStringValue" || parameter.DisplayName == "DefaultIntValue" || parameter.DisplayName == "DefaultDateTimeValue")
                    parameter.IsEditable = false; ;

                
                parameters.Add(parameter);

                if (item.Elements().Count() == 0)
                {
                    if (item.FirstNode is XCData data)
                    {
                        parameter.IsCData = true;
                        parameter.Value = data.Value.Trim();
                    }
                    else parameter.Value = item.Value;

                    //есди раскомментировать этот кусок кода, то для пустого значения будет использоваться значение по умолчанию
                    //if (string.IsNullOrEmpty(parameter.Value))
                    //{
                    //    string defaultValue;
                    //    switch (parameter.Name)
                    //    {
                    //        case "StringValue":
                    //             defaultValue = item.Parent.Element("DefaultStringValue")?.Value;
                    //            break;
                    //        case "IntValue":
                    //            defaultValue = item.Parent.Element("DefaultIntValue")?.Value;
                    //            break;
                    //        case "DateTimeValue":
                    //            defaultValue = item.Parent.Element("DefaultDateTimeValue")?.Value;
                    //            break;
                    //        default:
                    //            defaultValue = null;
                    //            break;
                    //    }
                    //    if (!string.IsNullOrEmpty(defaultValue))
                    //    {
                    //        parameter.Value = defaultValue;
                    //    }                      
                    //}
                }
                else
                {
                    foreach (var elem in item.Elements())
                    {
                        Refresh(elem, parameters, parameter);
                    }
                }
                parameter.PropertyChanged += _configurationProvider.Parameter_PropertyChanged;
            }
        }
      
    }
}
