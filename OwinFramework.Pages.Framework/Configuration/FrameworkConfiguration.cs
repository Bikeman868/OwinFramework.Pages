﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.Framework.Configuration
{
    /// <summary>
    /// Defines the configuration options for the Framework assembly
    /// </summary>
    internal class FrameworkConfiguration : IFrameworkConfiguration
    {
        [JsonProperty("defaultLanguage")]
        public string DefaultLanguage { get; private set; }

        [JsonProperty("assetRootPath")]
        public string AssetRootPath { get; private set; }

        [JsonProperty("servicesRootPath")]
        public string ServicesRootPath { get; private set; }

        [JsonProperty("assetCacheTime")]
        public TimeSpan AssetCacheTime { get; private set; }

        [JsonProperty("assetVersion")]
        public string AssetVersion { get; private set; }

        [JsonProperty("debugLogging")]
        public bool DebugLogging { get; private set; }
        
        [JsonProperty("debugLibraries")]
        public bool DebugLibraries{ get; private set; }

        [JsonProperty("templateUrlRootPath")]
        public string TemplateUrlRootPath { get; private set; }

        [JsonProperty("templateRootPath")]
        public string TemplateRootPath { get; private set; }

        [JsonProperty("minifyCss")]
        public bool MinifyCss{ get; private set; }

        [JsonProperty("minifyJavascript")]
        public bool MinifyJavascript{ get; private set; }

        [JsonProperty("htmlFormat")]
        public HtmlFormat HtmlFormat { get; private set; }

        [JsonProperty("indented")]
        public bool Indented { get; private set; }

        [JsonProperty("includeComments")]
        public bool IncludeComments { get; private set; }

        #region Implementation details

        private readonly List<Action<IFrameworkConfiguration>> _subscribers;
        private readonly IDisposable _configChange;

        public FrameworkConfiguration()
        {
            DefaultLanguage = "en-US";
            AssetRootPath = "/assets";
            ServicesRootPath = "/services";
            AssetCacheTime = TimeSpan.FromHours(1);
            AssetVersion = "1";
            DebugLogging = false;
            DebugLibraries = false;
            TemplateUrlRootPath = "/";
            TemplateRootPath = "/";
            HtmlFormat = HtmlFormat.Html;
            Indented = true;
            IncludeComments = true;
        }

        public FrameworkConfiguration(IConfigurationStore configurationStore)
        {
            _subscribers = new List<Action<IFrameworkConfiguration>>();

            _configChange = configurationStore.Register(
                "/owinFramework/pages/framework",
                c =>
                {
                    DefaultLanguage = c.DefaultLanguage;
                    AssetRootPath = c.AssetRootPath;
                    ServicesRootPath = c.ServicesRootPath;
                    AssetCacheTime = c.AssetCacheTime;
                    AssetVersion = c.AssetVersion;
                    DebugLogging = c.DebugLogging;
                    DebugLibraries = c.DebugLibraries;
                    TemplateUrlRootPath = c.TemplateUrlRootPath;
                    TemplateRootPath = c.TemplateRootPath;
                    MinifyCss = c.MinifyCss;
                    MinifyJavascript = c.MinifyJavascript;
                    HtmlFormat = c.HtmlFormat;
                    Indented = c.Indented;
                    IncludeComments = c.IncludeComments;

                    lock (_subscribers)
                        foreach (var subscriber in _subscribers)
                            subscriber(this);
                },
                new FrameworkConfiguration());
        }

        public void Subscribe(Action<IFrameworkConfiguration> action)
        {
            if (action == null) return;
            lock (_subscribers) _subscribers.Add(action);
            action(this);
        }

        #endregion
    }
}
