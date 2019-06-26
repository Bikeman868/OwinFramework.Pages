using System.Collections.Generic;
using System.Net;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using PropertyChange = OwinFramework.Pages.CMS.Runtime.Interfaces.Database.PropertyChange;

namespace OwinFramework.Pages.CMS.Manager.Services
{
    /// <summary>
    /// Provides create, retrieve, update and delete methods foreach entity type
    /// </summary>
    internal class CrudService
    {
        private readonly IDataLayer _dataLayer;

        public CrudService(IDataLayer dataLater)
        {
            _dataLayer = dataLater;
        }

        [Endpoint(Methods = new[] {Method.Post}, RequiredPermission = Permissions.EditPage)]
        [EndpointParameter("websiteVersionId", typeof(PositiveNumber<long?>))]
        private void CreatePage(IEndpointRequest request)
        {
            var page = request.Body<PageRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");

            var result = _dataLayer.CreatePage(request.Identity, page);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            page = _dataLayer.GetPageVersion(result.NewRecordId, 1, (p, v) => p);
            if (page == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError, 
                    "After creating the new page it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddPageToWebsiteVersion(request.Identity, page.ElementId, 1, websiteVersionId.Value);
            }
            request.Success(page);
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void RetrievePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var page = _dataLayer.GetPageVersion(id, (p, v) => p);

            if (page == null)
                request.NotFound("No page with ID " + id);
            else
                request.Success(page);
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Put}, RequiredPermission = Permissions.EditPage)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void UpdatePage(IEndpointRequest request)
        {
            var pageId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdatePage(request.Identity, pageId, changes);

            if (result.Success)
            {
                var page = _dataLayer.GetPageVersion(pageId, (p, v) => p);
                request.Success(page);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Delete}, RequiredPermission = Permissions.EditPage)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void DeletePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeletePage(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

    }
}
