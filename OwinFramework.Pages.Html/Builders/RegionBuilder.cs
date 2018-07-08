using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    internal class RegionBuilder: IRegionBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IDataSupplierFactory _dataSupplierFactory;

        public RegionBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IRegionDependenciesFactory regionDependenciesFactory,
            IFluentBuilder fluentBuilder,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _regionDependenciesFactory = regionDependenciesFactory;
            _fluentBuilder = fluentBuilder;
            _dataDependencyFactory = dataDependencyFactory;
            _dataSupplierFactory = dataSupplierFactory;
        }

        IRegionDefinition IRegionBuilder.Region(Type declaringType, IPackage package)
        {
            return new RegionDefinition(
                declaringType, 
                _nameManager, 
                _htmlHelper, 
                _fluentBuilder, 
                _regionDependenciesFactory, 
                _dataDependencyFactory, 
                _dataSupplierFactory, 
                package);
        }
    }
}
