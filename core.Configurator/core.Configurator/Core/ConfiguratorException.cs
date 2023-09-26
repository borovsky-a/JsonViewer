using System;
using System.Runtime.Serialization;

namespace mop.Configurator
{
    [Serializable]
    public class ConfiguratorException : Exception
    {
        public ConfiguratorException()
        {
        }

        public ConfiguratorException(string message) : base(message)
        {
        }

        public ConfiguratorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfiguratorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
