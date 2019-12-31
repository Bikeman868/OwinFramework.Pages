using System.Collections.Generic;
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

        [Endpoint(
            Methods = new[] {Method.Post}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "websiteVersionId", 
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new page to a specific version of the website")]
        [EndpointParameter(
            "scenario", 
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the page to use in this segmentation test scenario")]
        [Description("Creates a new page and optionally adds it to a specific version of the website")]
        private void CreatePage(IEndpointRequest request)
        {
            var page = request.Body<PageRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreatePage(request.Identity, page);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            page = _dataLayer.GetPage(result.NewRecordId, p => p);
            if (page == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError, 
                    "After creating the new page it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddPageToWebsiteVersion(request.Identity, page.RecordId, 1, websiteVersionId.Value, scenario);
            }
            request.Success(page);
        }

        [Endpoint(
            UrlPath = "page/{id}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
        private void RetrievePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var page = _dataLayer.GetPage(id, p => p);

            if (page == null)
                request.NotFound("No page with ID " + id);
            else
                request.Success(page);
        }

        [Endpoint(
            UrlPath = "page/{id}", 
            Methods = new[] {Method.Put},
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdatePage(IEndpointRequest request)
        {
            var pageId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdatePage(request.Identity, pageId, changes);

            if (result.Success)
            {
                var page = _dataLayer.GetPage(pageId, p => p);
                request.Success(page);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "page/{id}", 
            Methods = new[] {Method.Delete}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
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

        #region Page versions

        [Endpoint(
            Methods = new[] {Method.Post}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "websiteVersionId", 
            typeof(PositiveNumber<long?>), 
            Description = "Include the optional website version id to add the new page version to a specific version of the website")]
        [EndpointParameter(
            "scenario", 
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the page version to use in this segmentation test scenario")]
        [Description("Creates a new version of a page and optionally adds it to a specific version of the website")]
        private void CreatePageVersion(IEndpointRequest request)
        {
            var pageVersion = request.Body<PageVersionRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreatePageVersion(request.Identity, pageVersion);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            pageVersion = _dataLayer.GetPageVersion(result.NewRecordId, (p, v) => v);
            if (pageVersion == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError, 
                    "After creating the new page version it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddPageToWebsiteVersion(request.Identity, pageVersion.RecordId, websiteVersionId.Value, scenario);
            }

            request.Success(pageVersion);
        }

        [Endpoint(
            UrlPath = "pageversion/{id}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment,
            Description = "The ID of the page version to return")]
        [Description("Retrieves details of a specific version of a page")]
        private void RetrievePageVersion(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var pageVersion = _dataLayer.GetPageVersion(pageVersionId, (p, v) => v);

            if (pageVersion == null)
                request.NotFound("No page version with ID " + pageVersionId);
            else
                request.Success(pageVersion);
        }

        [Endpoint(
            UrlPath = "pageversion/{id}",
            Methods = new[] {Method.Put}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
        private void UpdatePageVersion(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdatePageVersion(request.Identity, pageVersionId, changes);

            if (result.Success)
            {
                var pageVersion = _dataLayer.GetPageVersion(pageVersionId, (p, v) => v);
                request.Success(pageVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "pageversion/{id}/routes", 
            Methods = new[] {Method.Put}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
        private void UpdatePageVersionRoutes(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var routes = request.Body<List<PageRouteRecord>>();

            var result = _dataLayer.UpdatePageVersionRoutes(request.Identity, pageVersionId, routes);

            if (result.Success)
            {
                var pageVersion = _dataLayer.GetPageVersion(pageVersionId, (p, v) => v);
                request.Success(pageVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "pageversion/{id}/zones", 
            Methods = new[] {Method.Put}, 
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
        private void UpdatePageVersionZones(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var zones = request.Body<List<LayoutZoneRecord>>();

            var result = _dataLayer.UpdatePageVersionLayoutZones(request.Identity, pageVersionId, zones);

            if (result.Success)
            {
                var pageVersion = _dataLayer.GetPageVersion(pageVersionId, (p, v) => v);
                request.Success(pageVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "pageversion/{id}/components",
            Methods = new[] {Method.Put},
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment)]
        private void UpdatePageVersionComponents(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var components = request.Body<List<ElementComponentRecord>>();

            var result = _dataLayer.UpdatePageVersionComponents(request.Identity, pageVersionId, components);

            if (result.Success)
            {
                var pageVersion = _dataLayer.GetPageVersion(pageVersionId, (p, v) => v);
                request.Success(pageVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "pageversion/{id}", 
            Methods = new[] {Method.Delete},
            RequiredPermission = Permissions.EditPage)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeletePageVersion(IEndpointRequest request)
        {
            var pageVersionId = request.Parameter<long>("id");
            var result = _dataLayer.DeletePageVersion(request.Identity, pageVersionId);

            if (result.Success)
                request.Success(new { pageVersionId });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Layouts

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new layout to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the layout to use in this segmentation test scenario")]
        [Description("Creates a new layout and optionally adds it to a specific version of the website")]
        private void CreateLayout(IEndpointRequest request)
        {
            var layout = request.Body<LayoutRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateLayout(request.Identity, layout);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            layout = _dataLayer.GetLayout(result.NewRecordId, l => l);
            if (layout == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new layout it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddLayoutToWebsiteVersion(request.Identity, layout.RecordId, 1, websiteVersionId.Value, scenario);
            }
            request.Success(layout);
        }

        [Endpoint(
            UrlPath = "layout/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void RetrieveLayout(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var layout = _dataLayer.GetLayout(id, l => l);

            if (layout == null)
                request.NotFound("No layout with ID " + id);
            else
                request.Success(layout);
        }

        [Endpoint(
            UrlPath = "layout/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateLayout(IEndpointRequest request)
        {
            var layoutId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateLayout(request.Identity, layoutId, changes);

            if (result.Success)
            {
                var layout = _dataLayer.GetLayout(layoutId, l => l);
                request.Success(layout);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "layout/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteLayout(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeleteLayout(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Layout versions

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new layout version to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the layout version to use in this segmentation test scenario")]
        [Description("Creates a new version of a layout and optionally adds it to a specific version of the website")]
        private void CreateLayoutVersion(IEndpointRequest request)
        {
            var layoutVersion = request.Body<LayoutVersionRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateLayoutVersion(request.Identity, layoutVersion);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            layoutVersion = _dataLayer.GetLayoutVersion(result.NewRecordId, (p, v) => v);
            if (layoutVersion == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new layout version it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {

                _dataLayer.AddLayoutToWebsiteVersion(request.Identity, layoutVersion.RecordId, websiteVersionId.Value, scenario);
            }

            request.Success(layoutVersion);
        }

        [Endpoint(
            UrlPath = "layoutversion/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the layout version to return")]
        [Description("Retrieves details of a specific version of a layout")]
        private void RetrieveLayoutVersion(IEndpointRequest request)
        {
            var layoutVersionId = request.Parameter<long>("id");
            var layoutVersion = _dataLayer.GetLayoutVersion(layoutVersionId, (p, v) => v);

            if (layoutVersion == null)
                request.NotFound("No layout version with ID " + layoutVersionId);
            else
                request.Success(layoutVersion);
        }

        [Endpoint(
            UrlPath = "layoutversion/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateLayoutVersion(IEndpointRequest request)
        {
            var layoutVersionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateLayoutVersion(request.Identity, layoutVersionId, changes);

            if (result.Success)
            {
                var layoutVersion = _dataLayer.GetLayoutVersion(layoutVersionId, (p, v) => v);
                request.Success(layoutVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "layoutversion/{id}/zones",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateLayoutVersionZones(IEndpointRequest request)
        {
            var layoutVersionId = request.Parameter<long>("id");
            var zones = request.Body<List<LayoutZoneRecord>>();

            var result = _dataLayer.UpdateLayoutVersionZones(request.Identity, layoutVersionId, zones);

            if (result.Success)
            {
                var layoutVersion = _dataLayer.GetLayoutVersion(layoutVersionId, (p, v) => v);
                request.Success(layoutVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "layoutversion/{id}/components",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateLayoutVersionComponents(IEndpointRequest request)
        {
            var layoutVersionId = request.Parameter<long>("id");
            var components = request.Body<List<ElementComponentRecord>>();

            var result = _dataLayer.UpdateLayoutVersionComponents(request.Identity, layoutVersionId, components);

            if (result.Success)
            {
                var layoutVersion = _dataLayer.GetLayoutVersion(layoutVersionId, (p, v) => v);
                request.Success(layoutVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "layoutversion/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditLayout)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteLayoutVersion(IEndpointRequest request)
        {
            var layoutVersionId = request.Parameter<long>("id");
            var result = _dataLayer.DeleteLayoutVersion(request.Identity, layoutVersionId);

            if (result.Success)
                request.Success(new { layoutVersionId });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Regions

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new region to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the region to use in this segmentation test scenario")]
        [Description("Creates a new region and optionally adds it to a specific version of the website")]
        private void CreateRegion(IEndpointRequest request)
        {
            var region = request.Body<RegionRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateRegion(request.Identity, region);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            region = _dataLayer.GetRegion(result.NewRecordId, r => r);
            if (region == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new region it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddRegionToWebsiteVersion(request.Identity, region.RecordId, 1, websiteVersionId.Value, scenario);
            }
            request.Success(region);
        }

        [Endpoint(
            UrlPath = "region/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void RetrieveRegion(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var region = _dataLayer.GetRegion(id, r => r);

            if (region == null)
                request.NotFound("No region with ID " + id);
            else
                request.Success(region);
        }

        [Endpoint(
            UrlPath = "region/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateRegion(IEndpointRequest request)
        {
            var regionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateRegion(request.Identity, regionId, changes);

            if (result.Success)
            {
                var region = _dataLayer.GetRegion(regionId, r => r);
                request.Success(region);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "region/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteRegion(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeleteRegion(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Region versions

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new region version to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the region version to use in this segmentation test scenario")]
        [Description("Creates a new version of a region and optionally adds it to a specific version of the website")]
        private void CreateRegionVersion(IEndpointRequest request)
        {
            var regionVersion = request.Body<RegionVersionRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateRegionVersion(request.Identity, regionVersion);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            regionVersion = _dataLayer.GetRegionVersion(result.NewRecordId, (p, v) => v);
            if (regionVersion == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new region version it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {

                _dataLayer.AddRegionToWebsiteVersion(request.Identity, regionVersion.RecordId, websiteVersionId.Value, scenario);
            }

            request.Success(regionVersion);
        }

        [Endpoint(
            UrlPath = "regionversion/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the region version to return")]
        [Description("Retrieves details of a specific version of a region")]
        private void RetrieveRegionVersion(IEndpointRequest request)
        {
            var regionVersionId = request.Parameter<long>("id");
            var regionVersion = _dataLayer.GetRegionVersion(regionVersionId, (p, v) => v);

            if (regionVersion == null)
                request.NotFound("No region version with ID " + regionVersionId);
            else
                request.Success(regionVersion);
        }

        [Endpoint(
            UrlPath = "regionversion/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateRegionVersion(IEndpointRequest request)
        {
            var regionVersionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateRegionVersion(request.Identity, regionVersionId, changes);

            if (result.Success)
            {
                var regionVersion = _dataLayer.GetRegionVersion(regionVersionId, (p, v) => v);
                request.Success(regionVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "regionversion/{id}/components",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateRegionVersionComponents(IEndpointRequest request)
        {
            var regionVersionId = request.Parameter<long>("id");
            var components = request.Body<List<ElementComponentRecord>>();

            var result = _dataLayer.UpdateRegionVersionComponents(request.Identity, regionVersionId, components);

            if (result.Success)
            {
                var regionVersion = _dataLayer.GetRegionVersion(regionVersionId, (p, v) => v);
                request.Success(regionVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "regionversion/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditRegion)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteRegionVersion(IEndpointRequest request)
        {
            var regionVersionId = request.Parameter<long>("id");
            var result = _dataLayer.DeleteRegionVersion(request.Identity, regionVersionId);

            if (result.Success)
                request.Success(new { regionVersionId });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Components

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new component to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the component to use in this segmentation test scenario")]
        [Description("Creates a new component and optionally adds it to a specific version of the website")]
        private void CreateComponent(IEndpointRequest request)
        {
            var component = request.Body<ComponentRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateComponent(request.Identity, component);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            component = _dataLayer.GetComponent(result.NewRecordId, l => l);
            if (component == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new component it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {
                _dataLayer.AddComponentToWebsiteVersion(request.Identity, component.RecordId, 1, websiteVersionId.Value, scenario);
            }
            request.Success(component);
        }

        [Endpoint(
            UrlPath = "component/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void RetrieveComponent(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var component = _dataLayer.GetComponent(id, l => l);

            if (component == null)
                request.NotFound("No component with ID " + id);
            else
                request.Success(component);
        }

        [Endpoint(
            UrlPath = "component/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateComponent(IEndpointRequest request)
        {
            var componentId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateComponent(request.Identity, componentId, changes);

            if (result.Success)
            {
                var component = _dataLayer.GetComponent(componentId, l => l);
                request.Success(component);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "component/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteComponent(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var result = _dataLayer.DeleteComponent(request.Identity, id);

            if (result.Success)
                request.Success(new { id });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion

        #region Component versions

        [Endpoint(
            Methods = new[] { Method.Post },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "websiteVersionId",
            typeof(PositiveNumber<long?>),
            Description = "Include the optional website version id to add the new component version to a specific version of the website")]
        [EndpointParameter(
            "scenario",
            typeof(OptionalString),
            Description = "Include the optional scenario name to make this the component version to use in this segmentation test scenario")]
        [Description("Creates a new version of a component and optionally adds it to a specific version of the website")]
        private void CreateComponentVersion(IEndpointRequest request)
        {
            var componentVersion = request.Body<ComponentVersionRecord>();
            var websiteVersionId = request.Parameter<long?>("websiteVersionId");
            var scenario = request.Parameter<string>("scenario");

            var result = _dataLayer.CreateComponentVersion(request.Identity, componentVersion);

            if (!result.Success)
            {
                request.BadRequest(result.DebugMessage);
                return;
            }

            componentVersion = _dataLayer.GetComponentVersion(result.NewRecordId, (p, v) => v);
            if (componentVersion == null)
            {
                request.HttpStatus(
                    HttpStatusCode.InternalServerError,
                    "After creating the new component version it could not be found in the database");
                return;
            }

            if (websiteVersionId.HasValue)
            {

                _dataLayer.AddComponentToWebsiteVersion(request.Identity, componentVersion.RecordId, websiteVersionId.Value, scenario);
            }

            request.Success(componentVersion);
        }

        [Endpoint(
            UrlPath = "componentversion/{id}",
            Methods = new[] { Method.Get },
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment,
            Description = "The ID of the component version to return")]
        [Description("Retrieves details of a specific version of a component")]
        private void RetrieveComponentVersion(IEndpointRequest request)
        {
            var componentVersionId = request.Parameter<long>("id");
            var componentVersion = _dataLayer.GetComponentVersion(componentVersionId, (p, v) => v);

            if (componentVersion == null)
                request.NotFound("No component version with ID " + componentVersionId);
            else
                request.Success(componentVersion);
        }

        [Endpoint(
            UrlPath = "componentversion/{id}",
            Methods = new[] { Method.Put },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void UpdateComponentVersion(IEndpointRequest request)
        {
            var componentVersionId = request.Parameter<long>("id");
            var changes = request.Body<List<PropertyChange>>();

            var result = _dataLayer.UpdateComponentVersion(request.Identity, componentVersionId, changes);

            if (result.Success)
            {
                var componentVersion = _dataLayer.GetComponentVersion(componentVersionId, (p, v) => v);
                request.Success(componentVersion);
            }
            else
            {
                request.BadRequest(result.DebugMessage);
            }
        }

        [Endpoint(
            UrlPath = "componentversion/{id}",
            Methods = new[] { Method.Delete },
            RequiredPermission = Permissions.EditComponent)]
        [EndpointParameter(
            "id",
            typeof(PositiveNumber<long>),
            EndpointParameterType.PathSegment)]
        private void DeleteComponentVersion(IEndpointRequest request)
        {
            var componentVersionId = request.Parameter<long>("id");
            var result = _dataLayer.DeleteComponentVersion(request.Identity, componentVersionId);

            if (result.Success)
                request.Success(new { componentVersionId });
            else
                request.BadRequest(result.DebugMessage);
        }

        #endregion
    }
}
