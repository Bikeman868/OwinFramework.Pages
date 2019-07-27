using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Records analytics about user journeys for different segments of
    /// users. This is often referred to as A/B testing but in fact you can
    /// have as many user segments as you like
    /// </summary>
    public interface ISegmentTestRecorder
    {
        /// <summary>
        /// This is called whenever a segmented user makes a request. The recorder
        /// should capture any relevant information from the request so that user
        /// journeys can be compared for different segments of users
        /// </summary>
        /// <param name="context">The request that the user made</param>
        /// <param name="testId">The databse ID of the active segmentation test</param>
        /// <param name="segmentKey">The unique identifier for the users segment</param>
        void Record(IOwinContext context, long testId, string segmentKey);
    }
}
