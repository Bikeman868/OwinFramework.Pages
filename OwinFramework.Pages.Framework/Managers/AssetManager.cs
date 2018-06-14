using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class AssetManager: IAssetManager
    {
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IStringBuilderFactory _stringBuilderFactory;

        public string DefaultLanguage { get; set; }

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

        public AssetManager(
            IHtmlWriterFactory htmlWriterFactory,
            IStringBuilderFactory stringBuilderFactory,
            IDictionaryFactory dictionaryFactory)
        {
            _htmlWriterFactory = htmlWriterFactory;
            _stringBuilderFactory = stringBuilderFactory;
            DefaultLanguage = "en-US";

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
            if (element == null) return;

            var elementName = element.GetType().Name + "." + element.Name;
            elementName = element.Package == null ? elementName : element.Package.NamespaceName + ":" + elementName;

            var hashSet = _elementsAddedToWebsite;
            lock (hashSet) if (!hashSet.Add(elementName)) return;

            GetStaticAssets(element, AssetType.Style, _websiteStylesBuilder);
            GetStaticAssets(element, AssetType.Script, _websiteFunctionsBuilder);

            _websiteStyles = _websiteStylesBuilder.ToString();
            _websiteFunctions = _websiteFunctionsBuilder.ToString();
        }

        void IAssetManager.AddModuleAssets(IElement element, IModule module)
        {
            if (element == null || module == null) return;

            var elementName = element.GetType().Name + "." + element.Name;
            elementName = element.Package == null ? elementName : element.Package.NamespaceName + ":" + elementName;

            var moduleName = module.Name;

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
            if (element == null || page == null) return;

            var elementName = element.GetType().Name + "." + element.Name;
            elementName = element.Package == null ? elementName : element.Package.NamespaceName + ":" + elementName;

            var pageName = page.Package == null ? page.Name : page.Package.NamespaceName + ":" + page.Name;

            var hashSet = _elementsAddedToPage.GetOrAdd(pageName, p => new HashSet<string>(), null);
            lock (hashSet) if (!hashSet.Add(elementName)) return;

            var styleBuilder = _pageStylesBuilder.GetOrAdd(pageName, m => _stringBuilderFactory.Create(), null);
            lock (styleBuilder) GetStaticAssets(element, AssetType.Style, styleBuilder);

            var functionBuilder = _pageFunctionsBuilder.GetOrAdd(pageName, m => _stringBuilderFactory.Create(), null);
            lock (functionBuilder) GetStaticAssets(element, AssetType.Script, functionBuilder);

            _pageStyles[pageName] = _websiteStylesBuilder.ToString();
            _pageFunctions[pageName] = _websiteFunctionsBuilder.ToString();
        }

        System.Uri IAssetManager.GetWebsiteAssetUrl(AssetType type)
        {
            throw new System.NotImplementedException();
        }

        System.Uri IAssetManager.GetModuleAssetUrl(IModule module, AssetType type)
        {
            throw new System.NotImplementedException();
        }

        System.Uri IAssetManager.GetPageAssetUrl(IPage page, AssetType type)
        {
            throw new System.NotImplementedException();
        }
    }
}
