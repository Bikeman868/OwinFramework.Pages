using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prius.Contracts.Attributes;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all element versions.
    /// When new versions are created all elements from the current element 
    /// versions are duplicated into the new version. The new version can then
    /// be edited, validated and then made live.
    /// </summary>
    public class ElementVersionRecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element version in the database.
        /// </summary>
        [Mapping("elementVersionId")]
        public long Id { get; set; }

        /// <summary>
        /// The unique ID of the version that this is part of. 
        /// </summary>
        [Mapping("versionId")]
        public long VersionId { get; set; }

        /// <summary>
        /// The unique ID of the element that this is a version of.
        /// </summary>
        [Mapping("elementId")]
        public long ElementId { get; set; }

        /// <summary>
        /// Set this to false to temporarily remove this version of the element 
        /// from the website
        /// </summary>
        [Mapping("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Optional package name defines the namespace of the element name
        /// </summary>
        [Mapping("packageName")]
        public string PackageName { get; set; }

        /// <summary>
        /// Optional module name defines how assets are deployed when 
        /// asset deployment is configured as module
        /// </summary>
        [Mapping("moduleName")]
        public string ModuleName { get; set; }

        /// <summary>
        /// See the AssetDeployment enumeration. If empty defaults to
        /// inheritance except for pages where it defaults to website
        /// </summary>
        [Mapping("assetDeployment")]
        public AssetDeployment AssetDeployment{ get; set; }
    }
}
