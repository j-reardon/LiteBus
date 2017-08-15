using LiteBus.Domain.Concepts;
using LiteBus.Domain.Providers;
using System;
using System.Messaging;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace LiteBus.Host
{
    public class MessageService : ServiceBase
    {
        private readonly IInitializeMessageService _messageServiceInitializer;
        private readonly IQueuePathProvider _qPathProvider;
        private readonly ISerializationProvider _serializer;
        private Thread _workerThread;
        private ManualResetEvent _shutdownEvent;
        private readonly string _queueName;

        public MessageService(
            IInitializeMessageService messageServiceInitializer, 
            IQueuePathProvider qPathProvider,
            ISerializationProvider serializer)
        {
            _messageServiceInitializer = messageServiceInitializer;
            _qPathProvider = qPathProvider;
            _serializer = serializer;
            _shutdownEvent = new ManualResetEvent(false);
            _queueName = Assembly.GetCallingAssembly().GetName().Name;
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("starting...");
            _messageServiceInitializer.Init();
            _workerThread = new Thread(new ThreadStart(ListenForMessages));
            _workerThread.Start();
        }

        private void ListenForMessages()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                using (var q = new MessageQueue(_qPathProvider.GetPath(_queueName)))
                {
                    q.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                    using (var messageEnumerator = q.GetMessageEnumerator2())
                        if (messageEnumerator.MoveNext())
                        {
                            var message = messageEnumerator.Current;
                            DeserializeMessage(message.Body.ToString());
                            messageEnumerator.RemoveCurrent();
                        }
                }
            }
        }

        private void DeserializeMessage(string message)
        {
            var liteBusMessage = _serializer.Deserialize<LiteBusMessage>(message);
            var messageType = Type.GetType($"{liteBusMessage.ObjectType}, {liteBusMessage.ObjectAssembly}");
            var messageObj = _serializer.Deserialize(liteBusMessage.Message, messageType);
        }

        protected override void OnStop()
        {
            Console.WriteLine("stopping...");
            _shutdownEvent.Set();
            if (!_workerThread.Join(5000))
            {
                _workerThread.Abort();
            }
        }

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            OnStop();
        }
    }
}
