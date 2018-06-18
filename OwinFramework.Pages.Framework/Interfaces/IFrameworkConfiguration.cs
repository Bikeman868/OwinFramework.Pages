using System;
namespace OwinFramework.Pages.Framework
{
    interface IFrameworkConfiguration
    {
        string AssetRootPath { get; }
        string DefaultLanguage { get; }
        TimeSpan AssetCacheTime { get; }
    }
}
