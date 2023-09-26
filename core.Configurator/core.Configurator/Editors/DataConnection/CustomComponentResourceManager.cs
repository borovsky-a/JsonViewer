using System;
using System.ComponentModel;

namespace mop.Configurator.Editors.DataConnection
{
    internal class CustomComponentResourceManager : ComponentResourceManager
    {
        public CustomComponentResourceManager(Type type, string resourceName)
           : base(type)
        {
            this.BaseNameField = resourceName;
        }
    }
}
