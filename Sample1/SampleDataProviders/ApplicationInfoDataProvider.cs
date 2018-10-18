using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample1.SampleDataProviders
{
    internal class ApplicationInfo
    {
        public string Name { get { return "Sample 1"; } }
    }

    [IsDataProvider("application", typeof(ApplicationInfo))]
    internal class ApplicationInfoDataProvider : DataProvider
    {
        private readonly ApplicationInfo _applicationInfo;

        public ApplicationInfoDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies)
        {
            _applicationInfo = new ApplicationInfo();
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_applicationInfo);
        }
    }
}