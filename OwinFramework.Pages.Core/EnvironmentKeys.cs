using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core
{
    /// <summary>
    /// Defines the keys added to the Owin environment by the Owin Framework
    /// </summary>
    public static class EnvironmentKeys
    {
        /// <summary>
        /// For A/B testing this is the name of the user segment to render the response for.
        /// This environment variable is not set if the user is not in any test segments or
        /// there is no active test
        /// </summary>
        public const string UserSegment = "framework.UserSegment";

        /// <summary>
        /// For A/B testing this is the name of the test scenario to render for this segment
        /// of users in the currently active test.
        /// This environment variable is not set if the user is not in any test segments or
        /// there is no active test
        /// </summary>
        public const string TestScenario = "framework.TestScenario";
    }
}
