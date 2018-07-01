using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataConsumerFactory: IDataConsumerFactory
    {
        private readonly IDataProviderDefinitionFactory _dataProviderDefinitionFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;

        public DataConsumerFactory(
            IDataProviderDefinitionFactory dataProviderDefinitionFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataDependencyFactory = dataDependencyFactory;
        }

        public IDataConsumer Create()
        {
            return new DataConsumer(
                _dataProviderDefinitionFactory,
                _dataDependencyFactory);
        }
    }
}
