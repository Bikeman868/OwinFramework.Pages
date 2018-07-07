using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataConsumerFactory : MockImplementationProvider<IDataConsumerFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataConsumerFactory> mock)
        {
            mock.Setup(m => m.Create()).Returns(mockProducer.SetupMock<IDataConsumer>());
        }
    }
}
