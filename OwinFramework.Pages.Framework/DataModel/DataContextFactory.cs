using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContextFactory : ReusableObjectFactory, IDataContextFactory
    {
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;

        public DataContextFactory(
            IQueueFactory queueFactory,
            IDictionaryFactory dictionaryFactory,
            IDataDependencyFactory dataDependencyFactory)
            : base(queueFactory)
        {
            _dictionaryFactory = dictionaryFactory;
            _dataDependencyFactory = dataDependencyFactory;
            Initialize(100);
        }

        public IDataContext Create(IRenderContext renderContext)
        {
            return Create(renderContext, null);
        }

        public IDataContext Create(IRenderContext renderContext, DataContext parent)
        {
            var dataContext = (DataContext)Queue.DequeueOrDefault()
                ?? new DataContext(_dictionaryFactory, this, _dataDependencyFactory);

            return dataContext.Initialize(DisposeAction, renderContext, parent);
        }
    }
}
