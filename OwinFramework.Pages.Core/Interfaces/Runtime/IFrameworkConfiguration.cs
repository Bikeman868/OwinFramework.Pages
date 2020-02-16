using OwinFramework.Pages.Core.Enums;
using System;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    public interface IFrameworkConfiguration
    {
        /// <summary>
        /// The URL path on the website where assets should be served, for example '/assets'
        /// </summary>
        string AssetRootPath { get; }

        /// <summary>
        /// The URL path on the website where services should be served, for example '/service'
        /// </summary>
        string ServicesRootPath { get; }

        /// <summary>
        /// If the user agent is only requesting languages we do not support or is not specifying
        /// which languages it wants then use this language by default. For example 'en-US'
        /// </summary>
        string DefaultLanguage { get; }

        /// <summary>
        /// This is how long the browser will be instructed to cache css and js assets
        /// </summary>
        TimeSpan AssetCacheTime { get; }
        
        /// <summary>
        /// This will be added into the URL of assets to bust the cache on deployments.
        /// </summary>
        string AssetVersion { get; }

        /// <summary>
        /// Turn this on to see detailed trace output when diagnosing issues with your website.
        /// Turn this off always in your production environment
        /// </summary>
        bool DebugLogging { get; }

        /// <summary>
        /// Turn this on to reference debug versions of JavaScript libraries.
        /// </summary>
        bool DebugLibraries { get; }

        /// <summary>
        /// The root url of pages that are mapped onto templates
        /// </summary>
        string TemplateUrlRootPath { get; }

        /// <summary>
        /// The root template path for url mapped pages
        /// </summary>
        string TemplateRootPath { get; }

        /// <summary>
        /// True if CSS should be minified. Minified CSS is smaller
        /// and faster to parse by the browser but less readable for dubugging
        /// </summary>
        bool MinifyCss { get; }

        /// <summary>
        /// True if Javascript should be minified. Minified Javascript is smaller
        /// and faster to parse by the browser but less readable for dubugging
        /// </summary>
        bool MinifyJavascript { get; }

        /// <summary>
        /// Defines the Html standards to apply to the Html produced
        /// </summary>
        HtmlFormat HtmlFormat { get; }

        /// <summary>
        /// Turns indentation on/off. The html is more readable with
        /// indentation turned on but the output is bigger because of 
        /// all the extra spaces
        /// </summary>
        bool Indented { get; }

        /// <summary>
        /// Turns comments on/off. The comments are good for debugging 
        /// issues but the html can be a lot larger
        /// </summary>
        bool IncludeComments { get; }

        /// <summary>
        /// Adds an action to perform when the framework configuration changes
        /// </summary>
        void Subscribe(Action<IFrameworkConfiguration> action);
    }
}
