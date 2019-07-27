using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Defines a framework for running A/B testing scenarios over specific time
    /// periods and collecting analytics
    /// </summary>
    public interface ISegmentTestingFramework
    {
        /// <summary>
        /// Gets the segments that can be configured for A/B testing
        /// </summary>
        /// <returns>An array of user segments that can be configured for A/B testing 
        /// of null if A/B testing is not supported in this configuration</returns>
        UserSegment[] GetSegments();

        /// <summary>
        /// Adds segmentation context to a request
        /// </summary>
        /// <param name="context">The request context to add segmentation 
        /// information to</param>
        void SegmentRequest(IOwinContext context);
    }
}
