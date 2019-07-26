using System.Linq;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
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
        private readonly IUserSegmenter _userSegmenter;

        public ListService(
            IDataLayer dataLater,
            IUserSegmenter userSegmenter)
        {
            _dataLayer = dataLater;
            _userSegmenter = userSegmenter;
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
            "segment", 
            typeof(OptionalString), 
            Description = "The user segment to get pages for")]
        private void WebsiteVersionPages(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var segment = request.Parameter<string>("segment") ?? string.Empty;

            var records = _dataLayer.GetWebsitePages(id, segment, wvp => wvp);

            if (records == null)
                request.NotFound("There are no pages in website version #" + id + 
                    (string.IsNullOrEmpty(segment) ? "" : " for '" + segment + "' users"));
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
            "segment", 
            typeof(OptionalString), 
            Description = "The user segment to get a page version for")]
        private void WebsitePageVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("websiteVersionId");
            var pageId = request.Parameter<long>("pageId");
            var segment = request.Parameter<string>("segment") ?? string.Empty;

            var pageVersions = _dataLayer.GetWebsitePages(websiteVersionId, segment, pv => pv, pv => pv.PageId == pageId);
            if (pageVersions == null || pageVersions.Length == 0)
                request.NotFound(
                    "There is no version of page #" + pageId + 
                    " in version #" + websiteVersionId + " of the website" +
                    (string.IsNullOrEmpty(segment) ? "" : " for '" + segment + "' users"));
            else
                request.Success(pageVersions[0]);
        }

        #endregion

        #region User segments

        [Endpoint(UrlPath = "usersegments", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void UserSegments(IEndpointRequest request)
        {
            var segments = _userSegmenter
                .GetSegments()
                .Select(s => new
                {
                    key = s.Key,
                    name = s.Name,
                    description = s.Description
                });
            request.Success(segments);
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

    }
}
