using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// There are several phases to name resolution as follows
    /// </summary>
    public enum NameResolutionPhase
    {
        /// <summary>
        /// The first step is to resolve the names of packages into
        /// the package instance. This has to be the first step because
        /// the package defines the namespace and this is needed to
        /// resolve other name refrences
        /// </summary>
        ResolvePackageNames = 0,

        /// <summary>
        /// This step is where elements that are part of a package have their
        /// fully qualified name registered with the name manager
        /// </summary>
        RegisterPackagedElements = 1,

        /// <summary>
        /// The next step is where elements refer to each other by name.
        /// The name can be qualified by a namespace. An example of this is
        /// where a region is configured to contain a named component. During
        /// this phase the name will be translated into a reference to the
        /// actual component
        /// </summary>
        ResolveElementReferences = 2,

        /// <summary>
        /// In this phase names of css classes and javascript functions are
        /// resolved by replacing the namespace placeholder with the actual
        /// namespace of the element that defines the asset
        /// </summary>
        ResolveAssetNames = 3,

        /// <summary>
        /// This final phase does not actually resolve any names but rather
        /// provides a place where elements can perform initialization tasks
        /// that require all of the name resolution steps to have been completed
        /// first. An example of this is where layouts create RegionInstance
        /// instances for each regin in the layout so that each layout can
        /// have different content in the same region.
        /// </summary>
        CreateInstances = 4,

        /// <summary>
        /// During this phase the pages and services perform initialization 
        /// tasks that can be done once at startup, making the pages and
        /// services handle requests more efficiently. An example of this
        /// is pre-calculating the data needs of each page and constructing
        /// a list od data suppliers that are needed to supply the data for this
        /// page
        /// </summary>
        InitializeRunables = 5
    }
}
