using System;
using Newtonsoft.Json;
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
        [JsonProperty("htmlFormat")]
        public HtmlFormat HtmlFormat { get; set; }

        [JsonProperty("includeComments")]
        public bool IncludeComments { get; set; }

        [JsonProperty("indented")]
        public bool Indented { get; set; }

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
