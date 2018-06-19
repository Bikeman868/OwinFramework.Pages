using System;
using OwinFramework.Pages.Framework.Interfaces;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.Framework.Configuration
{
    /// <summary>
    /// Defines the cnfiguration options for the Framework assembly
    /// </summary>
    public class FrameworkConfiguration : IFrameworkConfiguration
    {
        public string DefaultLanguage { get; private set; }
        public string AssetRootPath { get; private set; }
        public TimeSpan AssetCacheTime { get; private set; }

        #region Implementation details

        private readonly IDisposable _configChange;

        public FrameworkConfiguration()
        {
            DefaultLanguage = "en-US";
            AssetRootPath = "/assets";
            AssetCacheTime = TimeSpan.FromHours(1);
        }

        public FrameworkConfiguration(IConfigurationStore configurationStore)
        {
            _configChange = configurationStore.Register(
                "/owinFramework/pages/framework",
                c =>
                {
                    DefaultLanguage = c.DefaultLanguage;
                    AssetRootPath = c.AssetRootPath;
                    AssetCacheTime = c.AssetCacheTime;
                },
                new FrameworkConfiguration());
        }

        #endregion
    }
}
