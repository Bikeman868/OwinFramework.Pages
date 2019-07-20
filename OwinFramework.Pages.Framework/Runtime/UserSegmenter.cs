using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// The default segmenter just separates out logged in users. Application developers
    /// should write their own version of this interface and provide application specific
    /// logic to segment users
    /// </summary>
    internal class UserSegmenter: IUserSegmenter
    {
        private readonly UserSegment[] _segments = new[]
        {
            new UserSegment { Name = "A", Description = "Logged in users" },
        };

        UserSegment[] IUserSegmenter.GetSegments()
        {
            return _segments;
        }

        string IUserSegmenter.GetSegment(Microsoft.Owin.IOwinContext owinContext)
        {
            var identification = owinContext.GetFeature<IIdentification>();

            if (identification == null || identification.IsAnonymous) 
                return string.Empty; // Defaut website behaviour

            return "A"; // Different UX for logged in users
        }
    }
}
