using OwinFramework.Pages.CMS.Runtime.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.CMS.Runtime
{
    public class CmsRuntimePackage : IPackage
    {
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string NamespaceName { get; set; }
        public string Name { get; set; }

        private readonly IPackageDependenciesFactory _dependencies;
        private readonly IDatabaseReader _database;
        private readonly IDisposable _config;
        private CmsConfiguration _configuration;

        public CmsRuntimePackage(
            IPackageDependenciesFactory dependencies,
            IConfigurationStore configurationStore,
            IDatabaseReader databaseReader)
        {
            _dependencies = dependencies;
            _database = databaseReader;
            _config = configurationStore.Register("/owinFramework/pages/cms", ConfigurationChanged, new CmsConfiguration());
        }

        private void ConfigurationChanged(CmsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IPackage Build(IFluentBuilder builder)
        {
            BuildRegions(builder);
            BuildLayouts(builder);
            BuildPages(builder);

            return this;
        }

        private void BuildRegions(IFluentBuilder builder)
        {
        }

        private void BuildLayouts(IFluentBuilder builder)
        {
            var records = _database.GetLayouts(
                _configuration.VersionName, 
                (l, v) => new Tuple<LayoutRecord, LayoutVersionRecord>(l, v), 
                (l, v) => v.Enabled);

            foreach(var record in records)
            {
                var layout = record.Item1;
                var layoutVersion = record.Item2;

                var layoutDefinition = builder.BuildUpLayout()
                    .Name(layout.Name)
                    .AssetDeployment(layoutVersion.AssetDeployment)
                    .PartOf(layoutVersion.PackageName)
                    .DeployIn(layoutVersion.ModuleName)
                    .RegionNesting(layoutVersion.RegionNesting);

                if (layoutVersion.Components != null)
                    foreach (var component in layoutVersion.Components)
                        layoutDefinition.NeedsComponent(component.ComponentName);

                if (layoutVersion.LayoutRegions != null)
                { 
                    foreach(var layoutRegion in layoutVersion.LayoutRegions)
                    {
                        if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                            layoutDefinition.Html(layoutRegion.RegionName, layoutRegion.ContentName, layoutRegion.ContentValue);
                        else if (string.Equals(layoutRegion.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                            layoutDefinition.Layout(layoutRegion.RegionName, layoutRegion.ContentName);
                        else if (string.Equals(layoutRegion.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                            layoutDefinition.Template(layoutRegion.RegionName, layoutRegion.ContentName);
                        else if (string.Equals(layoutRegion.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                            layoutDefinition.Component(layoutRegion.RegionName, layoutRegion.ContentName);
                    }
                }

                layoutDefinition.Build();
            }
        }

        private void BuildPages(IFluentBuilder builder)
        {
            var records = _database.GetPages(
                _configuration.VersionName, 
                (p, v) => new Tuple<PageRecord, PageVersionRecord>(p, v), 
                (p, v) => v.Enabled);

            foreach(var record in records)
            {
                var page = record.Item1;
                var pageVersion = record.Item2;

                var pageDefinition = builder.BuildUpPage()
                    .Name(page.Name)
                    .AssetDeployment(pageVersion.AssetDeployment)
                    .PartOf(pageVersion.PackageName)
                    .DeployIn(pageVersion.ModuleName)
                    .Title(pageVersion.Title)
                    .CanonicalUrl(pageVersion.CanonicalUrl)
                    .Layout(pageVersion.LayoutName)
                    .BodyStyle(pageVersion.BodyStyle)
                    .RequiresPermission(pageVersion.RequiredPermission, pageVersion.AssetPath);

                if (pageVersion.Routes != null)
                    foreach (var route in pageVersion.Routes)
                        pageDefinition.Route(route.Path, route.Priority);

                if (pageVersion.Components != null)
                    foreach (var component in pageVersion.Components)
                        pageDefinition.NeedsComponent(component.ComponentName);

                if (pageVersion.LayoutRegions != null)
                { 
                    foreach(var layoutRegion in pageVersion.LayoutRegions)
                    {
                        if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                            pageDefinition.RegionHtml(layoutRegion.RegionName, layoutRegion.ContentName, layoutRegion.ContentValue);
                        else if (string.Equals(layoutRegion.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                            pageDefinition.RegionLayout(layoutRegion.RegionName, layoutRegion.ContentName);
                        else if (string.Equals(layoutRegion.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                            pageDefinition.RegionTemplate(layoutRegion.RegionName, layoutRegion.ContentName);
                        else if (string.Equals(layoutRegion.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                            pageDefinition.RegionComponent(layoutRegion.RegionName, layoutRegion.ContentName);
                    }
                }

                pageDefinition.Build();
            }
        }
    }
}
