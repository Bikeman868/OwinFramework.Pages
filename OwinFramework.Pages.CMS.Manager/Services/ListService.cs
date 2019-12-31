using System.Linq;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.CMS.Manager.Services
{
    /// <summary>
    /// Provides endpoints to return lists of records and for changing relationships
    /// between records
    /// </summary>
    internal class ListService
    {
        private readonly IDataLayer _dataLayer;
        private readonly ISegmentTestingFramework _segmentTestingFramework;

        public ListService(
            IDataLayer dataLater,
            ISegmentTestingFramework segmentTestingFramework)
        {
            _dataLayer = dataLater;
            _segmentTestingFramework = segmentTestingFramework;
        }

        #region Environments

        [Endpoint(UrlPath = "environments", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void Environments(IEndpointRequest request)
        {
            var records = _dataLayer.GetEnvironments(e => e);
            request.Success(records);
        }

        #endregion

        #region Website versions

        [Endpoint(UrlPath = "websiteversions", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void WebsiteVersions(IEndpointRequest request)
        {
            var records = _dataLayer.GetWebsiteVersions(wvp => wvp);
            request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{id}/pages", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the website version to get pages for")]
        [EndpointParameter(
            "scenario", 
            typeof(OptionalString), 
            Description = "The name of the segmentation scenario to get a page version for")]
        private void WebsiteVersionPages(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var records = _dataLayer.GetWebsitePages(id, scenarioName, wvp => wvp);

            if (records == null)
                request.NoContent("There are no pages in website version #" + id + 
                    (string.IsNullOrEmpty(scenarioName) ? "" : " for '" + scenarioName + "' test scenario"));
            else
                request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{websiteVersionId}/page/{pageId}", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "websiteVersionId", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the website version to get information for")]
        [EndpointParameter(
            "pageId", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the page to get information for")]
        [EndpointParameter(
            "scenario", 
            typeof(OptionalString), 
            Description = "The name of the segmentation scenario to get a page version for")]
        private void WebsitePageVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var pageId = request.Parameter<long>("pageId");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var pageVersions = _dataLayer.GetWebsitePages(websiteVersionId, scenarioName, pv => pv, pv => pv.PageId == pageId);
            if (pageVersions == null || pageVersions.Length == 0)
                request.NoContent(
                    "There is no version of page #" + pageId + 
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " in the '" + scenarioName + "' test scenario"));
            else
                request.Success(pageVersions[0]);
        }

        [Endpoint(UrlPath = "websiteversion/{id}/layouts", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get layouts for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a layout version for")]
        private void WebsiteVersionLayouts(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var records = _dataLayer.GetWebsiteLayouts(id, scenarioName, wvp => wvp);

            if (records == null)
                request.NoContent("There are no layouts in website version #" + id +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " for '" + scenarioName + "' test scenario"));
            else
                request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{websiteVersionId}/layout/{layoutId}", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get information for")]
        [EndpointParameter(
            "layoutId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the layout to get information for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a layout version for")]
        private void WebsiteLayoutVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var layoutId = request.Parameter<long>("layoutId");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var layoutVersions = _dataLayer.GetWebsiteLayouts(websiteVersionId, scenarioName, pv => pv, pv => pv.LayoutId == layoutId);
            if (layoutVersions == null || layoutVersions.Length == 0)
                request.NoContent(
                    "There is no version of layout #" + layoutId +
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " in the '" + scenarioName + "' test scenario"));
            else
                request.Success(layoutVersions[0]);
        }

        [Endpoint(UrlPath = "websiteversion/{id}/regions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get regions for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a region version for")]
        private void WebsiteVersionRegions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var records = _dataLayer.GetWebsiteRegions(id, scenarioName, wvp => wvp);

            if (records == null)
                request.NoContent("There are no regions in website version #" + id +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " for '" + scenarioName + "' test scenario"));
            else
                request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{websiteVersionId}/region/{regionId}", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get information for")]
        [EndpointParameter(
            "regionId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the region to get information for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a region version for")]
        private void WebsiteRegionVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var regionId = request.Parameter<long>("regionId");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var regionVersions = _dataLayer.GetWebsiteRegions(websiteVersionId, scenarioName, pv => pv, pv => pv.RegionId == regionId);
            if (regionVersions == null || regionVersions.Length == 0)
                request.NoContent(
                    "There is no version of region #" + regionId +
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " in the '" + scenarioName + "' test scenario"));
            else
                request.Success(regionVersions[0]);
        }

        [Endpoint(UrlPath = "websiteversion/{id}/components", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get components for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a component version for")]
        private void WebsiteVersionComponents(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var records = _dataLayer.GetWebsiteComponents(id, scenarioName, wvp => wvp);

            if (records == null)
                request.NoContent("There are no components in website version #" + id +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " for '" + scenarioName + "' test scenario"));
            else
                request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{websiteVersionId}/component/{componentId}", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get information for")]
        [EndpointParameter(
            "componentId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the component to get information for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a component version for")]
        private void WebsiteComponentVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var componentId = request.Parameter<long>("componentId");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var componentVersions = _dataLayer.GetWebsiteComponents(websiteVersionId, scenarioName, pv => pv, pv => pv.ComponentId == componentId);
            if (componentVersions == null || componentVersions.Length == 0)
                request.NoContent(
                    "There is no version of component #" + componentId +
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " in the '" + scenarioName + "' test scenario"));
            else
                request.Success(componentVersions[0]);
        }

        [Endpoint(UrlPath = "websiteversion/{id}/datatypes", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get data types for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a data type version for")]
        private void WebsiteVersionDataTypes(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var records = _dataLayer.GetWebsiteDataTypes(id, scenarioName, wvp => wvp);

            if (records == null)
                request.NoContent("There are no data types in website version #" + id +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " for '" + scenarioName + "' test scenario"));
            else
                request.Success(records);
        }

        [Endpoint(UrlPath = "websiteversion/{websiteVersionId}/datatype/{dataTypeId}", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the website version to get information for")]
        [EndpointParameter(
            "dataTypeId",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the data type to get information for")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "The name of the segmentation scenario to get a data type version for")]
        private void WebsiteDataTypeVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var dataTypeId = request.Parameter<long>("dataTypeId");
            var scenarioName = request.Parameter<string>("scenario") ?? string.Empty;

            var dataTypeVersions = _dataLayer.GetWebsiteDataTypes(websiteVersionId, scenarioName, pv => pv, pv => pv.DataTypeId == dataTypeId);
            if (dataTypeVersions == null || dataTypeVersions.Length == 0)
                request.NoContent(
                    "There is no version of data type #" + dataTypeId +
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(scenarioName) ? "" : " in the '" + scenarioName + "' test scenario"));
            else
                request.Success(dataTypeVersions[0]);
        }

        #endregion

        #region User segments

        [Endpoint(UrlPath = "usersegments", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void UserSegments(IEndpointRequest request)
        {
            var segments = _segmentTestingFramework.GetSegments();
            if (segments == null)
            {
                request.Success(new object[0]);
            }
            else
            {
                request.Success(segments.Select(s => new
                {
                    key = s.Key,
                    name = s.Name,
                    description = s.Description
                }));
            }
        }

        #endregion

        #region Pages

        [Endpoint(UrlPath = "page/{id}/versions", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment,
            Description = "The ID of the page to get a list of versions for")]
        private void PageVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var pageVersions = _dataLayer.GetElementVersions(id, p => p as PageVersionRecord);

            if (pageVersions == null)
                request.NotFound("No page with ID " + id);
            else
                request.Success(pageVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "pages", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void AllPages(IEndpointRequest request)
        {
            var pages = _dataLayer.GetPages(p => p);
            request.Success(pages);
        }

        #endregion

        #region Layouts

        [Endpoint(UrlPath = "layout/{id}/versions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the layout to get a list of versions for")]
        private void LayoutVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var layoutVersions = _dataLayer.GetElementVersions(id, p => p as LayoutVersionRecord);

            if (layoutVersions == null)
                request.NotFound("No layout with ID " + id);
            else
                request.Success(layoutVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "layouts", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllLayouts(IEndpointRequest request)
        {
            var layouts = _dataLayer.GetLayouts(p => p);
            request.Success(layouts);
        }

        #endregion

        #region Regions

        [Endpoint(UrlPath = "region/{id}/versions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the region to get a list of versions for")]
        private void RegionVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var regionVersions = _dataLayer.GetElementVersions(id, p => p as RegionVersionRecord);

            if (regionVersions == null)
                request.NotFound("No region with ID " + id);
            else
                request.Success(regionVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "regions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllRegions(IEndpointRequest request)
        {
            var regions = _dataLayer.GetRegions(p => p);
            request.Success(regions);
        }

        #endregion

        #region Components

        [Endpoint(UrlPath = "component/{id}/versions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the component to get a list of versions for")]
        private void ComponentVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var componentVersions = _dataLayer.GetElementVersions(id, p => p as ComponentVersionRecord);

            if (componentVersions == null)
                request.NotFound("No component with ID " + id);
            else
                request.Success(componentVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "components", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllComponents(IEndpointRequest request)
        {
            var components = _dataLayer.GetComponents(p => p);
            request.Success(components);
        }

        #endregion

        #region Modules

        [Endpoint(UrlPath = "module/{id}/versions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the module to get a list of versions for")]
        private void ModuleVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var moduleVersions = _dataLayer.GetElementVersions(id, p => p as ModuleVersionRecord);

            if (moduleVersions == null)
                request.NotFound("No module with ID " + id);
            else
                request.Success(moduleVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "modules", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllModules(IEndpointRequest request)
        {
            var modules = _dataLayer.GetModules(p => p);
            request.Success(modules);
        }

        #endregion

        #region DataScopes

        [Endpoint(UrlPath = "datascopes", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllDataScopes(IEndpointRequest request)
        {
            var dataScopes = _dataLayer.GetDataScopes(p => p);
            request.Success(dataScopes);
        }

        #endregion

        #region DataTypes

        [Endpoint(UrlPath = "datatype/{id}/versions", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the data type to get a list of versions for")]
        private void DataTypeVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var dataTypeVersions = _dataLayer.GetElementVersions(id, p => p as DataTypeVersionRecord);

            if (dataTypeVersions == null)
                request.NotFound("No dataType with ID " + id);
            else
                request.Success(dataTypeVersions.Where(v => v != null));
        }

        [Endpoint(UrlPath = "datatypes", Methods = new[] { Method.Get }, RequiredPermission = Permissions.View)]
        private void AllDataTypes(IEndpointRequest request)
        {
            var dataTypes = _dataLayer.GetDataTypes(p => p);
            request.Success(dataTypes);
        }

        #endregion
    }
}
