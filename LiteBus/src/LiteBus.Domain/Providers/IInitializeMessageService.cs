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

        public InitializeMessageService(IQueuePathProvider qPathProvider)
        {
            _qPathProvider = qPathProvider;
        }

        public void Init()
        {
            var queueName = Assembly.GetCallingAssembly().GetName().Name;
            var queuePath = _qPathProvider.GetPath(queueName);
            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }
        }
    }
}
