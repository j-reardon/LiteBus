using System;
using LiteBus.Domain.Concepts;

namespace LiteBus.Core
{
    public class TestHandler : IHandleMessages<TestMessage>
    {

        public void Handle(TestMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
