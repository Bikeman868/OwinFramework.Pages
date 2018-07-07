using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataSupplierFactory : MockImplementationProvider<IDataSupplierFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataSupplierFactory> mock)
        {
            mock.Setup(m => m.Create()).Returns(mockProducer.SetupMock<IDataSupplier>());
        }
    }
}
