using System;
using System.Linq;
using Newtonsoft.Json.Serialization;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Segmentation;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Manager.Services
{
    /// <summary>
    /// Provides endpoints to return the history of database changes
    /// </summary>
    internal class SegmentTestingService
    {
        private readonly ISegmentTestingFramework _segmentTestingFramework;

        public SegmentTestingService(
            ISegmentTestingFramework segmentTestingFramework)
        {
            _segmentTestingFramework = segmentTestingFramework;
        }

        [Endpoint(
            UrlPath = "scenarios", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        private void AllScenarios(IEndpointRequest request)
        {
            var scenarios = _segmentTestingFramework.GetAllScenarios();
            request.Success(scenarios.Select(s => new Scenario
            {
                Name = s.Name,
                DisplayName = s.DisplayName,
                Description = s.Description
            }));
        }

        [Endpoint(
            UrlPath = "scenarios", 
            Methods = new[] {Method.Post}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        private void CreateScenario(IEndpointRequest request)
        {
            var scenario = request.Body<Scenario>();
            var scenarioName = _segmentTestingFramework.CreateScenario(scenario);
            request.Success(scenarioName);
        }

        [Endpoint(
            UrlPath = "scenario/{name}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter("name", typeof(RequiredString), EndpointParameterType.PathSegment)]
        private void RetrieveScenario(IEndpointRequest request)
        {
            var scenarioName = request.Parameter<string>("name");
            var scenario = _segmentTestingFramework.GetAllScenarios().FirstOrDefault(s => string.Equals(s.Name, scenarioName, StringComparison.OrdinalIgnoreCase));
            if (scenario == null)
                request.NotFound();
            else
                request.Success(new Scenario
                {
                    Name = scenario.Name,
                    DisplayName = scenario.DisplayName,
                    Description = scenario.Description
                });
        }

        [Endpoint(
            UrlPath = "scenarios", 
            Methods = new[] {Method.Put}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        private void UpdateScenario(IEndpointRequest request)
        {
            var scenario = request.Body<Scenario>();
            _segmentTestingFramework.UpdateScenario(scenario);
        }

        [Endpoint(
            UrlPath = "scenario/{name}", 
            Methods = new[] {Method.Delete}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        [EndpointParameter("name", typeof(RequiredString), EndpointParameterType.PathSegment)]
        private void DeleteScenario(IEndpointRequest request)
        {
            _segmentTestingFramework.DeleteScenario(request.Parameter<string>("name"));
        }

        [Endpoint(
            UrlPath = "tests", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        private void AllTests(IEndpointRequest request)
        {
            var tests = _segmentTestingFramework.GetAllTests();
            request.Success(tests.Select(t => new Test
            {
                Name = t.Name,
                DisplayName = t.DisplayName,
                Description = t.Description,
                StartUtc = t.StartUtc,
                EndUtc = t.EndUtc,
                EnvironmentName = t.EnvironmentName,
                PageNames = t.PageNames,
                SerializableScenarioMap = t.ScenarioMap.Select(m => new ScenarioMapEntry
                {
                    SegmentKey = m.Item1,
                    ScenarioName = m.Item2
                }).ToArray()
            }));
        }

        [Endpoint(
            UrlPath = "tests", 
            Methods = new[] {Method.Post}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        private void CreateTest(IEndpointRequest request)
        {
            var test = request.Body<Test>();
            if (test.SerializableScenarioMap != null)
                test.ScenarioMap = test.SerializableScenarioMap.Select(m => new Tuple<string, string>(m.SegmentKey, m.ScenarioName)).ToArray();

            var testName = _segmentTestingFramework.CreateTest(test);
            request.Success(testName);
        }

        [Endpoint(
            UrlPath = "test/{name}", 
            Methods = new[] {Method.Get}, 
            RequiredPermission = Permissions.View)]
        [EndpointParameter("name", typeof(RequiredString), EndpointParameterType.PathSegment)]
        private void RetrieveTest(IEndpointRequest request)
        {
            var testName = request.Parameter<string>("name");
            var test = _segmentTestingFramework.GetAllTests().FirstOrDefault(t => string.Equals(t.Name, testName, StringComparison.OrdinalIgnoreCase));
            if (test == null)
                request.NotFound();
            else
                request.Success(new Test
                {
                    Name = test.Name,
                    DisplayName = test.DisplayName,
                    Description = test.Description,
                    StartUtc = test.StartUtc,
                    EndUtc = test.EndUtc,
                    EnvironmentName = test.EnvironmentName,
                    PageNames = test.PageNames,
                    SerializableScenarioMap = test.ScenarioMap.Select(m => new ScenarioMapEntry
                    {
                        SegmentKey = m.Item1,
                        ScenarioName = m.Item2
                    }).ToArray()
                });
        }

        [Endpoint(
            UrlPath = "tests", 
            Methods = new[] {Method.Put}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        private void UpdateTest(IEndpointRequest request)
        {
            var test = request.Body<Test>();
            if (test.SerializableScenarioMap != null)
                test.ScenarioMap = test.SerializableScenarioMap.Select(m => new Tuple<string, string>(m.SegmentKey, m.ScenarioName)).ToArray();

            _segmentTestingFramework.UpdateTest(test);
        }

        [Endpoint(
            UrlPath = "test/{name}", 
            Methods = new[] {Method.Delete}, 
            RequiredPermission = Permissions.EditSegmentScenario)]
        [EndpointParameter("name", typeof(RequiredString), EndpointParameterType.PathSegment)]
        private void DeleteTest(IEndpointRequest request)
        {
            var testName = request.Parameter<string>("name");
            _segmentTestingFramework.DeleteTest(testName);
        }

        private class Scenario : ISegmentTestingScenario
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        private class Test : ISegmentTestingTest
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("start")]
            public DateTime StartUtc { get; set; }

            [JsonProperty("end")]
            public System.DateTime EndUtc { get; set; }

            [JsonProperty("environment")]
            public string EnvironmentName { get; set; }

            [JsonProperty("pages")]
            public string[] PageNames { get; set; }

            [JsonIgnore]
            public Tuple<string, string>[] ScenarioMap { get; set; }

            [JsonProperty("map")]
            public ScenarioMapEntry[] SerializableScenarioMap { get; set; }
        }

        private class ScenarioMapEntry
        {
            [JsonProperty("segmentKey")]
            public string SegmentKey { get; set; }

            [JsonProperty("scenarioName")]
            public string ScenarioName { get; set; }
        }
    }
}
