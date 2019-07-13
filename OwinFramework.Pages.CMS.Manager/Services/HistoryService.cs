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
    /// Provides endpoints to return the history of database changes
    /// </summary>
    internal class HistoryService
    {
        private readonly IDataLayer _dataLayer;

        public HistoryService(IDataLayer dataLater)
        {
            _dataLayer = dataLater;
        }

        [Endpoint(
            UrlPath = "period/{type}/{id}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "type", 
            typeof(RequiredString),
            EndpointParameterType.PathSegment, 
            Description = "The type of record to get history for")]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the record to get history for")]
        [EndpointParameter(
            "bookmark", 
            typeof(OptionalString),
            Description = "Optional bookmark. Used to to infinite scroll back through the history")]
        private void Period(IEndpointRequest request)
        {
            var type = request.Parameter<string>("type");
            var id = request.Parameter<long>("id");
            var bookmark = request.Parameter<string>("bookmark");
            var history = _dataLayer.GetHistory(type, id, bookmark);
            if (history == null)
                request.NotFound();
            else
                request.Success(history);
        }

        [Endpoint(
            UrlPath = "summary/{id}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the history summary to return")]
        private void Summary(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var summary = _dataLayer.GetHistorySummary(id);
            if (summary == null)
                request.NotFound();
            else
            request.Success(summary);
        }
    }
}
