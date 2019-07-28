using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class SegmentTestingData: ISegmentTestingData
    {
        private ISegmentTestingTest[] _allTests;
        private ISegmentTestingScenario[] _allScenarios;

        private readonly object _lock = new object();

        public SegmentTestingData()
        {
            _allScenarios = new ISegmentTestingScenario[]
            {
                new Scenario
                {
                    Name = "a",
                    DisplayName = "Blue theme",
                    Description = "Uses the blue color scheme"
                },
                new Scenario
                {
                    Name = "b",
                    DisplayName = "Yellow theme",
                    Description = "Uses the yellow color scheme"
                }
            };

            _allTests = new ISegmentTestingTest[]
            {
                new Test
                {
                    Name = "test1",
                    DisplayName = "Test 1",
                    Description = "First test",
                    StartUtc = DateTime.UtcNow.AddMinutes(1),
                    EndUtc = DateTime.UtcNow.AddMinutes(10),
                    EnvironmentName = "production",
                    PageNames = new[]
                    {
                        "customer_list"
                    },
                    ScenarioMap = new[]
                    {
                        new Tuple<string, string>("s0", _allScenarios[0].Name),
                        new Tuple<string, string>("a", _allScenarios[0].Name),
                        new Tuple<string, string>("b", _allScenarios[1].Name)
                    }
                }
            };
        }

        ISegmentTestingScenario[] ISegmentTestingData.GetAllScenarios()
        {
            return _allScenarios;
        }

        string ISegmentTestingData.CreateScenario(ISegmentTestingScenario scenario)
        {
            lock (_lock)
            {
                var list = _allScenarios.ToList();
                list.Add(scenario);
                _allScenarios = list.ToArray();
                return "s" + _allScenarios.Length;
            }
        }

        void ISegmentTestingData.UpdateScenario(ISegmentTestingScenario scenario)
        {
            lock (_lock)
            {
                for (var i = 0; i < _allScenarios.Length; i++)
                {
                    if (_allScenarios[i].Name == scenario.Name)
                        _allScenarios[i] = scenario;
                }
            }
        }

        void ISegmentTestingData.DeleteScenario(string scenarioName)
        {
            lock (_lock)
            {
                _allScenarios = _allScenarios.Where(s => s.Name != scenarioName).ToArray();
            }
        }

        ISegmentTestingTest[] ISegmentTestingData.GetAllTests()
        {
            return _allTests;
        }

        string ISegmentTestingData.CreateTest(ISegmentTestingTest test)
        {
            lock (_lock)
            {
                var list = _allTests.ToList();
                list.Add(test);
                _allTests = list.ToArray();
                return "t" + _allTests.Length;
            }
        }

        void ISegmentTestingData.UpdateTest(ISegmentTestingTest test)
        {
            lock (_lock)
            {
                for (var i = 0; i < _allTests.Length; i++)
                {
                    if (_allTests[i].Name == test.Name)
                        _allTests[i] = test;
                }
            }
        }

        void ISegmentTestingData.DeleteTest(string testName)
        {
            lock (_lock)
            {
                _allTests = _allTests.Where(s => s.Name != testName).ToArray();
            }
        }

        private class Test: ISegmentTestingTest
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string EnvironmentName { get; set; }
            public DateTime StartUtc { get; set; }
            public DateTime EndUtc { get; set; }
            public string[] PageNames { get; set; }
            public Tuple<string, string>[] ScenarioMap { get; set; }
        }

        private class Scenario: ISegmentTestingScenario
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }
    }
}
