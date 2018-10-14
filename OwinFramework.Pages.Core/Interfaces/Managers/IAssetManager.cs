using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Managers
{
    /// <summary>
    /// The asset manager manages all JaavScript, css, images and text
    /// and provides localization of those things
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// Specifies the default language to use when the asset manager
        /// does not support any of the languages accepted by the browser
        /// </summary>
        string DefaultLanguage { get; }

        /// <summary>
        /// Examines the browser accepted languages and selects the
        /// language for output rendering
        /// </summary>
        /// <param name="acceptLanguageHeader">The Accept-Language header as defined by http</param>
        string GetSupportedLanguage(string acceptLanguageHeader);

        /// <summary>
        /// Returns a localized version of some text
        /// </summary>
        /// <param name="renderContext">The context of the request that this is being rendered for</param>
        /// <param name="assetName">The name of the text asset to localize</param>
        /// <param name="defaultText">The html to return for all unsupported languages</param>
        string GetLocalizedText(IRenderContext renderContext, string assetName, string defaultText);

        /// <summary>
        /// Gets static assets from an element and adds them to the global
        /// website assets. Only adds each element once even if called 
        /// multiple times
        /// </summary>
        /// <param name="element">The element whose static assets are to be
        /// added</param>
        void AddWebsiteAssets(IElement element);

        /// <summary>
        /// Gets static assets from an element and adds them to the global
        /// website assets. Only adds each element once to a given module
        /// even if called multiple times
        /// </summary>
        /// <param name="element">The element whose static assets are to be
        /// added</param>
        /// <param name="module">The module to add these assets to</param>
        void AddModuleAssets(IElement element, IModule module);

        /// <summary>
        /// Gets static assets from an element and adds them to the global
        /// website assets. Only adds each element once to a given page
        /// even if called multiple times
        /// </summary>
        /// <param name="element">The element whose static assets are to be
        /// added</param>
        /// <param name="page">The page to add these assets to</param>
        void AddPageAssets(IElement element, IPage page);

        /// <summary>
        /// Returns the URL at which the asset manager will serve the static assets
        /// that are shared across the whole website
        /// </summary>
        /// <param name="type">The type of assets to get</param>
        Uri GetWebsiteAssetUrl(AssetType type);

        /// <summary>
        /// Returns the URL at which the asset manager will serve the static assets
        /// specific to a module
        /// </summary>
        /// <param name="module">The module to get the assets for</param>
        /// <param name="type">The type of assets to get</param>
        Uri GetModuleAssetUrl(IModule module, AssetType type);

        /// <summary>
        /// Returns the URL at which the asset manager will serve the static assets
        /// specific to a page
        /// </summary>
        /// <param name="page">The page to get the assets for</param>
        /// <param name="type">The type of assets to get</param>
        Uri GetPageAssetUrl(IPage page, AssetType type);
    }
}
