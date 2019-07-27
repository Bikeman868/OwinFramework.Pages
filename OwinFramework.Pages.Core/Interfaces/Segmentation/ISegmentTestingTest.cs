using System;

namespace OwinFramework.Pages.Core.Interfaces.Segmentation
{
    /// <summary>
    /// Defaines a segmentation test that will run over a defined interval and
    /// will present certain segments of users with a different user experience
    /// and capture analytics about their user journeys through the website
    /// </summary>
    public interface ISegmentTestingTest
    {
        /// <summary>
        /// The unique internal name of this test
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of this test to display in a UI
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// An HTML formatted description of this test
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The date and time in UTC on which this test is/was scheduled to start
        /// </summary>
        DateTime StartUtc { get; }

        /// <summary>
        /// The date and time in UTC on which this test is/was scheduled to end
        /// </summary>
        DateTime EndUtc { get; }

        /// <summary>
        /// The name of the environment in which this test is to be conducted,
        /// for example 'production' or 'staging'
        /// </summary>
        string EnvironmentName { get; }

        /// <summary>
        /// The names of the pages on which the test scenarios are to be applied
        /// </summary>
        string[] PageNames { get; }

        /// <summary>
        /// A collection of mappings between user segment keys and scenario names
        /// that defines which user experience each segment of users should see
        /// for the duration of the test
        /// </summary>
        Tuple<string, string>[] ScenarioMap { get; }
    }
}