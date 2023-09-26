using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mop.Configurator
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class ParameterAttribute : Attribute
    {
        public PropertyType PropertyType { get; set; } = PropertyType.Property;
    }

    public enum PropertyType
    {
        Section,
        Property
    }

}
