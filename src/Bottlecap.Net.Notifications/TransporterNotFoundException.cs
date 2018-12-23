using System;
using System.Runtime.Serialization;

namespace Bottlecap.Net.Notifications
{
    [Serializable]
    public class TransporterNotFoundException : Exception
    {
        public TransporterNotFoundException()
        {
        }

        public TransporterNotFoundException(string message) : base(message)
        {
        }

        public TransporterNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TransporterNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}