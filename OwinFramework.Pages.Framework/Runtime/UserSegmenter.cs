using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// The default segmenter just separates out logged in users. Application developers
    /// should write their own version of this interface and provide application specific
    /// logic to segment users then register their implementation with IoC instead of this 
    /// one
    /// </summary>
    internal class UserSegmenter: IUserSegmenter
    {
        private readonly UserSegment[] _segments = {
            new UserSegment
            {
                Index = 0,
                Name = "Logged in", 
                Description = "Users that are logged into the website", 
                Key = "s0"
            }
        };

        UserSegment[] IUserSegmenter.GetSegments()
        {
            return _segments;
        }

        int? IUserSegmenter.GetSegmentIndex(Microsoft.Owin.IOwinContext owinContext)
        {
            var identification = owinContext.GetFeature<IIdentification>();

            if (identification == null || identification.IsAnonymous) 
                return null; // Defaut website behaviour

            return 0; // Different UX for logged in users
        }
    }
}
