using System;

namespace OwinFramework.Pages.Framework.Interfaces
{
    public interface IFrameworkConfiguration
    {
        /// <summary>
        /// The URL path on the website where assets should be served, for example '/assets'
        /// </summary>
        string AssetRootPath { get; }

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
        /// Adds an action to perform when the framework configuration changes
        /// </summary>
        /// <param name="action"></param>
        void Subscribe(Action<IFrameworkConfiguration> action);
    }
}
