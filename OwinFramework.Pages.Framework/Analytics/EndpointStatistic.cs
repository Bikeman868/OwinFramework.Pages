using System;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.Pages.Framework.Analytics
{
    /// <summary>
    /// Encapsulates information about a stat that is available from the pages middleware
    /// </summary>
    public class EndpointStatistic
    {
        /// <summary>
        /// Contains details about the statistic
        /// </summary>
        public IStatisticInformation Statistic;

        /// <summary>
        /// Retrieves the current value of this statistic
        /// </summary>
        public Func<IStatistic> GetStatistic;
    }
}