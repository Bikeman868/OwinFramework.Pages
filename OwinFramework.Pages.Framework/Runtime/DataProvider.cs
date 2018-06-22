using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// You can inherit from this base class to insulate your implementation from
    /// future additions to the IDataProvider interface
    /// </summary>
    public class DataProvider: IDataProvider
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }

        private readonly List<string> _scopes = new List<string>();

        protected DataProvider(IDataProviderDependenciesFactory dependencies)
        {

        }

        public void AddScope(string scopeName)
        {
            _scopes.Add(scopeName);
        }

        public ICollection<string> Scopes
        {
            get { return _scopes; }
        }

        /// <summary>
        /// Override this method if your data provider can return different types of data
        /// </summary>
        public virtual void EstablishContext(IRenderContext renderContext, IDataContext dataContext, Type dataType)
        {
            EstablishContext(renderContext, dataContext);
        }

        /// <summary>
        /// Override this method if your data provider only provides one type of data
        /// </summary>
        public virtual void EstablishContext(IRenderContext renderContext, IDataContext dataContext)
        {
            throw new NotImplementedException("Data providers must override one of the EstablishContext overloads");
        }
    }
}
