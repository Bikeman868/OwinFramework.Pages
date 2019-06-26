using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
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

        public ListService(IDataLayer dataLater)
        {
            _dataLayer = dataLater;
        }

        [Endpoint(UrlPath = "websiteversion/{id}/pages", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the website version to get pages for")]
        private void WebsiteVersionPages(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var records = _dataLayer.GetWebsitePages(id, wvp => wvp);
            request.Success(records);
        }

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

    }
}
