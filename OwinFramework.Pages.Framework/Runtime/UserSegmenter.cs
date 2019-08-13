using System;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// The default segmenter separates logged in users randomly into two groups. 
    /// Application developers should write their own version of this interface and 
    /// provide application specific logic to segment users then register their 
    /// implementation with IoC instead of this one
    /// </summary>
    internal class UserSegmenter: IUserSegmenter
    {
        private const string _cookieName = "segment";
        private readonly Random _random = new Random();

        private readonly UserSegment[] _segments = {
            new UserSegment
            {
                Index = 0,
                Name = "Logged in A", 
                Description = "Group A of users that are logged into the website", 
                Key = "a"
            },
            new UserSegment
            {
                Index = 1,
                Name = "Logged in B",
                Description = "Group B of users that are logged into the website",
                Key = "b"
            }
        };

        UserSegment[] IUserSegmenter.GetSegments()
        {
            return _segments;
        }

        int? IUserSegmenter.GetSegmentIndex(IOwinContext owinContext)
        {
            var identification = owinContext.GetFeature<IIdentification>();

            // Not logged in users get the default website behavior

            if (identification == null || identification.IsAnonymous) 
                return null;

            // If the user has visited before use the cookie to segment them

            var segmentCookie = owinContext.Request.Cookies[_cookieName];

            foreach (var segment in _segments)
                if (segmentCookie == segment.Key) return segment.Index;

            // If the user has not visited before then put them in a random segment

            var randomSegment = _segments[_random.Next(_segments.Length)];

            owinContext.Response.Cookies.Append(
                _cookieName, 
                randomSegment.Key, 
                new CookieOptions { Expires = DateTime.UtcNow.AddDays(7) });

            return randomSegment.Index;
        }
    }
}
