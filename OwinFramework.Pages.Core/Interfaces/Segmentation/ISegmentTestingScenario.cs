namespace OwinFramework.Pages.Core.Interfaces.Segmentation
{
    /// <summary>
    /// Defines a set of changes to the website that can be applied to a
    /// segment of users during a user segmentation test. A test scenario
    /// might be a different color theme, a different layout of a specific
    /// page or set of pages, different style or placement of call to action
    /// etc.
    /// </summary>
    public interface ISegmentTestingScenario
    {
        /// <summary>
        /// The unique internal name of this scenario. This is the name that
        /// must be used in the ScenarioMap property of the test
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of this scenario to display in a UI
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// An HTML formatted description of this scenario
        /// </summary>
        string Description { get; }
    }
}