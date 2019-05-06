using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Used to expose a C# class to the CMS that can be selected
    /// as the type of data to repeat in a region
    /// </summary>
    public class DataScopeRecord
    {
        /// <summary>
        /// The unique ID of this data scope
        /// </summary>
        [Mapping("dataScopeId")]
        public long DataScopeId { get; set; }

        /// <summary>
        /// The name to show users in the CMS UI
        /// </summary>
        [Mapping("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// An HTML formatted description to show in the CMS UI
        /// </summary>
        [Mapping("description")]
        public string Description { get; set; }

        /// <summary>
        /// The identity of the user/application/system that created this data scope
        /// </summary>
        [Mapping("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date/time when this data scope was created
        /// </summary>
        [Mapping("createdWhen")]
        public DateTime CreatedWhen { get; set; }

        /// <summary>
        /// The name to configure on elements
        /// </summary>
        [Mapping("name")]
        public string Name { get; set; }

        /// <summary>
        /// Optional data type specifier. If this is missing then the Name
        /// property is assumed to be the name of a data provider
        /// </summary>
        [Mapping("dataTypeId")]
        public long? DataTypeId { get; set; }
    }
}
