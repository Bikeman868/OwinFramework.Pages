using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("id")]
        public long Id { get; set; }

        /// <summary>
        /// The name of this website version. This name is displayed in the CMS editor so that
        /// the user can identify which version this is. If the name is blank then the
        /// ID will be used instead. It is expected that users will use this field for a
        /// version number like 1.2, 1.3 etc but the format of this field is not defined by the CMS
        /// </summary>
        [Mapping("name")]
        public string Name { get; set; }

        /// <summary>
        /// Allows the user to enter an optional description to help them to remember
        /// why they created this version in the CMS editor
        /// </summary>
        [Mapping("description")]
        public string Description { get; set; }

        /// <summary>
        /// The identity of the user/application/system that created this version
        /// </summary>
        [Mapping("createdBy")]
        public string CraetedBy { get; set; }

        /// <summary>
        /// The date/time when this version was created
        /// </summary>
        [Mapping("createdWhen")]
        public DateTime CreatedWhen { get; set; }

        /// <summary>
        /// This is used to construct the Canonical URL of pages in the website
        /// </summary>
        [Mapping("baseUrl")]
        public string BaseUrl { get; set; }
    }
}
