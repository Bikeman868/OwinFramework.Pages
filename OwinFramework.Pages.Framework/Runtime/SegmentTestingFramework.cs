using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Owin;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class SegmentTestingFramework: ISegmentTestingFramework
    {
        private readonly IUserSegmenter _userSegmenter;
        private readonly ISegmentTestRecorder _segmentTestRecorder;
        private readonly IRequestRouter _requestRouter;
        private readonly UserSegment[] _userSegments;
        private readonly Thread _schedulingThread;

        private SegmentTest _activeTest;

        public SegmentTestingFramework(
            IUserSegmenter userSegmenter,
            ISegmentTestRecorder segmentTestRecorder,
            IRequestRouter requestRouter)
        {
            _userSegmenter = userSegmenter;
            _segmentTestRecorder = segmentTestRecorder;
            _requestRouter = requestRouter;

            _userSegments = userSegmenter.GetSegments();

            if (_userSegments != null)
            {
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
            }

            if (_userSegments != null)
            {
                _schedulingThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Thread.Sleep(3000);
                            RunSchedule();
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
        }

        UserSegment[] ISegmentTestingFramework.GetSegments()
        {
            return _userSegments;
        }

        void ISegmentTestingFramework.SegmentRequest(IOwinContext context)
        {
            if (_userSegments == null) return;

            var test = _activeTest;
            if (test == null) return;

            var segmentIndex = _userSegmenter.GetSegmentIndex(context);
            if (segmentIndex.HasValue && segmentIndex.Value >= 0 && segmentIndex.Value <= _userSegments.Length)
            {
                var segment = _userSegments[segmentIndex.Value];
                context.Set(EnvironmentKeys.UserSegment, segment);
                _segmentTestRecorder.Record(context, test.TestId, segment.Key);
            }
        }

        private void RunSchedule()
        {
            if (_activeTest == null)
            {
                // TODO: look at test schedule to see if there is a test scheduled to run
                _activeTest = new SegmentTest
                {
                    TestId = 1,
                    StartDateTime = DateTime.UtcNow,
                    FinishDateTime = DateTime.UtcNow.AddHours(1),
                    PageIds = new[] {1L, 2L, 3L}
                };
                // TODO: Get routes to pages included in the test and register new routes with +1 priority and capture IDisposables
            }
            else
            {
                if (DateTime.UtcNow >= _activeTest.FinishDateTime)
                {
                    if (_activeTest.Routes != null)
                        foreach(var route in _activeTest.Routes)
                            route.Dispose();

                    _activeTest = null;
                }
            }
        }

        private class SegmentTest
        {
            public long TestId { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime FinishDateTime { get; set; }
            public long[] PageIds { get; set; }
            public IDisposable[] Routes { get; set; }
        }
    }
}
