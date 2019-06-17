using System.Collections.Generic;
using OwinFramework.Pages.CMS.Editor.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using PropertyChange = OwinFramework.Pages.CMS.Runtime.Interfaces.Database.PropertyChange;

namespace OwinFramework.Pages.CMS.Editor.Services
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

        [Endpoint(UrlPath = "pages/all", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        private void AllPages(IEndpointRequest request)
        {
            var page = request.Body<PageRecord>();
            var result = _dataLayer.CreatePage(page);

            if (result.Success)
            {
                page = _dataLayer.GetPage(result.NewRecordId, (p, v) => p);
                request.Success(page);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(UrlPath = "page/{pageid}/versions", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter("pageId", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void PageVersions(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var page = _dataLayer.GetPage(id, (p, v) => p);

            if (page == null)
                request.NotFound("No page with ID " + id);
            else
                request.Success(page);
        }

    }
}
