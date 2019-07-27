using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace Sample1
{
    /// <summary>
    /// This segmenter assigns users randomly to A, B or default segment
    /// This is only useful for testing and experimenting, you would never
    /// want to do this in production code
    /// </summary>
    internal class UserSegmenter : IUserSegmenter
    {
        private readonly UserSegment[] _segments =
        {
            new UserSegment{ Index = 0, Name = "Group A", Key = "A" },
            new UserSegment{ Index = 1, Name = "Group B", Key = "B" }
        };

        private readonly Random _random = new Random();

        UserSegment[] IUserSegmenter.GetSegments()
        {
            return _segments;
        }

        int? IUserSegmenter.GetSegmentIndex(IOwinContext owinContext)
        {
            switch (_random.Next(_segments.Length + 1))
            {
                case 0: return 0;
                case 1: return 1;
            }
            return null;
        }
    }
}