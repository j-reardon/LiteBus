using LiteBus.Domain.Concepts;
using StructureMap;
using System;
using System.Linq;
using System.Messaging;
using System.Reflection;

namespace LiteBus.Domain.Providers
{
    public interface IInitializeMessageService
    {
        void Init();
    }

    public class InitializeMessageService
        : IInitializeMessageService
    {
        private readonly IQueuePathProvider _qPathProvider;
        private readonly IContainer _container;

        public InitializeMessageService(IQueuePathProvider qPathProvider, IContainer container)
        {
            _qPathProvider = qPathProvider;
            _container = container;
        }

        public void Init()
        {
            Console.WriteLine("Initializing endpoint...");
            var queueName = Assembly.GetCallingAssembly().GetName().Name;
            var queuePath = _qPathProvider.GetPath(queueName);
            if (!MessageQueue.Exists(queuePath))
            {
                Console.WriteLine("Queue '{0}' doesn't exist, creating...", queueName);
                MessageQueue.Create(queuePath);
            }

            StartBus();
        }

        private void StartBus()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            Console.WriteLine("Running startup on {0}...", callingAssembly.GetName().Name);

            var runOnStartupType = typeof(IRunOnStartup);
            var types = callingAssembly.GetTypes()
                .Where(p => runOnStartupType.IsAssignableFrom(p) && !p.IsInterface).ToList();

            foreach (var type in types)
            {
                Console.WriteLine("loading {0}...", type);
                var instance = (IRunOnStartup)_container.GetInstance(type);
                instance.Start();
            }
        }
    }
}
