using System.Linq;
using System.Collections.Generic;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Standard;
using Sample5.Session;

namespace Sample5.DataProviders
{
    /// <summary>
    /// This data provider defines the menu options on all pages of the website
    /// </summary>
    [IsDataProvider("session", typeof(ISessionData))]
    public class SessionDataProvider : DataProvider
    {
        public SessionDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            var session = renderContext.OwinContext.GetFeature<ISession>();
            dataContext.Set<ISessionData>(new SessionData(session));
        }
    }
}