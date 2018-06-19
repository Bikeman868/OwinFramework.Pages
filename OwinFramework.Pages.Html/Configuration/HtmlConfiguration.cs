using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Html.Interfaces;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.Html.Configuration
{
    /// <summary>
    /// Defines the configuration options for the Html assembly
    /// </summary>
    internal class HtmlConfiguration : IHtmlConfiguration
    {
        public HtmlFormat HtmlFormat { get; private set; }
        public bool IncludeComments { get; private set; }
        public bool Indented { get; private set; }

        #region Implementation details

        private readonly IDisposable _configChange;

        public HtmlConfiguration()
        {
            HtmlFormat = HtmlFormat.Html;
            IncludeComments = true;
            Indented = true;
        }

        public HtmlConfiguration(IConfigurationStore configurationStore)
        {
            _configChange = configurationStore.Register(
                "/owinFramework/pages/html",
                c =>
                {
                    HtmlFormat = c.HtmlFormat;
                    IncludeComments = c.IncludeComments;
                    Indented = c.Indented;
                },
                new HtmlConfiguration());
        }

        #endregion
    }
}
