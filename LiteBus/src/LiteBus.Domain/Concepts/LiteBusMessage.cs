using System;
using System.Reflection;

namespace LiteBus.Domain.Concepts
{
    public class LiteBusMessage
    {
        public string OriginatingEndpoint { get; set; }
        public string DestinationEndpoint { get; set; }
        public DateTime TimeSent { get; set; }
        public string ObjectType { get; set; }
        public string ObjectAssembly { get; set; }
        public string Message { get; set; }
    }
}
