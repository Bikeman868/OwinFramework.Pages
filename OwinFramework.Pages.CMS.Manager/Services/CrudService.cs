using System.Collections.Generic;
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
        private void CreatePage(IEndpointRequest request)
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

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void RetrievePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var page = _dataLayer.GetPage(id, (p, v) => p);

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

            var result = _dataLayer.UpdatePage(pageId, changes);

            if (result.Success)
            {
                var page = _dataLayer.GetPage(pageId, (p, v) => p);
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
            var result = _dataLayer.DeletePage(id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

    }
}
