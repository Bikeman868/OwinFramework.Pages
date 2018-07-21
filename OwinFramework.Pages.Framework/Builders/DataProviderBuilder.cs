using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Framework.DataModel;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// Plug-in to the fluent builder for building data providers.
    /// Uses the supplied DataProvider or constructs a new DataProvider instance.
    /// Returns a fluent interface for defining the data provider characteristics
    /// </summary>
    internal class DataProviderBuilder : IDataProviderBuilder
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IDataProviderDependenciesFactory _dataProviderDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly INameManager _nameManager;

        public DataProviderBuilder(
            IDataProviderDependenciesFactory dataProviderDependenciesFactory,
            IElementConfiguror elementConfiguror,
            INameManager nameManager,
            IFluentBuilder fluentBuilder)
        {
            _dataProviderDependenciesFactory = dataProviderDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _nameManager = nameManager;
            _fluentBuilder = fluentBuilder;
        }

        IDataProviderDefinition IDataProviderBuilder.BuildUpDataProvider(object dataProviderInstance, Type declaringType, IPackage package)
        {
            var dataProvider = dataProviderInstance as DataProvider ?? new DataProvider(_dataProviderDependenciesFactory);
            if (declaringType == null) declaringType = (dataProviderInstance ?? dataProvider).GetType();

            var dataProviderDefinition = new DataProviderDefinition(
                dataProvider, 
                _fluentBuilder, 
                _dataProviderDependenciesFactory.DataDependencyFactory, 
                _nameManager,
                package);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(dataProviderDefinition, attributes);

            return dataProviderDefinition;
        }
    }
}