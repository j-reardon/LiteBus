using LiteBus.Domain;
using LiteBus.Domain.Concepts;
using LiteBus.Domain.Providers;
using StructureMap;
using System;
using System.Messaging;
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
            var messageHandler = new MessageService(initializer, qPathProvider, serializer);
            if (Environment.UserInteractive)
            {
                var xmlSer = new XmlSerializationProvider();
                var message = new TestMessage
                {
                    Prop1 = "Property One",
                    Prop2 = "Property Two"
                };
                var liteBusMessage = new LiteBusMessage
                {
                    OriginatingEndpoint = "OriginatingEndpoint",
                    DestinationEndpoint = "DestinationEndpoint",
                    ObjectType = message.GetType().ToString(),
                    ObjectAssembly = message.GetType().Assembly.ToString(),
                    TimeSent = DateTime.Now,
                    Message = xmlSer.Serialize(message)
                };
                using (var q = new MessageQueue(@".\Private$\LiteBus.Host"))
                {
                    q.Send(xmlSer.Serialize(liteBusMessage));
                }

                messageHandler.RunAsConsole(args);
            }
            else
            {
                ServiceBase[] servicesToRun;
                servicesToRun = new ServiceBase[] { messageHandler };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
