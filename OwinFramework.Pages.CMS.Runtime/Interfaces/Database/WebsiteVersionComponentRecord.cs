﻿using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of component versions within a website
    /// </summary>
    public class WebsiteVersionComponentRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The unique ID of a component to include in this version of the website
        /// </summary>
        [Mapping("componentId")]
        public long ComponentId { get; set; }

        /// <summary>
        /// Defines which version of this component should be used in this 
        /// version of the website
        /// </summary>
        [Mapping("componentVersionId")]
        public long ComponentVersionId { get; set; }
    }
}
