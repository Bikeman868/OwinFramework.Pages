﻿using Newtonsoft.Json;
using Prius.Contracts.Attributes;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all element versions.
    /// </summary>
    public class ElementVersionRecordBase: RecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element version in the database.
        /// </summary>
        [Mapping("elementVersionId")]
        [JsonProperty("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The unique ID of the element that this is a version of.
        /// </summary>
        [Mapping("elementId")]
        [JsonProperty("elementId")]
        public long ElementId { get; set; }

        /// <summary>
        /// The version number of this version of the element. The website
        /// can use different versions of the same element on different pages
        /// at the same time.
        /// </summary>
        [Mapping("version")]
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// The name of this version of the element. This will be a 
        /// calculated from of the element name and element version number
        /// for example "twoColumnLayout_v3" and is not stored in the database
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// Optional module name defines how assets are deployed when 
        /// asset deployment is configured as module
        /// </summary>
        [Mapping("moduleName")]
        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        /// <summary>
        /// See the AssetDeployment enumeration. If empty defaults to
        /// inheritance except for pages where it defaults to website
        /// </summary>
        [Mapping("assetDeployment")]
        [JsonProperty("assetDeployment")]
        public AssetDeployment AssetDeployment{ get; set; }
    }
}
