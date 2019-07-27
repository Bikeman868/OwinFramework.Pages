using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class SegmentTestRecorder: ISegmentTestRecorder
    {
        /// <summary>
        /// Records a request by a segmented user
        /// </summary>
        /// <param name="context">The request that the user made</param>
        /// <param name="testId">The database ID of the segmentation test that is running</param>
        /// <param name="segmentKey">The key that identifies the user's segment</param>
        void ISegmentTestRecorder.Record(IOwinContext context, long testId, string segmentKey)
        {
            var identification = context.GetFeature<IIdentification>();
            var identity = identification == null ? "unknown" : (identification.IsAnonymous ? "anonymous" : identification.Identity);

            Trace.WriteLine("SEGMENT: " + identity + " user in '" + segmentKey + "' segment requested " + context.Request.Uri);
        }
    }
}
