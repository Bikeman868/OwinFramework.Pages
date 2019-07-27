using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Segmentation
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

        /// <summary>
        /// Returns a list of all test scenarios
        /// </summary>
        ISegmentTestingScenario[] GetAllScenarios();

        /// <summary>
        /// Adds a new test scenario
        /// </summary>
        /// <returns>The unique name assigned to this scenario</returns>
        string CreateScenario(ISegmentTestingScenario scenario);

        /// <summary>
        /// Updates an existing scenario. Note that the name of the
        /// scenario cannot be changed, the name provided in the scenario
        /// identifies which scenario will be updated
        /// </summary>
        void UpdateScenario(ISegmentTestingScenario scenario);

        /// <summary>
        /// Deletes a test scenario. This must delete the scenario from the
        /// results of calling GetAllScenarios but can retain the scenario
        /// in the database for referential integrity (going back and looking
        /// at the analytics for old tests)
        /// </summary>
        void DeleteScenario(string scenarioName);

        /// <summary>
        /// Returns a list of all tests
        /// </summary>
        ISegmentTestingTest[] GetAllTests();

        /// <summary>
        /// Adds a new test
        /// </summary>
        /// <returns>The unique name assigned to this test</returns>
        string CreateTest(ISegmentTestingTest test);

        /// <summary>
        /// Updates an existing test. Note that the name of the
        /// test cannot be changed, the name provided in the test
        /// identifies which test will be updated
        /// </summary>
        void UpdateTest(ISegmentTestingTest test);

        /// <summary>
        /// Deletes a test. This must delete the test from the
        /// results of calling GetAllTests but can retain the test
        /// in the database for referential integrity (going back and looking
        /// at the analytics for old tests)
        /// </summary>
        void DeleteTest(string testName);
    }
}
