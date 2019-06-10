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

        public const string ConfigPath = "/owinFramework/pages/html";
        private readonly IDisposable _changeNotifier;

        /// <summary>
        /// This default public constructor is used by the JSON deserializer
        /// </summary>
        public HtmlConfiguration()
        {
            HtmlFormat = HtmlFormat.Html;
            IncludeComments = true;
            Indented = true;
        }

        public HtmlConfiguration(IConfigurationStore configurationStore)
        {
            _changeNotifier = configurationStore.Register(
                ConfigPath,
                c =>
                {
                    c = c.Sanitize();
                    HtmlFormat = c.HtmlFormat;
                    IncludeComments = c.IncludeComments;
                    Indented = c.Indented;
                },
                new HtmlConfiguration());
        }

        public HtmlConfiguration Sanitize()
        {
            return this;
        }

        #endregion
    }
}
