namespace LiteBus.Core
{
    public interface IHandleMessages
    {
    }

    public interface IHandleMessages<T>
        : IHandleMessages
    {
        void Handle(T message);
    }
}
