using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class DataProviderDefinition : IDataProviderDefinition
    {
        private readonly IFluentBuilder _builder;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly INameManager _nameManager;
        private readonly DataProvider _dataProvider;

        public DataProviderDefinition(
            DataProvider dataProvider,
            IFluentBuilder builder,
            IDataDependencyFactory dataDependencyFactory,
            INameManager nameManager)
        {
            _dataProvider = dataProvider;
            _builder = builder;
            _dataDependencyFactory = dataDependencyFactory;
            _nameManager = nameManager;
        }

        IDataProviderDefinition IDataProviderDefinition.Name(string name)
        {
            _dataProvider.Name = name;
            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.BindTo<T>(string scope)
        {
            var dataConsumer = _dataProvider as IDataConsumer;
            if (dataConsumer == null)
                throw new FluentBuilderException("This data provider is not a consumer of data");

            dataConsumer.HasDependency<T>(scope);

            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.BindTo(Type dataType, string scope)
        {
            var dataConsumer = _dataProvider as IDataConsumer;
            if (dataConsumer == null)
                throw new FluentBuilderException("This data provider is not a consumer of data");

            dataConsumer.HasDependency(dataType, scope);

            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.DependsOn(string dataProviderName)
        {
            var dataConsumer = _dataProvider as IDataConsumer;
            if (dataConsumer == null)
                throw new FluentBuilderException("This data provider is not a consumer of data");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n)),
                dataConsumer,
                dataProviderName);

            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.DependsOn(IDataProvider dataProvider)
        {
            var dataConsumer = _dataProvider as IDataConsumer;
            if (dataConsumer == null)
                throw new FluentBuilderException("This data provider is not a consumer of data");

            dataConsumer.HasDependency(dataProvider);

            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.PartOf(IPackage package)
        {
            _dataProvider.Package = package;
            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.PartOf(string packageName)
        {
            _dataProvider.Package = _nameManager.ResolvePackage(packageName);
            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.Provides<T>(string scope)
        {
            _dataProvider.Add<T>(scope);
            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.Provides(Type dataType, string scope)
        {
            _dataProvider.Add(dataType, scope);
            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.Provides(
            Type dataType, 
            Action<IRenderContext, IDataContext, IDataDependency> action, 
            string scope)
        {
            if (action == null)
            {
                _dataProvider.Add(dataType, scope);
            }
            else
            {
                var dataDependency = _dataDependencyFactory.Create(dataType, scope);
                var dataSupplier = (IDataSupplier)_dataProvider;
                dataSupplier.Add(dataDependency, action);
            }

            return this;
        }

        IDataProviderDefinition IDataProviderDefinition.Provides<T>(
            Action<IRenderContext, IDataContext, IDataDependency> action,
            string scope)
        {
            if (action == null)
            {
                _dataProvider.Add<T>(scope);
            }
            else
            {
                var dataSupplier = (IDataSupplier)_dataProvider;
                var dataDependency = _dataDependencyFactory.Create(typeof(T), scope);
                dataSupplier.Add(dataDependency, action);
            }

            return this;
        }

        IDataProvider IDataProviderDefinition.Build()
        {
            _builder.Register(_dataProvider);
            return _dataProvider;
        }

    }
}
