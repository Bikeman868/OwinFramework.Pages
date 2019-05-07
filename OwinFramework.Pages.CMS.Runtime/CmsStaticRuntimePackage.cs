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

        private Dictionary<long, long> _pageVersions;
        private Dictionary<long, long> _layoutVersions;
        private Dictionary<long, long> _regionVersions;
        private Dictionary<long, PageVersionRecord> _masterPages;
        private Dictionary<long, ILayout> _layouts;
        private Dictionary<long, IRegion> _regions;
        private Dictionary<long, DataScopeRecord> _dataScopes;
        private Dictionary<long, DataTypeVersionRecord> _dataTypes;

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
            _masterPages = new Dictionary<long, PageVersionRecord>();
            _layouts = new Dictionary<long, ILayout>();
            _regions = new Dictionary<long, IRegion>();
            _dataScopes = new Dictionary<long, DataScopeRecord>();

            var websiteVersions = _database.GetWebsiteVersions(
                v => v, 
                v => string.Equals(v.Name, _configuration.WebsiteVersionName, StringComparison.OrdinalIgnoreCase));

            if (websiteVersions.Length != 1)
                throw new Exception("There is no website version '" + _configuration.WebsiteVersionName + "' in the database");

            var websiteVersion = websiteVersions[0];
            var websiteVersionPages = _database.GetWebsitePages(websiteVersion.Id, vp => vp);

            _dataTypes = _database
                .GetWebsiteDataTypes(
                    websiteVersion.Id, 
                    wd => _database.GetDataType(wd.DataTypeVersionId, (dt, dtv) => dtv))
                .ToDictionary(dtv => dtv.DataTypeId, dtv => dtv);

            _layoutVersions = _database
                .GetWebsiteLayouts(websiteVersion.Id, v => new { v.LayoutId, v.LayoutVersionId})
                .ToDictionary(v => v.LayoutId, v => v.LayoutVersionId);

            _regionVersions = _database
                .GetWebsiteRegions(websiteVersion.Id, v => new { v.RegionId, v.RegionVersionId})
                .ToDictionary(v => v.RegionId, v => v.RegionVersionId);

            foreach (var regionId in _regionVersions.Keys.ToList())
                GetRegion(builder, regionId);

            foreach (var layoutId in _layoutVersions.Keys.ToList())
                GetLayout(builder, layoutId);

            foreach (var page in websiteVersionPages)
                GetPage(builder, websiteVersion, page.PageVersionId);

            _masterPages.Clear();
            _layouts.Clear();
            _regions.Clear();
            _dataScopes.Clear();

            return this;
        }

        private PageVersionRecord GetPage(
            IFluentBuilder builder, 
            WebsiteVersionRecord websiteVersion, 
            long pageVersionId)
        {
            PageVersionRecord pageRecord;
            if (_masterPages.TryGetValue(pageVersionId, out pageRecord))
                return pageRecord;

            var data = _database.GetPage(pageVersionId, (p, v) => new Tuple<PageRecord, PageVersionRecord>(p, v));
            var page = data.Item1;
            var pageVersion = data.Item2;

            pageVersion.VersionName = page.Name + "_v" + pageVersion.Version;

            if (pageVersion.Routes != null && pageVersion.Routes.Length > 0)
                BuildPage(builder, websiteVersion, pageVersion);

            _masterPages.Add(pageVersionId, pageVersion);
            return pageVersion;
        }

        private ILayout GetLayout(
            IFluentBuilder builder, 
            long layoutId)
        {
            ILayout layout;
            if (_layouts.TryGetValue(layoutId, out layout))
                return layout;

            layout = BuildLayout(builder, layoutId);
            _layouts.Add(layoutId, layout);
            return layout;
        }

        private IRegion GetRegion(
            IFluentBuilder builder, 
            long regionId)
        {
            IRegion region;
            if (_regions.TryGetValue(regionId, out region))
                return region;

            region = BuildRegion(builder, regionId);
            _regions.Add(regionId, region);
            return region;
        }

        private void BuildPage(
            IFluentBuilder builder, 
            WebsiteVersionRecord websiteVersion, 
            PageVersionRecord pageVersion)
        {
            if (pageVersion.MasterPageId.HasValue)
            {
                var masterPageRecord = GetPage(builder, websiteVersion, pageVersion.MasterPageId.Value);

                if (string.IsNullOrEmpty(pageVersion.RequiredPermission))
                    pageVersion.RequiredPermission = masterPageRecord.RequiredPermission;

                if (string.IsNullOrEmpty(pageVersion.AssetPath))
                    pageVersion.AssetPath = masterPageRecord.AssetPath;

                if (string.IsNullOrEmpty(pageVersion.BodyStyle))
                    pageVersion.BodyStyle = masterPageRecord.BodyStyle;

                if (string.IsNullOrEmpty(pageVersion.ModuleName))
                    pageVersion.ModuleName = masterPageRecord.ModuleName;

                if (!pageVersion.LayoutId.HasValue && string.IsNullOrEmpty(pageVersion.LayoutName))
                {
                    pageVersion.LayoutId = masterPageRecord.LayoutId;
                    pageVersion.LayoutName = masterPageRecord.LayoutName;
                }

                if (pageVersion.LayoutRegions == null || pageVersion.LayoutRegions.Length == 0)
                    pageVersion.LayoutRegions = masterPageRecord.LayoutRegions;
                else if (masterPageRecord.LayoutRegions != null && masterPageRecord.LayoutRegions.Length > 0)
                {
                    foreach (var layoutRegion in masterPageRecord.LayoutRegions)
                    {
                        if (pageVersion.LayoutRegions.FirstOrDefault(
                                lr => string.Equals(layoutRegion.RegionName, lr.RegionName, StringComparison.OrdinalIgnoreCase)) == null)
                            pageVersion.LayoutRegions = pageVersion.LayoutRegions
                                .Concat(Enumerable.Repeat(layoutRegion, 1))
                                .ToArray();
                    }
                }

                if (pageVersion.Components == null || pageVersion.Components.Length == 0)
                    pageVersion.Components = masterPageRecord.Components;
                else if (masterPageRecord.Components != null && masterPageRecord.Components.Length > 0)
                {
                    foreach (var component in masterPageRecord.Components)
                    {
                        if (pageVersion.Components.FirstOrDefault(
                                c => string.Equals(component.ComponentName, c.ComponentName, StringComparison.OrdinalIgnoreCase)) == null)
                            pageVersion.Components = pageVersion.Components
                                .Concat(Enumerable.Repeat(component, 1))
                                .ToArray();
                    }
                }
            }

            var pageUrl = pageVersion.CanonicalUrl;
            if (string.IsNullOrEmpty(pageUrl) && pageVersion.Routes != null && pageVersion.Routes.Length > 0)
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
                .DeployIn(pageVersion.ModuleName)
                .Title(pageVersion.Title)
                .CanonicalUrl(canonicalUrl)
                .BodyStyle(pageVersion.BodyStyle)
                .RequiresPermission(pageVersion.RequiredPermission, pageVersion.AssetPath);

            if (pageVersion.LayoutId.HasValue)
                pageDefinition.Layout(GetLayout(builder, pageVersion.LayoutId.Value));
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
                    if (layoutRegion.RegionId.HasValue)
                        pageDefinition.RegionRegion(layoutRegion.RegionName, GetRegion(builder, layoutRegion.RegionId.Value));
                    else if (layoutRegion.LayoutId.HasValue)
                        pageDefinition.RegionLayout(layoutRegion.RegionName, GetLayout(builder, layoutRegion.LayoutId.Value));
                    else if (string.Equals(layoutRegion.ContentType, "region", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.RegionRegion(layoutRegion.RegionName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
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

        private ILayout BuildLayout(
            IFluentBuilder builder, 
            long layoutId)
        {
            long layoutVersionId;
            if (!_layoutVersions.TryGetValue(layoutId, out layoutVersionId))
                throw new Exception("The website version does not define which version of layout ID " + layoutId + " to use");

            var data = _database.GetLayout(layoutVersionId, (l, v) => new Tuple<LayoutRecord, LayoutVersionRecord>(l, v));
            var layout = data.Item1;
            var layoutVersion = data.Item2;

            layoutVersion.VersionName = layout.Name + "_v" + layoutVersion.Version;

            var layoutDefinition = builder.BuildUpLayout()
                .Name(layoutVersion.VersionName)
                .AssetDeployment(layoutVersion.AssetDeployment)
                .DeployIn(layoutVersion.ModuleName)
                .RegionNesting(layoutVersion.RegionNesting);

            if (layoutVersion.Components != null)
                foreach (var component in layoutVersion.Components)
                    layoutDefinition.NeedsComponent(component.ComponentName);

            if (layoutVersion.LayoutRegions != null)
            { 
                foreach(var layoutRegion in layoutVersion.LayoutRegions)
                {
                    if (layoutRegion.RegionId.HasValue)
                        layoutDefinition.Region(layoutRegion.RegionName, GetRegion(builder, layoutRegion.RegionId.Value));
                    else if (layoutRegion.LayoutId.HasValue)
                        layoutDefinition.Layout(layoutRegion.RegionName, GetLayout(builder, layoutRegion.LayoutId.Value));                        
                    else if (string.Equals(layoutRegion.ContentType, "region", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Region(layoutRegion.RegionName, layoutRegion.ContentName);
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

        private IRegion BuildRegion(
            IFluentBuilder builder, 
            long regionId)
        {
            long regionVersionId;
            if (!_regionVersions.TryGetValue(regionId, out regionVersionId))
                throw new Exception("The website version does not define which version of region ID " + regionId + " to use");

            var data = _database.GetRegion(regionVersionId, (l, v) => new Tuple<RegionRecord, RegionVersionRecord>(l, v));
            var region = data.Item1;
            var regionVersion = data.Item2;

            regionVersion.VersionName = region.Name + "_v" + regionVersion.Version;

            var regionDefinition = builder.BuildUpRegion()
                .Name(regionVersion.VersionName)
                .AssetDeployment(regionVersion.AssetDeployment)
                .DeployIn(regionVersion.ModuleName);

            if (regionVersion.Components != null)
                foreach (var component in regionVersion.Components)
                    regionDefinition.NeedsComponent(component.ComponentName);

            var hasLayout = false;

            if (regionVersion.LayoutId.HasValue)
            {
                regionDefinition.Layout(GetLayout(builder, regionVersion.LayoutId.Value));
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

            if (regionVersion.DataScopes != null)
            {
                foreach (var dataScope in regionVersion.DataScopes)
                {
                    DataScopeRecord dataScopeRecord;
                    if (_dataScopes.TryGetValue(dataScope.DataScopeId, out dataScopeRecord))
                    {
                        if (dataScopeRecord.DataTypeId.HasValue)
                        {
                            DataTypeVersionRecord dataType;
                            if (!_dataTypes.TryGetValue(dataScopeRecord.DataTypeId.Value, out dataType))
                                throw new Exception("Region ID " + regionId + 
                                    " has an invalid data scope with ID " + dataScope.DataScopeId + 
                                    ". There is no data type ID " + dataScopeRecord.DataTypeId.Value +
                                    " in this version of the website");

                            regionDefinition.DataScope(dataType.Type, dataScopeRecord.Name);
                        }
                        else
                        {
                            regionDefinition.DataProvider(dataScopeRecord.Name);
                        }
                    }                    
                }
            }

            if (regionVersion.RepeatDataTypeId.HasValue)
            {
                DataTypeVersionRecord dataType;
                if (!_dataTypes.TryGetValue(regionVersion.RepeatDataTypeId.Value, out dataType))
                    throw new Exception("Region ID " + regionId + 
                        " has an invalid data repetition. There is no data type ID " + regionVersion.RepeatDataTypeId.Value +
                        " in this version of the website");

                var repeatScope = regionVersion.RepeatDataScopeName;
                var listScope = regionVersion.ListDataScopeName;

                if (regionVersion.RepeatDataScopeId.HasValue)
                {
                    DataScopeRecord repeatScopeRecord;
                    if (_dataScopes.TryGetValue(regionVersion.RepeatDataScopeId.Value, out repeatScopeRecord))
                        repeatScope = repeatScopeRecord.Name;
                }

                if (regionVersion.ListDataScopeId.HasValue)
                {
                    DataScopeRecord listScopeRecord;
                    if (_dataScopes.TryGetValue(regionVersion.ListDataScopeId.Value, out listScopeRecord))
                        listScope = listScopeRecord.Name;
                }

                var childClasses = string.IsNullOrEmpty(regionVersion.ListElementClasses)
                    ? new string[0]
                    : regionVersion.ListElementClasses.Split(',');
                
                regionDefinition.ForEach(
                    dataType.Type, 
                    repeatScope, 
                    regionVersion.ListElementTag,
                    regionVersion.ListElementStyle,
                    listScope,
                    childClasses);
            }

            if (hasLayout && regionVersion.LayoutRegions != null)
            { 
                foreach(var layoutRegion in regionVersion.LayoutRegions)
                {
                    if (layoutRegion.LayoutId.HasValue)
                        regionDefinition.Layout(GetLayout(builder, layoutRegion.LayoutId.Value));                        
                    else if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
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
