using LiteBus.Domain.Providers;
using StructureMap;

namespace LiteBus.Domain
{
    public class DomainRegistry : Registry
    {
        public DomainRegistry()
        {
            For<ISerializationProvider>()
                .Use<XmlSerializationProvider>();

            Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();
            });
        }
    }
}
