using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Mocks.Managers
{
    public class MockIdManager: MockImplementationProvider<IIdManager>
    {
        private int _next = 1;

        protected override void SetupMock(IMockProducer mockProducer, Moq.Mock<IIdManager> mock)
        {
            mock.Setup(m => m.GetUniqueId()).Returns(() => _next++);
        }
    }
}
