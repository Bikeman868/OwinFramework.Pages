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
        private Dictionary<long, long> _componentVersions;
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

            Name = "cms";
            NamespaceName = "cms";
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

            var environments = _database.GetEnvironments(
                v => v,
                v => string.Equals(v.Name, _configuration.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (environments.Length != 1)
                throw new Exception("There is no environment '" + _configuration.EnvironmentName + "' in the database");

            var environment = environments[0];

            var websiteVersions = _database.GetWebsiteVersions(
                v => v, 
                v => v.WebsiteVersionId == environment.WebsiteVersionId);

            if (websiteVersions.Length != 1)
                throw new Exception("There is no website version ID " + environment.WebsiteVersionId + " in the database");

            var websiteVersion = websiteVersions[0];
            var websiteVersionPages = _database.GetWebsitePages(websiteVersion.WebsiteVersionId, vp => vp);

            _pageVersions = websiteVersionPages
                .ToDictionary(vp => vp.PageId, vp => vp.PageVersionId);

            _dataTypes = _database
                .GetWebsiteDataTypes(
                    websiteVersion.WebsiteVersionId, 
                    wd => _database.GetDataType(wd.DataTypeVersionId, (dt, dtv) => dtv))
                .ToDictionary(dtv => dtv.ElementId, dtv => dtv);

            _componentVersions = _database
                .GetWebsiteComponents(websiteVersion.WebsiteVersionId, v => new { v.ComponentId, v.ComponentVersionId})
                .ToDictionary(v => v.ComponentId, v => v.ComponentVersionId);

            _layoutVersions = _database
                .GetWebsiteLayouts(websiteVersion.WebsiteVersionId, v => new { v.LayoutId, v.LayoutVersionId})
                .ToDictionary(v => v.LayoutId, v => v.LayoutVersionId);

            _regionVersions = _database
                .GetWebsiteRegions(websiteVersion.WebsiteVersionId, v => new { v.RegionId, v.RegionVersionId})
                .ToDictionary(v => v.RegionId, v => v.RegionVersionId);

            foreach (var regionId in _regionVersions.Keys.ToList())
                GetRegion(builder, regionId);

            foreach (var layoutId in _layoutVersions.Keys.ToList())
                GetLayout(builder, layoutId);

            foreach (var page in websiteVersionPages)
                GetPage(builder, environment, page.PageVersionId);

            _masterPages.Clear();
            _layouts.Clear();
            _regions.Clear();
            _dataScopes.Clear();
            _componentVersions.Clear();

            return this;
        }

        private PageVersionRecord GetPage(
            IFluentBuilder builder, 
            EnvironmentRecord environment, 
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
                BuildPage(builder, environment, pageVersion);

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
            EnvironmentRecord environment, 
            PageVersionRecord pageVersion)
        {
            if (pageVersion.MasterPageId.HasValue)
            {
                long masterPageVersionId;
                if (!_pageVersions.TryGetValue(pageVersion.MasterPageId.Value, out masterPageVersionId))
                    throw new Exception("Page with version ID "+ pageVersion.ElementVersionId + 
                        " has a master page ID of " + pageVersion.MasterPageId.Value + 
                        " but there is no version of that page configured for this version of the website");

                var masterPageRecord = GetPage(builder, environment, masterPageVersionId);

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

                if (pageVersion.LayoutZones == null || pageVersion.LayoutZones.Length == 0)
                    pageVersion.LayoutZones = masterPageRecord.LayoutZones;
                else if (masterPageRecord.LayoutZones != null && masterPageRecord.LayoutZones.Length > 0)
                {
                    foreach (var layoutRegion in masterPageRecord.LayoutZones)
                    {
                        if (pageVersion.LayoutZones.FirstOrDefault(
                                lr => string.Equals(layoutRegion.ZoneName, lr.ZoneName, StringComparison.OrdinalIgnoreCase)) == null)
                            pageVersion.LayoutZones = pageVersion.LayoutZones
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
            if (!string.IsNullOrEmpty(environment.BaseUrl) && !string.IsNullOrEmpty(pageUrl))
            {
                canonicalUrl = environment.BaseUrl;
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

            if (pageVersion.LayoutZones != null)
            { 
                foreach(var layoutRegion in pageVersion.LayoutZones)
                {
                    if (layoutRegion.RegionId.HasValue)
                        pageDefinition.ZoneRegion(layoutRegion.ZoneName, GetRegion(builder, layoutRegion.RegionId.Value));
                    else if (layoutRegion.LayoutId.HasValue)
                        pageDefinition.ZoneLayout(layoutRegion.ZoneName, GetLayout(builder, layoutRegion.LayoutId.Value));
                    else if (string.Equals(layoutRegion.ContentType, "region", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.ZoneRegion(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.ZoneHtml(layoutRegion.ZoneName, layoutRegion.ContentName, layoutRegion.ContentValue);
                    else if (string.Equals(layoutRegion.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.ZoneLayout(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.ZoneTemplate(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                        pageDefinition.ZoneComponent(layoutRegion.ZoneName, layoutRegion.ContentName);
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
                .ZoneNesting(layoutVersion.ZoneNesting);

            if (layoutVersion.Components != null)
                foreach (var component in layoutVersion.Components)
                    layoutDefinition.NeedsComponent(component.ComponentName);

            if (layoutVersion.LayoutZones != null)
            { 
                foreach(var layoutRegion in layoutVersion.LayoutZones)
                {
                    if (layoutRegion.RegionId.HasValue)
                        layoutDefinition.Region(layoutRegion.ZoneName, GetRegion(builder, layoutRegion.RegionId.Value));
                    else if (layoutRegion.LayoutId.HasValue)
                        layoutDefinition.Layout(layoutRegion.ZoneName, GetLayout(builder, layoutRegion.LayoutId.Value));                        
                    else if (string.Equals(layoutRegion.ContentType, "region", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Region(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Html(layoutRegion.ZoneName, layoutRegion.ContentName, layoutRegion.ContentValue);
                    else if (string.Equals(layoutRegion.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Layout(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Template(layoutRegion.ZoneName, layoutRegion.ContentName);
                    else if (string.Equals(layoutRegion.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                        layoutDefinition.Component(layoutRegion.ZoneName, layoutRegion.ContentName);
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
            else if (regionVersion.ComponentId.HasValue)
            {
                long componentVersionId;
                if (!_componentVersions.TryGetValue(regionVersion.ComponentId.Value, out componentVersionId))
                    throw new Exception("zone version ID " + regionVersion.ElementVersionId + 
                        " references component ID " + regionVersion.ComponentId.Value + 
                        " but this component has no version defined for this version of the website");

                var componentRecord = _database.GetComponent(componentVersionId, (c, v) => c);
                var componentName = componentRecord.Name;

                var regionProperties = _database.GetElementPropertyValues(regionVersion.ElementVersionId);
                if (regionProperties != null && regionProperties.Count > 0)
                {
                    _dependencies.NameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        nm =>
                        {
                            var component = nm.ResolveComponent(componentName, this);
                            var cloneable = component as ICloneable;

                            if (cloneable != null)
                            {
                                component = (IComponent)cloneable.Clone();
                                var componentProperties = component.GetType().GetProperties();
                                foreach (var regionProperty in regionProperties)
                                {
                                    var componentProperty = componentProperties.FirstOrDefault(
                                        p => string.Equals(p.Name, regionProperty.Key, StringComparison.OrdinalIgnoreCase));
                                    if (componentProperty != null)
                                        componentProperty.SetValue(component, regionProperty.Value, null);
                                }
                            }
                            regionDefinition.Component(component);
                        });
                }
                else
                {
                    regionDefinition.Component(componentName);
                }
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
                                throw new Exception("zone ID " + regionId + 
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
                    throw new Exception("zone ID " + regionId + 
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

            if (hasLayout && regionVersion.LayoutZones != null)
            { 
                foreach(var zone in regionVersion.LayoutZones)
                {
                    if (zone.LayoutId.HasValue)
                        regionDefinition.Layout(GetLayout(builder, zone.LayoutId.Value));                        
                    else if (string.Equals(zone.ContentType, "html", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.ZoneHtml(zone.ZoneName, zone.ContentName, zone.ContentValue);
                    else if (string.Equals(zone.ContentType, "layout", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.ZoneLayout(zone.ZoneName, zone.ContentName);
                    else if (string.Equals(zone.ContentType, "template", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.ZoneTemplate(zone.ZoneName, zone.ContentName);
                    else if (string.Equals(zone.ContentType, "component", StringComparison.OrdinalIgnoreCase))
                        regionDefinition.ZoneComponent(zone.ZoneName, zone.ContentName);
                }
            }

            return regionDefinition.Build();
        }
    }
}
