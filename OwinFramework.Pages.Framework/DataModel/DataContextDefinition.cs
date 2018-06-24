using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContextDefinition : IDataContextDefinition
    {
        public IList<IDataProviderDefinition> DataProviders { get; private set; }

        public void Add(IDataProviderDefinition dataProvider)
        {
        }

        public void Add(IDataProvider dataProvider, IDataDependency dependancy)
        {
        }
    }
}
