using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.Builder
{
    public class MockComponentDependenciesFactory : MockImplementationProvider<IComponentDependenciesFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IComponentDependenciesFactory> mock)
        {
            mock.Setup(m => m.DataConsumerFactory).Returns(mockProducer.SetupMock<IDataConsumerFactory>);
        }
    }
}
