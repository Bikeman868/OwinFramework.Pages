using System;
using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataDependencyFactory: MockImplementationProvider<IDataDependencyFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataDependencyFactory> mock)
        {
            mock.Setup(m => m.Create(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns<Type, string>((t, s) => new DataDependency { DataType = t, ScopeName = s });
        }

        private class DataDependency: IDataDependency
        {
            public Type DataType { get; set; }
            public string ScopeName { get; set; }

            public bool Equals(IDataDependency other)
            {
                if (ReferenceEquals(other, null)) return false;
                if (DataType != other.DataType) return false;
                return string.Equals(ScopeName, other.ScopeName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
