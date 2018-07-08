using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataConsumerFactory: IDataConsumerFactory
    {
        private readonly IDataDependencyFactory _dataDependencyFactory;

        public DataConsumerFactory(
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataDependencyFactory = dataDependencyFactory;
        }

        public IDataConsumer Create()
        {
            return new DataConsumer(
                _dataDependencyFactory);
        }
    }
}
