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
    public class MockDataProviderDependenciesFactory : MockImplementationProvider<IDataProviderDependenciesFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataProviderDependenciesFactory> mock)
        {
            mock.Setup(m => m.DataConsumerFactory).Returns(mockProducer.SetupMock<IDataConsumerFactory>);
            mock.Setup(m => m.DataSupplierFactory).Returns(mockProducer.SetupMock<IDataSupplierFactory>);
            mock.Setup(m => m.DataDependencyFactory).Returns(mockProducer.SetupMock<IDataDependencyFactory>);
        }
    }
}
