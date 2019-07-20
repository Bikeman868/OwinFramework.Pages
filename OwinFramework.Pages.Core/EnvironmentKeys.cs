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
        /// For A/B testing this is the name of the user segment to render the response for
        /// </summary>
        public const string UserSegment = "framework.UserSegment";
    }
}
