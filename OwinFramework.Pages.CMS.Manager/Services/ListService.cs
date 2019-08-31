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
    }
}
