using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Mocks.Collections
{
    public class MockQueueFactory: ConcreteImplementationProvider<IQueueFactory>
    {
        private readonly IQueueFactory _queueFactory;

        public MockQueueFactory()
        {
            _queueFactory = new QueueFactory();
        }

        protected override IQueueFactory GetImplementation(IMockProducer mockProducer)
        {
            return _queueFactory;
        }
    }
}
