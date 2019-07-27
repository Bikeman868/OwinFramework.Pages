using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Owin;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class SegmentTestingFramework: ISegmentTestingFramework
    {
        private readonly IUserSegmenter _userSegmenter;
        private readonly ISegmentTestRecorder _segmentTestRecorder;
        private readonly IRequestRouter _requestRouter;
        private readonly ISegmentTestingData _segmentTestingData;
        private readonly UserSegment[] _userSegments;
        private readonly Thread _schedulingThread;

        private ISegmentTestingTest _activeSegmentTest;
        private Test _activeTest;

        public SegmentTestingFramework(
            IUserSegmenter userSegmenter,
            ISegmentTestRecorder segmentTestRecorder,
            IRequestRouter requestRouter,
            ISegmentTestingData segmentTestingData)
        {
            _userSegmenter = userSegmenter;
            _segmentTestRecorder = segmentTestRecorder;
            _requestRouter = requestRouter;
            _segmentTestingData = segmentTestingData;

            _userSegments = userSegmenter.GetSegments();
            if (_userSegments == null) return;

            var hash = new HashSet<string>();
            for (var i = 0; i < _userSegments.Length; i++)
            {
                if (_userSegments[i].Index != i)
                    throw new Exception("The user segmenter implementation must set index values equal to the position of the segment within the array");

                if (string.IsNullOrEmpty(_userSegments[i].Key))
                    throw new Exception("The user segmenter implementation can not return empty keys for segments");

                if (string.IsNullOrEmpty(_userSegments[i].Name))
                    throw new Exception("The user segmenter implementation can not return empty names for segments");

                if (!hash.Add(_userSegments[i].Key))
                    throw new Exception("The user segmenter implementation must return unique keys for each segment");
            }

            if (_userSegments.Length == 0)
                _userSegments = null;

            if (_userSegments == null) return;

            _schedulingThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(3000);
                        RunSchedule();
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch
                    {
                    }
                }
            })
            {
                Name = "Segment test scheduler",
                IsBackground = true
            };
            _schedulingThread.Start();
        }

        UserSegment[] ISegmentTestingFramework.GetSegments()
        {
            return _userSegments;
        }

        void ISegmentTestingFramework.SegmentRequest(IOwinContext context)
        {
            if (_userSegments == null) return;

            var activeTest = _activeTest;
            var activeSegmentTest = _activeSegmentTest;
            if (activeTest == null) return;

            var segmentIndex = _userSegmenter.GetSegmentIndex(context);
            if (!segmentIndex.HasValue || segmentIndex.Value < 0 || segmentIndex.Value > _userSegments.Length) return;

            var segment = _userSegments[segmentIndex.Value];
            context.Set(EnvironmentKeys.UserSegment, segment);

            var scenarioMap = activeTest.ScenarioMap.FirstOrDefault(m => string.Equals(m.Item1, segment.Key, StringComparison.OrdinalIgnoreCase));
            if (scenarioMap == null) return;

            var scenario = scenarioMap.Item2;
            if (scenario == null) return;

            context.Set(EnvironmentKeys.TestScenario, scenario);
            _segmentTestRecorder.Record(context, activeSegmentTest, segment.Key, scenario);
        }

        private void RunSchedule()
        {
            if (_activeTest == null)
            {
                var now = DateTime.UtcNow;
                foreach (var testData in _segmentTestingData.GetAllTests())
                {
                    if (testData.StartUtc > now || testData.EndUtc <= now) continue;

                    var scenarios = _segmentTestingData.GetAllScenarios();

                    var test = new Test
                    {
                        Name = testData.Name,
                        EnvironmentName = testData.EnvironmentName,
                        StartUtc = testData.StartUtc,
                        EndUtc = testData.EndUtc,
                        PageNames = testData.PageNames,
                        ScenarioMap = _userSegments
                            .Select(segment =>
                            {
                                var map = testData.ScenarioMap.FirstOrDefault(m => string.Equals(m.Item1, segment.Key, StringComparison.OrdinalIgnoreCase));
                                ISegmentTestingScenario scenario = null;
                                if (map != null) scenario = scenarios.FirstOrDefault(s => string.Equals(s.Name, map.Item2, StringComparison.OrdinalIgnoreCase));
                                return new Tuple<string, ISegmentTestingScenario>(segment.Key, scenario);
                            })
                            .ToArray()
                    };

                    // TODO: Get routes to pages included in the test and register new routes with +1 priority and capture IDisposables

                    _activeTest = test;
                    _activeSegmentTest = testData;
                    break;
                }
            }
            else
            {
                if (DateTime.UtcNow >= _activeTest.EndUtc)
                {
                    if (_activeTest.Routes != null)
                        foreach(var route in _activeTest.Routes)
                            route.Dispose();

                    _activeTest = null;
                }
            }
        }

        private class Test
        {
            public string Name { get; set; }
            public string EnvironmentName { get; set; }
            public DateTime StartUtc { get; set; }
            public DateTime EndUtc { get; set; }
            public string[] PageNames { get; set; }
            public Tuple<string, ISegmentTestingScenario>[] ScenarioMap { get; set; }
            public IDisposable[] Routes { get; set; }
        }

        #region ISegmentTestingData mixin

        ISegmentTestingScenario[] ISegmentTestingFramework.GetAllScenarios()
        {
            return _segmentTestingData.GetAllScenarios();
        }

        string ISegmentTestingFramework.CreateScenario(ISegmentTestingScenario scenario)
        {
            return _segmentTestingData.CreateScenario(scenario);
        }

        void ISegmentTestingFramework.UpdateScenario(ISegmentTestingScenario scenario)
        {
            _segmentTestingData.UpdateScenario(scenario);
        }

        void ISegmentTestingFramework.DeleteScenario(string scenarioName)
        {
            _segmentTestingData.DeleteScenario(scenarioName);
        }

        ISegmentTestingTest[] ISegmentTestingFramework.GetAllTests()
        {
            return _segmentTestingData.GetAllTests();
        }

        string ISegmentTestingFramework.CreateTest(ISegmentTestingTest test)
        {
            return _segmentTestingData.CreateTest(test);
        }

        void ISegmentTestingFramework.UpdateTest(ISegmentTestingTest test)
        {
            _segmentTestingData.UpdateTest(test);
        }

        void ISegmentTestingFramework.DeleteTest(string testName)
        {
            _segmentTestingData.DeleteTest(testName);
        }

        #endregion
    }
}
