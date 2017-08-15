using System;

namespace LiteBus.Domain.Providers
{
    public interface ISerializationProvider
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string str);
        object Deserialize(string str, Type type);
    }
}
