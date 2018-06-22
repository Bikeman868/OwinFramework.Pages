﻿using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IComponentDependenciesFactory componentDependenciesFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _componentDependenciesFactory = componentDependenciesFactory;
        }

        IComponentDefinition IComponentBuilder.Component(IPackage package)
        {
            return new ComponentDefinition(_nameManager, _assetManager, _htmlHelper, _componentDependenciesFactory, package);
        }
    }
}
