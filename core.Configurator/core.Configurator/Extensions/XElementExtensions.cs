using System;
using System.Linq;
using System.Xml.Linq;

namespace mop.Configurator
{
    public static class XElementExtensions
    {
        public static string GetXPath(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Func<XElement, string> relativeXPath = e =>
            {
                var index = e.GetIndexPosition();
                var name = e.Name.LocalName;
                var strXPath = string.Empty;

                var nameAttrValue = e.Attribute("Name")?.Value;
                
                if(!string.IsNullOrEmpty(nameAttrValue))
                {
                    if(nameAttrValue != null)
                    {
                        strXPath = $"@Name='{nameAttrValue}'";
                    }
                }

            return (index == -1) ? "/" + name : string.Format
            (
                "/{0}[{1}]",
                   name,
                   string.IsNullOrEmpty(strXPath) ? index.ToString() : strXPath
                );
            };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
        }

        private static int GetIndexPosition(this XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element), "Ошибка обработки документа. Передан пустой XElement");

            if (element.Parent == null)
                return -1;

            int i = 1;

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }
                i++;
            }

            throw new InvalidOperationException("Ошибка обработки документа. Элемент не найден.");
        }

        private static string GetRelativeXPath(XElement e)
        {
            var index = e.GetIndexPosition();
            var name = e.Name.LocalName;
            if (index == -1)
                return "/";
            return $"/{name}[{index}]";
        }
    }
}
