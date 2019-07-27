using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Segmentation
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
        /// <param name="test">The active segmentation test</param>
        /// <param name="segmentKey">The unique identifier for the users segment</param>
        /// <param name="scenario">The test scenario to enact for this segment of users</param>
        void Record(IOwinContext context, ISegmentTestingTest test, string segmentKey, ISegmentTestingScenario scenario);
    }
}
