using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Manager.Configuration
{
    /// <summary>
    /// Defines the configuration options for the Html assembly
    /// </summary>
    internal class ManagerConfiguration
    {
        [JsonProperty("templateBasePath")]
        public string TemplateBasePath { get; set; }

        [JsonProperty("serviceBasePath")]
        public string ServiceBasePath { get; set; }

        [JsonProperty("managerPath")]
        public string ManagerPath { get; set; }

        public const string Path = "/owinFramework/pages/cms/manager";

        /// <summary>
        /// Default public constructor for serialization
        /// </summary>
        public ManagerConfiguration()
        {
            Sanitize();
        }

        /// <summary>
        /// Used to make the configuration valid
        /// </summary>
        public ManagerConfiguration Sanitize()
        {
            TemplateBasePath = FixBasePath(TemplateBasePath, "/cms/manager/");
            ServiceBasePath = FixBasePath(ServiceBasePath, "/cms/api/");
            ManagerPath = FixPath(ManagerPath, "/cms");
            return this;
        }

        #region Implementation details

        private string FixBasePath(string path, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(path)) path = defaultPath;
            if (!path.StartsWith("/")) path = "/" + path;
            if (path.Length > 1 && !path.EndsWith("/")) path = path + "/";
            return path;
        }

        private string FixPath(string path, string defaultPath)
        {
            if (string.IsNullOrEmpty(path)) path = defaultPath;
            if (!path.StartsWith("/")) path = "/" + path;
            if (path.Length > 1 && path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            return path;
        }

        #endregion
    }
}
