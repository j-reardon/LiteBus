using LiteBus.Core;
using LiteBus.Domain;
using LiteBus.Domain.Concepts;
using LiteBus.Domain.Providers;
using StructureMap;
using System;
using System.ServiceProcess;

namespace LiteBus.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Container.For<DomainRegistry>();
            var initializer = container.GetInstance<IInitializeMessageService>();
            var qPathProvider = container.GetInstance<IQueuePathProvider>();
            var serializer = container.GetInstance<ISerializationProvider>();
            var messageHandler = new MessageService(initializer, qPathProvider, serializer, container);
            if (Environment.UserInteractive)
            {
                Test(container.GetInstance<MessageSender>(), "litebus.host");
                messageHandler.RunAsConsole(args);
            }
            else
            {
                ServiceBase[] servicesToRun;
                servicesToRun = new ServiceBase[] { messageHandler };
                ServiceBase.Run(servicesToRun);
            }
        }

        private static void Test(IMessageSender bus, string qPath)
        {
            bus.Send(new TestMessage
            {
                Prop1 = "Property One",
                Prop2 = "Property Two"
            }, qPath);
        }
    }
}
