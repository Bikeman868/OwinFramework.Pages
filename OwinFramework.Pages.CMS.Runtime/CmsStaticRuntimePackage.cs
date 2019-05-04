using OwinFramework.Pages.CMS.Runtime.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.CMS.Runtime
{
    /// <summary>
    /// This CMS runtime package configures CMS pages into the Pages
    /// middleware at startup and does not change the configuration whilst
    /// the website is running. This is a very efficient and scaleable way
    /// of running a CMS website, but any changes to website pages will
    /// not be reflected on the website until the webservers are recylced.
    /// </summary>
    public class CmsStaticRuntimePackage : IPackage
    {
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string NamespaceName { get; set; }
        public string Name { get; set; }

        private readonly IPackageDependenciesFactory _dependencies;
        private readonly IDatabaseReader _database;

        private Dictionary<long, ILayout> _layoutVersions;
        private Dictionary<long, IRegion> _regionVersions;

        private readonly IDisposable _config;
        private CmsConfiguration _configuration;

        public CmsStaticRuntimePackage(
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
            _layoutVersions = new Dictionary<long, ILayout>();
            _regionVersions = new Dictionary<long, IRegion>();

            var websiteVersions = _database.GetWebsiteVersions(
                v => v, 
                v => string.Equals(v.Name, _configuration.WebsiteVersionName, StringComparison.OrdinalIgnoreCase));
            
            if (websiteVersions.Count != 1)
                throw new Exception("There is no website version in the database '" + _configuration.WebsiteVersionName + "'");

            var websiteVersion = websiteVersions[0];
            var websiteVersionPages = _database.GetWebsiteVersionPages(websiteVersion.Id, vp => vp);

            foreach (var page in websiteVersionPages)
                BuildPage(builder, websiteVersion, page.PageVersionId);

            _layoutVersions.Clear();
            _regionVersions.Clear();

            return this;
        }

        private ILayout GetLayoutVersion(IFluentBuilder builder, long layoutVersionId)
        {
            ILayout layout;
            if (_layoutVersions.TryGetValue(layoutVersionId, out layout))
                return layout;

            layout = BuildLayout(builder, layoutVersionId);
            _layoutVersions.Add(layoutVersionId, layout);
            return layout;
        }

        private IRegion GetRegionVersion(IFluentBuilder builder, long regionVersionId)
        {
            IRegion region;
            if (_regionVersions.TryGetValue(regionVersionId, out region))
                return region;

            region = BuildRegion(builder, regionVersionId);
            _regionVersions.Add(regionVersionId, region);
            return region;
        }

        private IPage BuildPage(
            IFluentBuilder builder, 
            WebsiteVersionRecord websiteVersion, 
            long pageVersionId)
        {
            var data = _database.GetPage(pageVersionId, (p, v) => new Tuple<PageRecord, PageVersionRecord>(p, v));
            var page = data.Item1;
            var pageVersion = data.Item2;

            pageVersion.VersionName = page.Name + "_v" + pageVersion.Version;

            var pageUrl = pageVersion.CanonicalUrl;
            if (string.IsNullOrEmpty(pageUrl) && pageVersion.Routes != null && pageVersion.Routes.Count > 0)
                pageUrl = pageVersion.Routes.OrderByDescending(r => r.Priority).First().Path;

            string canonicalUrl = null;
            if (!string.IsNullOrEmpty(websiteVersion.BaseUrl) && !string.IsNullOrEmpty(pageUrl))
            {
                canonicalUrl = websiteVersion.BaseUrl;
                if (canonicalUrl.EndsWith("/"))
                {
                    if (pageUrl.StartsWith("/"))
                        canonicalUrl += pageUrl.Substring(1);
                    else
                        canonicalUrl += pageUrl;
                }
                else
                {
                    if (pageUrl.StartsWith("/"))
                        canonicalUrl += pageUrl;
                    else
                        canonicalUrl += "/" + pageUrl;
                }
            }

            var pageDefinition = builder.BuildUpPage()
                .Name(pageVersion.VersionName)
                .AssetDeployment(pageVersion.AssetDeployment)
                .PartOf(pageVersion.PackageName)
                .DeployIn(pageVersion.ModuleName)
                .Title(pageVersion.Title)
                .CanonicalUrl(canonicalUrl)
                .BodyStyle(pageVersion.BodyStyle)
                .RequiresPermission(pageVersion.RequiredPermission, pageVersion.AssetPath);

            if (pageVersion.LayoutVersionId.HasValue)
                pageDefinition.Layout(GetLayoutVersion(builder, pageVersion.LayoutVersionId.Value));
            else
                pageDefinition.Layout(pageVersion.LayoutName);

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
                    if (layoutRegion.RegionVersionId.HasValue)
                        layoutDefinition.Region(layoutRegion.RegionName, GetRegionVersion(builder, layoutRegion.RegionVersionId.Value));
                    else if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
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

        private IRegion BuildRegion(IFluentBuilder builder, long regionVersionId)
        {
            var data = _database.GetRegion(regionVersionId, (l, v) => new Tuple<RegionRecord, RegionVersionRecord>(l, v));
            var region = data.Item1;
            var regionVersion = data.Item2;

            regionVersion.VersionName = region.Name + "_v" + regionVersion.Version;

            var regionDefinition = builder.BuildUpRegion()
                .Name(regionVersion.VersionName)
                .AssetDeployment(regionVersion.AssetDeployment)
                .PartOf(regionVersion.PackageName)
                .DeployIn(regionVersion.ModuleName);

            if (regionVersion.Components != null)
                foreach (var component in regionVersion.Components)
                    regionDefinition.NeedsComponent(component.ComponentName);

            var hasLayout = false;

            if (regionVersion.LayoutVersionId.HasValue)
            {
                regionDefinition.Layout(GetLayoutVersion(builder, regionVersion.LayoutVersionId.Value));
                hasLayout = true;
            }
            else if (!string.IsNullOrEmpty(regionVersion.LayoutName))
            {
                regionDefinition.Layout(regionVersion.LayoutName);
                hasLayout = true;
            }
            else if (!string.IsNullOrEmpty(regionVersion.AssetName))
                regionDefinition.Html(regionVersion.AssetName, regionVersion.AssetValue);
            else if (!string.IsNullOrEmpty(regionVersion.ComponentName))
                regionDefinition.Component(regionVersion.ComponentName);
            else if (regionVersion.RegionTemplates != null)
            {
                foreach (var template in regionVersion.RegionTemplates)
                    regionDefinition.AddTemplate(template.TemplatePath, template.PageArea);
            }

            if (hasLayout && regionVersion.LayoutRegions != null)
            { 
                foreach(var layoutRegion in regionVersion.LayoutRegions)
                {
                    if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.LayoutRegionHtml(layoutRegion.RegionName, layoutRegion.ContentName, layoutRegion.ContentValue);
                    else if (string.Equals(layoutRegion.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.LayoutRegionLayout(layoutRegion.RegionName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.LayoutRegionTemplate(layoutRegion.RegionName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.LayoutRegionComponent(layoutRegion.RegionName, layoutRegion.ContentName);
                }
            }

            return regionDefinition.Build();
        }
    }
}
