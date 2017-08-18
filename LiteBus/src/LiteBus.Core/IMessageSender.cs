using LiteBus.Domain.Concepts;
using LiteBus.Domain.Providers;
using System;
using System.Messaging;
using System.Reflection;

namespace LiteBus.Core
{
    public interface IMessageSender
    {
        void Send<T>(T message, string queue);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IQueuePathProvider _qPathProvider;
        private readonly ISerializationProvider _serializer;

        public MessageSender(
            IQueuePathProvider qPathProvider,
            ISerializationProvider serializer)
        {
            _qPathProvider = qPathProvider;
            _serializer = serializer;
        }

        public void Send<T>(T message, string queue)
        {
            var liteBusMessage = new LiteBusMessage
            {
                OriginatingEndpoint = Assembly.GetCallingAssembly().GetName().Name,
                DestinationEndpoint = queue,
                TimeSent = DateTime.Now,
                ObjectType = typeof(T).ToString(),
                ObjectAssembly = typeof(T).Assembly.ToString(),
                Message = _serializer.Serialize(message)
            };

            using (var q = new MessageQueue(_qPathProvider.GetPath(queue)))
            {
                q.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                q.Send(_serializer.Serialize(liteBusMessage));
            }
        }
    }
}
