using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using System.Web;

namespace OwinFramework.Pages.Framework.Managers
{
    [Description("Serves requests for static assets. These static assets can be deployed site-wide or in modules.")]
    internal class AssetManager: IAssetManager, IRunable
    {
        private readonly IFrameworkConfiguration _frameworkConfiguration;
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IStringBuilderFactory _stringBuilderFactory;

        public string DefaultLanguage { get { return _frameworkConfiguration.DefaultLanguage; } }

        private readonly HashSet<string> _elementsAddedToWebsite;
        private readonly IThreadSafeDictionary<string, HashSet<string>> _elementsAddedToModule;
        private readonly IThreadSafeDictionary<string, HashSet<string>> _elementsAddedToPage;

        private string _websiteStyles;
        private string _websiteFunctions;
        private readonly IThreadSafeDictionary<string, string> _moduleStyles;
        private readonly IThreadSafeDictionary<string, string> _moduleFunctions;
        private readonly IThreadSafeDictionary<string, string> _pageStyles;
        private readonly IThreadSafeDictionary<string, string> _pageFunctions;

        private readonly IStringBuilder _websiteStylesBuilder;
        private readonly IStringBuilder _websiteFunctionsBuilder;
        private readonly IThreadSafeDictionary<string, IStringBuilder> _moduleStylesBuilder;
        private readonly IThreadSafeDictionary<string, IStringBuilder> _moduleFunctionsBuilder;
        private readonly IThreadSafeDictionary<string, IStringBuilder> _pageStylesBuilder;
        private readonly IThreadSafeDictionary<string, IStringBuilder> _pageFunctionsBuilder;

        private readonly PathString _rootPath;

        public AssetManager(
            IRequestRouter requestRouter,
            IFrameworkConfiguration frameworkConfiguration,
            IHtmlWriterFactory htmlWriterFactory,
            IStringBuilderFactory stringBuilderFactory,
            IDictionaryFactory dictionaryFactory)
        {
            _frameworkConfiguration = frameworkConfiguration;
            _htmlWriterFactory = htmlWriterFactory;
            _stringBuilderFactory = stringBuilderFactory;

            _elementsAddedToWebsite = new HashSet<string>();
            _elementsAddedToModule = dictionaryFactory.Create<string, HashSet<string>>();
            _elementsAddedToPage = dictionaryFactory.Create<string, HashSet<string>>();

            _moduleStyles = dictionaryFactory.Create<string, string>();
            _moduleFunctions = dictionaryFactory.Create<string, string>();
            _pageStyles = dictionaryFactory.Create<string, string>();
            _pageFunctions = dictionaryFactory.Create<string, string>();

            _websiteStylesBuilder = stringBuilderFactory.Create();
            _websiteFunctionsBuilder = stringBuilderFactory.Create();

            _moduleStylesBuilder = dictionaryFactory.Create<string, IStringBuilder>();
            _moduleFunctionsBuilder = dictionaryFactory.Create<string, IStringBuilder>();
            _pageStylesBuilder = dictionaryFactory.Create<string, IStringBuilder>();
            _pageFunctionsBuilder = dictionaryFactory.Create<string, IStringBuilder>();

            var rootPath = frameworkConfiguration.AssetRootPath;
            if (rootPath.EndsWith("/") && rootPath.Length > 1) rootPath = rootPath.Substring(0, rootPath.Length - 1);
            _rootPath = new PathString(rootPath);

            requestRouter.Register(this, 
                new FilterAllFilters(
                    new FilterByMethod(Methods.Get),
                    new FilterByPath(_rootPath.Value + "/**")), 10);
        }

        string IAssetManager.GetSupportedLanguage(string acceptLanguageHeader)
        {
            if (string.IsNullOrEmpty(acceptLanguageHeader))
                return DefaultLanguage;

            var supportedLanguages = acceptLanguageHeader
                .Split(',')
                .Select(s => s.Trim())
                .Where(IsSupportedLanguage)
                .ToList();

            if (supportedLanguages.Count == 0)
                return DefaultLanguage;

            return supportedLanguages.FirstOrDefault(l => l.IndexOf('-') > 1)
                ?? supportedLanguages[0];
        }

        private bool IsSupportedLanguage(string language)
        {
            return language.Length > 0;
        }

        string IAssetManager.GetLocalizedText(IRenderContext renderContext, string assetName, string defaultText)
        {
            // TODO: Localize the text
            return defaultText + " (<i>" + renderContext.Language + "</i>)";
        }

