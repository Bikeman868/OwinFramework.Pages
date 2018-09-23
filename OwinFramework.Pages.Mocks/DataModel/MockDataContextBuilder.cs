using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataContextBuilder: MockImplementationProvider<IDataContextBuilder>
    {
        public List<IDataDependency> DependenciesInScope;

        public MockDataContextBuilder()
        {
            DependenciesInScope = new List<IDataDependency>();
        }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataContextBuilder> mock)
        {
            mock.Setup(m => m.IsInScope(It.IsAny<IDataDependency>()))
                .Returns<IDataDependency>(d => DependenciesInScope.Any(s => s.Equals(d)));
        }
    }
}
