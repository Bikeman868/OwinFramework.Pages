using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Used to expose a C# class to the CMS that can be selected
    /// as the type of data to repeat in a region
    /// </summary>
    public class DataTypeVersionRecord
    {
        /// <summary>
        /// The ID of the data type that this is a version of
        /// </summary>
        [Mapping("dataTypeId")]
        public long DataTypeId { get; set; }

        /// <summary>
        /// The unique ID of this data type version
        /// </summary>
        [Mapping("dataTypeVersionId")]
        public long DataTypeVersionId { get; set; }

        /// <summary>
        /// The identity of the user/application/system that created this data type version
        /// </summary>
        [Mapping("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date/time when this data type version was created
        /// </summary>
        [Mapping("createdWhen")]
        public DateTime CreatedWhen { get; set; }

        /// <summary>
        /// The name of the .Net Assembly that contains this type
        /// </summary>
        [Mapping("assembly")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// The fully qualified name of the C# class
        /// </summary>
        [Mapping("typeName")]
        public string TypeName { get; set; }

        /// <summary>
        /// The .Net Type that this record represents
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Ids of the data scope records for this type of data
        /// </summary>
        public long[] ScopeIds { get; set; }
    }
}
