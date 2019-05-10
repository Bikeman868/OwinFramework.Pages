using System;
using Newtonsoft.Json;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.CMS.Editor.Configuration
{
    /// <summary>
    /// Defines the configuration options for the Html assembly
    /// </summary>
    internal class EditorConfiguration
    {
        [JsonProperty("templateBasePath")]
        public string TemplateBasePath { get; set; }

        [JsonProperty("serviceBasePath")]
        public string ServiceBasePath { get; set; }

        [JsonProperty("editorPath")]
        public string EditorPath { get; set; }

        #region Implementation details

        private readonly IDisposable _configChange;

        /// <summary>
        /// Note that this constructor is public for serialization
        /// </summary>
        public EditorConfiguration()
        {
            TemplateBasePath = "/cms/editor/";
            ServiceBasePath = "/cms/api/";
            EditorPath = "/cms";
        }

        public EditorConfiguration(IConfigurationStore configurationStore)
        {
            _configChange = configurationStore.Register(
                "/owinFramework/pages/cms/editor",
                c =>
                {
                    TemplateBasePath = c.TemplateBasePath;
                    ServiceBasePath = c.ServiceBasePath;
                    EditorPath = c.EditorPath;
                },
                new EditorConfiguration());
        }

        #endregion
    }
}