        private string ModuleName(IModule module)
        {
            return module.Name.ToLower();
        }

        private string ElementName(IElement element)
        {
            if (element == null || element.ElementType == ElementType.Unnamed)
                return null;

            if (element.Package == null)
                return (element.ElementType + "_" + element.Name).ToLower();

            return (element.ElementType + "_" + element.Package.NamespaceName + "_" + element.Name).ToLower();
        }

        private string PageName(IPage page)
        {
            if (page == null)
                return null;

            if (page.Package == null)
                return page.Name.ToLower();

            return (page.Package.NamespaceName + "_" + page.Name).ToLower();
        }

        private void GetStaticAssets(IElement element, AssetType assetType, IStringBuilder stringBuilder)
        {
            var writer = _htmlWriterFactory.Create();
            try
            {
                var writeResult = element.WriteStaticAssets(assetType, writer);
                writeResult.Wait();
                writeResult.Dispose();
                writer.ToStringBuilder(stringBuilder);
            }
            finally
            {
                writer.Dispose();
            }
        }

        void IAssetManager.AddWebsiteAssets(IElement element)
        {
            var elementName = ElementName(element);
            if (elementName == null) return;

            var hashSet = _elementsAddedToWebsite;
            lock (hashSet) if (!hashSet.Add(elementName)) return;

            GetStaticAssets(element, AssetType.Style, _websiteStylesBuilder);
            GetStaticAssets(element, AssetType.Script, _websiteFunctionsBuilder);

            _websiteStyles = _websiteStylesBuilder.ToString();
            _websiteFunctions = _websiteFunctionsBuilder.ToString();
        }

        void IAssetManager.AddModuleAssets(IElement element, IModule module)
        {
            var elementName = ElementName(element);
            var moduleName = ModuleName(module);

            if (elementName == null || moduleName == null) 
                return;

            var hashSet = _elementsAddedToModule.GetOrAdd(moduleName, m => new HashSet<string>(), null);
            lock (hashSet) if (!hashSet.Add(elementName)) return;

            var styleBuilder = _moduleStylesBuilder.GetOrAdd(moduleName, m => _stringBuilderFactory.Create(), null);
            lock (styleBuilder) GetStaticAssets(element, AssetType.Style, styleBuilder);

            var functionBuilder = _moduleFunctionsBuilder.GetOrAdd(moduleName, m => _stringBuilderFactory.Create(), null);
            lock (functionBuilder) GetStaticAssets(element, AssetType.Script, functionBuilder);

            _moduleStyles[moduleName] = _websiteStylesBuilder.ToString();
            _moduleFunctions[moduleName] = _websiteFunctionsBuilder.ToString();
        }

        void IAssetManager.AddPageAssets(IElement element, IPage page)
        {
            var elementName = ElementName(element);
            var pageName = PageName(page);

            if (elementName == null || pageName == null)
                return;

            var hashSet = _elementsAddedToPage.GetOrAdd(pageName, p => new HashSet<string>(), null);
            lock (hashSet) if (!hashSet.Add(elementName)) return;

            var styleBuilder = _pageStylesBuilder.GetOrAdd(pageName, m => _stringBuilderFactory.Create(), null);
            lock (styleBuilder) GetStaticAssets(element, AssetType.Style, styleBuilder);

            var functionBuilder = _pageFunctionsBuilder.GetOrAdd(pageName, m => _stringBuilderFactory.Create(), null);
            lock (functionBuilder) GetStaticAssets(element, AssetType.Script, functionBuilder);

            _pageStyles[pageName] = _websiteStylesBuilder.ToString();
            _pageFunctions[pageName] = _websiteFunctionsBuilder.ToString();
        }

        Uri IAssetManager.GetWebsiteAssetUrl(AssetType type)
        {
            if (type == AssetType.Style)
            {
                if (!string.IsNullOrEmpty(_websiteStyles))
                    return new Uri(_rootPath + "/static.css", UriKind.Relative);
            }
            else if (type == AssetType.Script)
            {
                if (!string.IsNullOrEmpty(_websiteFunctions))
                    return new Uri(_rootPath + "/static.js", UriKind.Relative);
            }
            return null;
        }

        Uri IAssetManager.GetModuleAssetUrl(IModule module, AssetType type)
        {
            var moduleName = ModuleName(module);

            if (type == AssetType.Style)
            {
                if (_moduleStyles.ContainsKey(moduleName))
                    return new Uri(_rootPath + "/module/" + moduleName + ".css", UriKind.Relative);
            }
            else if (type == AssetType.Script)
            {
                if (!string.IsNullOrEmpty(_websiteFunctions))
                    return new Uri(_rootPath + "/module/" + moduleName + ".js", UriKind.Relative);
            }
            return null;
        }

