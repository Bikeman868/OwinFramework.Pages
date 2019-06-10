using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using OwinFramework.Pages.Restful.Serializers;

namespace OwinFramework.Pages.CMS.Editor.Services
{
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
        private void NewPage(IEndpointRequest request)
        {
            _liveUpdateSender.Send(new MessageDto
            {
                WhenUtc = DateTime.UtcNow,
                UniqueId = Guid.NewGuid(),
                MachineName = Environment.MachineName,
                NewElements = new List<ElementReference> { new ElementReference{ElementType = "page", ElementId = 1} },
            });
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Get})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void GetPage(IEndpointRequest request)
        {
            request.Success(new
            {
                id = 1,
                title = "Some Title",
                description = "Some description"
            });
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Delete})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        private void DeletePage(IEndpointRequest request)
        {
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
        }

        [Endpoint(UrlPath = "page/{id}", Methods = new[] {Method.Put})]
        [EndpointParameter("id", typeof(PositiveNumber<long>), EndpointParameterType.PathSegment)]
        [EndpointParameter("title", typeof(string), EndpointParameterType.FormField)]
        [EndpointParameter("description", typeof(string), EndpointParameterType.FormField)]
        private void UpdatePage(IEndpointRequest request)
        {
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
                        ElementVersionId = request.Parameter<long>("id"), 
                        PropertyName = "title", 
                        PropertyValue = request.Parameter<string>("title")
                    },
                    new PropertyChange
                    {
                        ElementType = "page", 
                        ElementVersionId = request.Parameter<long>("id"), 
                        PropertyName = "description", 
                        PropertyValue = request.Parameter<string>("description")
                    }
                }
            });
        }
    }
}
