using System.Linq;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.CMS.Manager.Services
{
    /// <summary>
    /// Provides endpoints to manage element versions and which version of
    /// each element is used in each version of the website. Also takes into
    /// account user segmentation for A/B testing
    /// </summary>
    internal class VersionsService
    {
        private readonly IDataLayer _dataLayer;
        private readonly ISegmentTestingFramework _segmentTestingFramework;

        public VersionsService(
            IDataLayer dataLater,
            ISegmentTestingFramework segmentTestingFramework)
        {
            _dataLayer = dataLater;
            _segmentTestingFramework = segmentTestingFramework;
        }

        [Endpoint(UrlPath = "websiteversions", Methods = new[] {Method.Get}, RequiredPermission = Permissions.View)]
        [Description("Returns a list of all versions of the website")]
        private void WebsiteVersions(IEndpointRequest request)
        {
            var records = _dataLayer.GetWebsiteVersions(wvp => wvp);
            request.Success(records);
        }

        [Endpoint(
            UrlPath = "{type}/{id}",
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [Description("Retrieves a list of all versions of an element (for example 'Page') including which versions of the website use each versions of the record in different segmentation scenarios")]
        [EndpointParameter(
            "type", 
            typeof(RequiredString), 
            EndpointParameterType.PathSegment, 
            Description = "The type of record to return versions for. For example the RecordType property of the PageRecord class is 'Page'. See other XxxxRecord class constructors for their record type names")]
        [EndpointParameter(
            "id", 
            typeof(PositiveNumber<long>), 
            EndpointParameterType.PathSegment, 
            Description = "The ID of the record to return versions of")]
        private void GetElementVersions(IEndpointRequest request)
        {
            var type = request.Parameter<string>("type");
            var id = request.Parameter<long>("id");

            var response = new ElementVersionsResponse
            {
                RecordId = id,
                RecordType = type
            };

            response.Versions = _dataLayer.GetElementVersions(id, r => new ElementVersionResponse
            {
                VersionId = r.RecordId,
                Version = r.Version,
                Name = r.Name,
                Description = r.Description
            });

            foreach (var version in response.Versions)
            {
                version.Usages = _dataLayer.GetElementUsage(version.VersionId, r => new ElementVersionUsageResponse
                {
                    WebsiteVersionId = r.WebsiteVersionId,
                    SegmentationScenarioName = r.Scenario
                });
            }

            request.Success(response);
        }

        private class ElementVersionsResponse
        {
            [JsonProperty("type")]
            public string RecordType { get; set; }

            [JsonProperty("id")]
            public long RecordId { get; set; }

            [JsonProperty("versions")]
            public ElementVersionResponse[] Versions { get; set; }
        }

        private class ElementVersionResponse
        {
            [JsonProperty("versionId")]
            public long VersionId { get; set; }

            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("usages")]
            public ElementVersionUsageResponse[] Usages { get; set; }
        }

        private class ElementVersionUsageResponse
        {
            [JsonProperty("websiteVersionId")]
            public long WebsiteVersionId { get; set; }

            [JsonProperty("scenario")]
            public string SegmentationScenarioName { get; set; }
        }

    }
}
