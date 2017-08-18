using LiteBus.Domain.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteBus.Core
{
    public class TestHandler2
        : IHandleMessages<TestMessage>
    {
        public void Handle(TestMessage message)
        {
        }
    }
}
