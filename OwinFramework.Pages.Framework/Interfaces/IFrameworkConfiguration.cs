using System;

namespace OwinFramework.Pages.Framework.Interfaces
{
    interface IFrameworkConfiguration
    {
        string AssetRootPath { get; }
        string DefaultLanguage { get; }
        TimeSpan AssetCacheTime { get; }
    }
}
