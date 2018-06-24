using System;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// This is used during initialization to capture the infomation
    /// needed to build the data context tree for each request
    /// </summary>
    public class DataReference
    {
        /// <summary>
        /// The data type that needs to be added to the data context
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The scope name used to locate the data provider
        /// </summary>
        public string Scope { get; set; }
    }
}
