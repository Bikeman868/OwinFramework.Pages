using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Collections;

namespace OwinFramework.Pages.Facilities.Runtime
{
    internal class DataContextFactory : ReusableObjectFactory, IDataContextFactory
    {
        private readonly IDictionaryFactory _dictionaryFactory;

        public DataContextFactory(
            IQueueFactory queueFactory,
            IDictionaryFactory dictionaryFactory)
            : base(queueFactory)
        {
            _dictionaryFactory = dictionaryFactory;
            Initialize(100);
        }

        public IDataContext Create()
        {
            return Create(null);
        }

        public IDataContext Create(DataContext parent)
        {
            var dataContext = (DataContext)Queue.DequeueOrDefault()
                ?? new DataContext(_dictionaryFactory, this);

            return dataContext.Initialize(DisposeAction, parent);
        }
    }
}
