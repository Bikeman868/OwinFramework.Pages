using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Facilities.Managers
{
    internal class AssetManager: IAssetManager
    {
        public string DefaultLanguage { get; set; }

        public AssetManager()
        {
            DefaultLanguage = "en-US";
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
    }
}
