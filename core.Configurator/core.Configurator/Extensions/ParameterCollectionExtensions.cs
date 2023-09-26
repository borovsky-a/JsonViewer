using System.Collections.Generic;

namespace mop.Configurator
{
    public static class ParameterCollectionExtensions
    {
        public static IDictionary<string, Parameter> ToDictionary(this ParameterCollection parameters)
        {
            var result = new Dictionary<string, Parameter>();
            foreach (var parameter in parameters)
            {
                result[parameter.XPath] = parameter;
                ToDictionary(parameter, result);
            }
            return result;
        }
        public static Parameter GetParameterByPath(this ParameterCollection parameters, string path, Parameter parameter = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var isNull = parameter == null;
            if (!isNull && parameter.XPath == path)
                return parameter;

            var parameterCollection = isNull ? parameters : parameter.Parameters;
            foreach (var item in parameterCollection)
            {
                var result = GetParameterByPath(parameters, path, item);
                if (result == null)
                    continue;
                return result;
            }
            return null;
        }
        private static void ToDictionary(Parameter parameter, Dictionary<string, Parameter> result)
        {
            foreach (var item in parameter.Parameters)
            {
                result[item.XPath] = item;
                ToDictionary(item, result);
            }
        }
    }
}
