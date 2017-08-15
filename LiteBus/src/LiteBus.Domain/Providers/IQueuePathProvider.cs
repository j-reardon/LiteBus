namespace LiteBus.Domain.Providers
{
    public interface IQueuePathProvider
    {
        string GetPath(string queueName);
    }

    public class QueuePathProvider
        : IQueuePathProvider
    {
        public string GetPath(string queueName)
        {
            return $@".\Private$\{queueName}";
        }
    }
}
