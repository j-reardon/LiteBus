using System;
using System.IO;
using System.Xml.Serialization;

namespace LiteBus.Domain.Providers
{
    public class XmlSerializationProvider
        : ISerializationProvider
    {
        public T Deserialize<T>(string str)
        {
            return (T)Deserialize(str, typeof(T));
        }

        public object Deserialize(string str, Type type)
        {
            var serializer = new XmlSerializer(type);
            using (var stringReader = new StringReader(str))
            {
                return serializer.Deserialize(stringReader);
            }
        }

        public string Serialize<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}
