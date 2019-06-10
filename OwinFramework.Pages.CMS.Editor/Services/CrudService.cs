using System;
using System.Collections.Generic;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.CMS.Editor.Services
{
    /// <summary>
    /// Provides create, retrieve, update and delete methods foreach entity type
    /// </summary>
    internal class CrudService
    {
        private readonly ILiveUpdateSender _liveUpdateSender;

        public CrudService(
            ILiveUpdateSender liveUpdateSender)
        {
            _liveUpdateSender = liveUpdateSender;
        }

        [Endpoint(Methods = new[] {Method.Post})]
        [EndpointParameter("title", typeof(string), EndpointParameterType.FormField)]
        [EndpointParameter("description", typeof(string), EndpointParameterType.FormField)]
        private void CreatePage(IEndpointRequest request)
        {
            _liveUpdateSender.Send(new MessageDto
            {
                WhenUtc = DateTime.UtcNow,
                UniqueId = Guid.NewGuid(),
                MachineName = Environment.MachineName,
                NewElements = new List<ElementReference> { new ElementReference{ElementType = "page", ElementId = 1} },
            });

            request.Success(new
            {
                id = 1,
                title = "New Page Title",
                description = "New page description"
            });
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Get})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void RetrievePage(IEndpointRequest request)
        {
            request.Success(new
            {
                id = 1,
                title = "Some Title",
                description = "Some description"
            });
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Put})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        [EndpointParameter("title", typeof(string), EndpointParameterType.FormField)]
        [EndpointParameter("description", typeof(string), EndpointParameterType.FormField)]
        private void UpdatePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");
            var title = request.Parameter<string>("title");
            var description = request.Parameter<string>("description");

            _liveUpdateSender.Send(new MessageDto
            {
                WhenUtc = DateTime.UtcNow,
                UniqueId = Guid.NewGuid(),
                MachineName = Environment.MachineName,
                PropertyChanges = new List<PropertyChange>
                {
                    new PropertyChange
                    {
                        ElementType = "page", 
                        ElementVersionId = id, 
                        PropertyName = "title", 
                        PropertyValue = title
                    },
                    new PropertyChange
                    {
                        ElementType = "page", 
                        ElementVersionId = id, 
                        PropertyName = "description", 
                        PropertyValue = description
                    }
                }
            });

            request.Success(new
            {
                id,
                title,
                description
            });
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Delete})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void DeletePage(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");

            _liveUpdateSender.Send(new MessageDto
            {
                WhenUtc = DateTime.UtcNow,
                UniqueId = Guid.NewGuid(),
                MachineName = Environment.MachineName,
                DeletedElements = new List<ElementReference>
                {
                    new ElementReference{ElementType = "page", ElementId = request.Parameter<long>("id")}
                },
            });

            request.Success(new
            {
                id
            });
        }

    }
}
