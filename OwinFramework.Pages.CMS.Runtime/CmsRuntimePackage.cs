using OwinFramework.Pages.CMS.Runtime.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using System;
using System.Collections.Generic;
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

        private Dictionary<long, ILayout> _layouts;

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
            _layouts = new Dictionary<long, ILayout>();

            var websitePages = _database.GetWebsiteVersionPages(_configuration.WebsiteVersionName, v => v);

            foreach (var page in websitePages)
            {
                BuildPage(builder, page.PageVersionId);
            }

            _layouts.Clear();

            return this;
        }

        private IPage BuildPage(IFluentBuilder builder, long pageVersionId)
        {
            var data = _database.GetPage(pageVersionId, (p, v) => new Tuple<PageRecord, PageVersionRecord>(p, v));
            var page = data.Item1;
            var pageVersion = data.Item2;

            pageVersion.VersionName = page.Name + "_v" + pageVersion.Version;

            var pageDefinition = builder.BuildUpPage()
                .Name(pageVersion.VersionName)
                .AssetDeployment(pageVersion.AssetDeployment)
                .PartOf(pageVersion.PackageName)
                .DeployIn(pageVersion.ModuleName)
                .Title(pageVersion.Title)
                .CanonicalUrl(pageVersion.CanonicalUrl)
                .BodyStyle(pageVersion.BodyStyle)
                .RequiresPermission(pageVersion.RequiredPermission, pageVersion.AssetPath);

            if (pageVersion.LayoutVersionId.HasValue)
            {
                ILayout layout;
                if (!_layouts.TryGetValue(pageVersion.LayoutVersionId.Value, out layout))
                {
                    layout = BuildLayout(builder, pageVersion.LayoutVersionId.Value);
                    _layouts.Add(pageVersion.LayoutVersionId.Value, layout);
                }

                pageDefinition.Layout(layout);
            }
            else
            {
                pageDefinition.Layout(pageVersion.LayoutName);
            }

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

            return pageDefinition.Build();
        }

        private ILayout BuildLayout(IFluentBuilder builder, long layoutVersionId)
        {
            var data = _database.GetLayout(layoutVersionId, (l, v) => new Tuple<LayoutRecord, LayoutVersionRecord>(l, v));
            var layout = data.Item1;
            var layoutVersion = data.Item2;

            layoutVersion.VersionName = layout.Name + "_v" + layoutVersion.Version;

            var layoutDefinition = builder.BuildUpLayout()
                .Name(layoutVersion.VersionName)
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

            return layoutDefinition.Build();
        }
    }
}
