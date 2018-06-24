using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataProviderDefinitionFactory : IDataProviderDefinitionFactory
    {
        public IDataProviderDefinition Create(IDataProvider dataProvider, IDataDependency dependency)
        {
            return new DataProviderDefinition
            {
                DataProvider = dataProvider,
                Dependency = dependency
            };
        }
    }
}