        Uri IAssetManager.GetPageAssetUrl(IPage page, AssetType type)
        {
            var pageName = PageName(page);

            if (type == AssetType.Style)
            {
                if (_moduleStyles.ContainsKey(pageName))
                    return new Uri(_rootPath + "/page/" + pageName + ".css", UriKind.Relative);
            }
            else if (type == AssetType.Script)
            {
                if (!string.IsNullOrEmpty(_websiteFunctions))
                    return new Uri(_rootPath + "/page/" + pageName + ".js", UriKind.Relative);
            }
            return null;
        }

        #region IRunable - serves assets

        string IRunable.RequiredPermission { get { return null; } }
        bool IRunable.AllowAnonymous { get { return true; } }
        Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }

        Task IRunable.Run(IOwinContext context)
        {
            if (context.Request.Method != "GET")
                throw new HttpException((int)HttpStatusCode.MethodNotAllowed, 
                    "The " + context.Request.Method + " method is not supported for static assets");

            // Path can be /assets/static.{css|js}
            // Path can be /assets/module/{modulename}.{css|js}
            // Path can be /assets/page/{packagename}-{pagename}.{css|js}

            PathString subPath;
            if (!context.Request.Path.StartsWithSegments(_rootPath, out subPath))
                throw new HttpException((int)HttpStatusCode.NotFound, 
                    "The requested path is not within the root path for assets");

            if (!subPath.HasValue || subPath.Value == "/")
                throw new HttpException((int)HttpStatusCode.NotFound,
                    "No asset was specifed in the path");

            var pathElements = subPath.Value.Split('/');
            var lastPathElement = pathElements[pathElements.Length - 1];

            var extIndex = lastPathElement.LastIndexOf('.');
            if (extIndex < 0)
                throw new HttpException((int)HttpStatusCode.NotFound,
                    "No file extension was present in the request path");

            var name = lastPathElement.Substring(0, extIndex);
            var ext = lastPathElement.Substring(extIndex);

            AssetType assetType;
            if (string.Equals(ext, ".css", StringComparison.OrdinalIgnoreCase)) assetType = AssetType.Style;
            else if (string.Equals(ext, ".js", StringComparison.OrdinalIgnoreCase)) assetType = AssetType.Script;
            else throw new HttpException((int)HttpStatusCode.NotFound,
                "Only css and js files are supported by thie endpoint");

            var secondLastPathElement = pathElements[pathElements.Length - 2];
            string content;

            if (assetType == AssetType.Script)
            {
                context.Response.ContentType = "application/javascript";

                if (string.Equals(secondLastPathElement, "module", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_moduleFunctions.TryGetValue(secondLastPathElement, out content))
                    throw new HttpException((int)HttpStatusCode.NotFound,
                        "Unknown module " + secondLastPathElement);
                }
                else if (string.Equals(secondLastPathElement, "page", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_pageFunctions.TryGetValue(secondLastPathElement, out content))
                    throw new HttpException((int)HttpStatusCode.NotFound,
                        "Unknown page " + secondLastPathElement);
                }
                else
                {
                    content = _websiteFunctions;
                }
            }
            else if (assetType == AssetType.Style)
            {
                context.Response.ContentType = "text/css";

                if (string.Equals(secondLastPathElement, "module", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_moduleStyles.TryGetValue(secondLastPathElement, out content))
                        throw new HttpException((int)HttpStatusCode.NotFound,
                            "Unknown module " + secondLastPathElement);
                }
                else if (string.Equals(secondLastPathElement, "page", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_pageStyles.TryGetValue(secondLastPathElement, out content))
                        throw new HttpException((int)HttpStatusCode.NotFound,
                            "Unknown page " + secondLastPathElement);
                }
                else
                {
                    content = _websiteStyles;
                }
            }
            else throw new HttpException((int)HttpStatusCode.InternalServerError,
                "Somehow the asset type was unknown");

            context.Response.Expires = DateTime.UtcNow + _frameworkConfiguration.AssetCacheTime;
            context.Response.Headers.Set(
                "Cache-Control",
                "public, max-age=" + (int)_frameworkConfiguration.AssetCacheTime.TotalSeconds);

            return context.Response.WriteAsync(content);
        }

        #endregion
    }
}
