﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
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

        #region Environments

        [Endpoint(Methods = new[] {Method.Post}, RequiredPermission = Permissions.EditEnvironment)]
        private void CreateEnvironment(IEndpointRequest request)
        {
            var environment = request.Body<EnvironmentRecord>();
            var result = _dataLayer.CreateEnvironment(request.Identity, environment);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            environment = _dataLayer.GetEnvironment(result.NewRecordId, e => e);
            if (environment == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError, 
                    "After creating the new environment it could not be found in the database");
                return;
            }

            request.Success(environment);
        }

        [Endpoint(UrlPath = "environment/{id}", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void RetrieveEnvironment(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var environment = _dataLayer.GetEnvironment(id, e => e);

            if (environment == null)
                request.NotFound("No environment with ID " + id);
            else
                request.Success(environment);
        }

        [Endpoint(UrlPath = "environment/{id}", Methods = new[] {Method.Put}, RequiredPermission = Permissions.EditEnvironment)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void UpdateEnvironment(IEndpointRequest request)
        {
            var environmentId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            if (changes.Any(c => c.PropertyName == "websiteVersionId"))
            {
                var authorization = request.OwinContext.GetFeature<IAuthorization>();
                if (authorization != null)
                {
                    if (!authorization.HasPermission(Permissions.ChangeEnvironmentVersion, environmentId.ToString()))
                        request.HttpStatus(HttpStatusCode.Forbidden, 
                            "You do not have permission '" + Permissions.ChangeEnvironmentVersion + 
                            " on environment with id=" + environmentId);
                    return;
                }
            }

            var result = _dataLayer.UpdateEnvironment(request.Identity, environmentId, changes);

            if (result.Success)
            {
                var environment = _dataLayer.GetEnvironment(environmentId, e => e);
                request.Success(environment);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(UrlPath = "environment/{id}", Methods = new[] {Method.Delete}, RequiredPermission = Permissions.EditEnvironment)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void DeleteEnvironment(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeleteEnvironment(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region WebsiteVersions

        [Endpoint(Methods = new[] {Method.Post}, RequiredPermission = Permissions.EditWebsiteVersion)]
        private void CreateWebsiteVersion(IEndpointRequest request)
        {
            var websiteVersion = request.Body<WebsiteVersionRecord>();
            var result = _dataLayer.CreateWebsiteVersion(request.Identity, websiteVersion);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            websiteVersion = _dataLayer.GetWebsiteVersion(result.NewRecordId, e => e);
            if (websiteVersion == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError, 
                    "After creating the new website version it could not be found in the database");
                return;
            }

            request.Success(websiteVersion);
        }

        [Endpoint(UrlPath = "websiteversion/{id}", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void RetrieveWebsiteVersion(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var websiteVersion = _dataLayer.GetWebsiteVersion(id, e => e);

            if (websiteVersion == null)
                request.NotFound("No website version with ID " + id);
            else
                request.Success(websiteVersion);
        }

        [Endpoint(UrlPath = "websiteversion/{id}", Methods = new[] {Method.Put}, RequiredPermission = Permissions.EditWebsiteVersion)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void UpdateWebsiteVersion(IEndpointRequest request)
        {
            var websiteVersionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateWebsiteVersion(request.Identity, websiteVersionId, changes);

            if (result.Success)
            {
                var websiteVersion = _dataLayer.GetWebsiteVersion(websiteVersionId, e => e);
                request.Success(websiteVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(UrlPath = "websiteversion/{id}", Methods = new[] {Method.Delete}, RequiredPermission = Permissions.EditWebsiteVersion)]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void DeleteWebsiteVersion(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeleteWebsiteVersion(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Pages

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

        #endregion
    }
}
