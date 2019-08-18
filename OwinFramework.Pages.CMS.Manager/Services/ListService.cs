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
            var page = _dataLayer.GetPageVersion(id, (p, v) => p);

            if (page == null)
                request.NotFound("No page with ID " + id);
            else
                request.Success(page);
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
            var layout = _dataLayer.GetLayoutVersion(id, (p, v) => p);

            if (layout == null)
                request.NotFound("No layout with ID " + id);
            else
                request.Success(layout);
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
            var region = _dataLayer.GetRegionVersion(id, (p, v) => p);

            if (region == null)
                request.NotFound("No region with ID " + id);
            else
                request.Success(region);
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
