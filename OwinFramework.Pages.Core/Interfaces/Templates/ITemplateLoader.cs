using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Defines a class that retrieves templates from a source (folder
    /// structure, database, web service etc) parses these templates
    /// and adds them to the template manager.
    /// </summary>
    public interface ITemplateLoader
    {
        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this page is
        /// part of</param>
        ITemplateLoader PartOf(IPackage package);

        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// page is part of</param>
        ITemplateLoader PartOf(string packageName);

        /// <summary>
        /// Any assets created from loaded templates will be deployed to this module
        /// </summary>
        /// <param name="module">The module to deploy to</param>
        ITemplateLoader DeployIn(IModule module);

        /// <summary>
        /// Any assets created from loaded templates will be deployed to this module
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        ITemplateLoader DeployIn(string moduleName);
        
        /// <summary>
        /// Specifies how assets created from loaded templates will be deployed
        /// </summary>
        ITemplateLoader DeployAssetsTo(AssetDeployment assetDeployment);

        /// <summary>
        /// Loads templates from the template source and parses them using
        /// the supplied parser then registers them with the template manager
        /// </summary>
        /// <param name="parser">The parser to use to construct the ITemplate
        /// instance from the template file</param>
        /// <param name="includeSubPaths">Pass true to load templates from all
        /// sub-paths recursively</param>
        /// <param name="predicate">Optional function to decide which templates 
        /// to load. By default all templates are loaded. When provided, the 
        /// function will be passed the template path. When loading from a 
        /// URI this will be the path of the URI. When loading from the file 
        /// system the file path of the template is converted into URI syntax to 
        /// avoid the application having to account for path separator differences</param>
        /// <param name="mapPath">An optional function to map the template
        /// location string to a template path string. The template path is what
        /// is used throughout the application to refer to templates. The
        /// template location is the path element from the URI that the
        /// template was loaded from. If no function is provided a default
        /// behaviour is dependent on the loader, for example the file system
        /// loader removes the file extension.</param>
        ITemplateLoader Load(
            ITemplateParser parser,
            Func<PathString, bool> predicate = null,
            Func<PathString, string> mapPath = null,
            bool includeSubPaths = true);

        /// <summary>
        /// Instructs the template to repeatedly load templates and parse them
        /// again if they have changed
        /// </summary>
        ITemplateLoader ReloadEvery(TimeSpan interval);

        /// <summary>
        /// Places loaded templates under a sub-path rather that being directly
        /// under the root.
        /// </summary>
        ITemplateLoader LoadUnder(PathString rootPath);

        /// <summary>
        /// Defines the root path to load templates under. This allows you to load 
        /// templates from different sources that have overlapping names and separate
        /// them but putting them under different root paths.
        /// </summary>
        PathString RootPath { get; }

        /// <summary>
        /// Sets the package to use for name resolution. Fully qualified names do
        /// not need a package definition, but when names are only partially
        /// specified the package namespace will be searched as well as the global
        /// namespace.
        /// </summary>
        IPackage Package { get; }

        /// <summary>
        /// Sets the module to use for deploying static assets. If the module is not
        /// set then the assets will be deployed to the website global asset resources
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// Gets the asset deployment location for templates loaded by this loader
        /// </summary>
        AssetDeployment AssetDeployment { get; }

        /// <summary>
        /// Setting this property makes the template loader reload templates at
        /// timed intervals. Note that setting this property means tha the loader
        /// must keep track of the templates that were loaded and the parser that
        /// was used for each template.
        /// Consider using an alternative approach where your application periodically
        /// calls the template loader with appropriate parameters.
        /// </summary>
        TimeSpan? ReloadInterval { get; }
    }
}
