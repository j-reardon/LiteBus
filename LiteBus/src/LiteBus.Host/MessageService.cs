using LiteBus.Core;
using LiteBus.Domain.Concepts;
using LiteBus.Domain.Providers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IContainer _container;
        private Thread _workerThread;
        private ManualResetEvent _shutdownEvent;
        private readonly string _queueName;

        public MessageService(
            IInitializeMessageService messageServiceInitializer, 
            IQueuePathProvider qPathProvider,
            ISerializationProvider serializer,
            IContainer container)
        {
            _messageServiceInitializer = messageServiceInitializer;
            _qPathProvider = qPathProvider;
            _serializer = serializer;
            _container = container;
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
                    {
                        if (messageEnumerator.MoveNext())
                        {
                            var message = messageEnumerator.Current;
                            var liteBusMessage = _serializer.Deserialize<LiteBusMessage>(message.Body.ToString());
                            CallHandler(liteBusMessage);
                            messageEnumerator.RemoveCurrent();
                        }
                    }
                }
            }
        }

        private void CallHandler(LiteBusMessage liteBusMessage)
        {
            var messageType = Type.GetType($"{liteBusMessage.ObjectType}, {liteBusMessage.ObjectAssembly}");
            var message = Convert.ChangeType(_serializer.Deserialize(liteBusMessage.Message, messageType), messageType);
            var handlers = GetAllHandlerImplementations(message).ToList();
            if (!handlers.Any())
                throw new InvalidOperationException($"Could not locate handler for message type {messageType}");

            foreach (var handler in handlers)
            {
                var handleMethod = handler.Key.GetRuntimeMethod("Handle", new Type[] { message.GetType() });
                handleMethod.Invoke(handler.Value, new object[] { message });
            }
        }

        private Dictionary<Type, IHandleMessages> GetAllHandlerImplementations<T>(T obj)
        {
            var handlerTypes = GetMatchingHandlerTypes(obj);
            var handlers = new Dictionary<Type, IHandleMessages>();
            foreach (var handlerType in handlerTypes)
            {
                handlers.Add(handlerType, (IHandleMessages)_container.GetInstance(handlerType));
            }

            return handlers;
        }

        private IEnumerable<Type> GetMatchingHandlerTypes<T>(T obj)
        {
            var messageHandlerType = typeof(IHandleMessages);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => messageHandlerType.IsAssignableFrom(p) && !p.IsInterface).ToList();

            var messageHandlers = new List<Type>();
            foreach (var handlerType in types)
            {
                foreach (var interf in handlerType.GetInterfaces())
                {
                    var thing = interf;
                }

                foreach (var type in handlerType.GetInterfaces())
                {
                    var genericTypeArguments = type.GenericTypeArguments;
                    if (genericTypeArguments.Any(x => x == obj.GetType()))
                    {
                        messageHandlers.Add(handlerType);
                    }
                }
            }

            return messageHandlers;
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
