using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataProviderDefinition : IDataProviderDefinition
    {
        public IDataProvider DataProvider { get; set; }
        public IDataDependency Dependency { get; set; }

        public void Execute(IRenderContext renderContext, IDataContext dataContext)
        {
            DataProvider.Satisfy(renderContext, dataContext, Dependency);
        }
    }
}
