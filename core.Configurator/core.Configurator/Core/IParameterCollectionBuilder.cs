using System.Xml.Linq;

namespace mop.Configurator
{
    public interface IParameterCollectionBuilder
    {
        ParameterCollection Create(XDocument document);
    }
}