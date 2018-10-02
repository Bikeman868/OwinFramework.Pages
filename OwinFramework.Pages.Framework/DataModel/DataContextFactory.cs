using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContextFactory : ReusableObjectFactory, IDataContextFactory
    {
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IIdManager _idManager;

        public DataContextFactory(
            IQueueFactory queueFactory,
            IDictionaryFactory dictionaryFactory,
            IDataDependencyFactory dataDependencyFactory,
            IIdManager idManager)
            : base(queueFactory)
        {
            _dictionaryFactory = dictionaryFactory;
            _dataDependencyFactory = dataDependencyFactory;
            _idManager = idManager;

            Initialize(100);
        }

        public IDataContext Create(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder)
        {
            return Create(renderContext, dataContextBuilder, null);
        }

        public IDataContext Create(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder,
            DataContext parent)
        {
            var dataContext = (DataContext)Queue.DequeueOrDefault()
                ?? new DataContext(_dictionaryFactory, this, _dataDependencyFactory, _idManager);

            return dataContext.Initialize(DisposeAction, renderContext, dataContextBuilder, parent);
        }
    }
}
