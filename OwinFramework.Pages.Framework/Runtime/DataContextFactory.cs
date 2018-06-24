using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class DataContextFactory : ReusableObjectFactory, IDataContextFactory
    {
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly IDataCatalog _dataCatalog;

        public DataContextFactory(
            IQueueFactory queueFactory,
            IDictionaryFactory dictionaryFactory,
            IDataCatalog dataCatalog)
            : base(queueFactory)
        {
            _dictionaryFactory = dictionaryFactory;
            _dataCatalog = dataCatalog;
            Initialize(100);
        }

        public IDataContext Create(IRenderContext renderContext)
        {
            return Create(renderContext, null);
        }

        public IDataContext Create(IRenderContext renderContext, DataContext parent)
        {
            var dataContext = (DataContext)Queue.DequeueOrDefault()
                ?? new DataContext(_dictionaryFactory, this, _dataCatalog);

            return dataContext.Initialize(DisposeAction, renderContext, parent);
        }
    }
}
