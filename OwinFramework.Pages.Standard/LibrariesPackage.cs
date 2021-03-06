﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Framework.Interfaces;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Standard
{
    public class LibrariesPackage : Framework.Runtime.Package
    {
        private readonly IFrameworkConfiguration _frameworkConfiguration;

        public LibrariesPackage(
            IPackageDependenciesFactory dependencies,
            IFrameworkConfiguration frameworkConfiguration)
            : base(dependencies)
        {
            _frameworkConfiguration = frameworkConfiguration;
            Name = "libraries";
            NamespaceName = "libraries";
        }

        /// <summary>
        /// This component renders a link to a library in the page head
        /// </summary>
        private class LibraryComponent : Component
        {
            private readonly string[] _scriptUrls;
            private readonly string[] _styleUrls;

            public LibraryComponent(
                IComponentDependenciesFactory dependencies, 
                string[] scriptUrls, 
                string[] styleUrls = null)
                : base(dependencies)
            {
                PageAreas = new[] { PageArea.Head };
                _scriptUrls = scriptUrls;
                _styleUrls = styleUrls;
            }

            public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
            {
                if (pageArea == PageArea.Head)
                {
                    if (_styleUrls != null)
                    {
                        foreach (var url in _styleUrls)
                            context.Html.WriteElementLine("link", string.Empty, "rel", "stylesheet", "href", url);
                    }
                    if (_scriptUrls != null)
                    {
                        foreach (var url in _scriptUrls)
                            context.Html.WriteElementLine("script", string.Empty, "src", url);
                    }
                }

                return WriteResult.Continue();
            }
        }

        public override IPackage Build(IFluentBuilder builder)
        {
            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"}))
                .Name("jQuery1")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://ajax.googleapis.com/ajax/libs/jquery/2.2.4/jquery.min.js"}))
                .Name("jQuery2")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"}))
                .Name("jQuery3")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"}))
                .Name("jQuery")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://ajax.googleapis.com/ajax/libs/angularjs/1.7.6/angular.min.js"}))
                .Name("AngularJS")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory,
                    _frameworkConfiguration.DebugLibraries 
                        ? new[]
                        {
                            "https://unpkg.com/react@16/umd/react.development.js", 
                            "https://unpkg.com/react-dom@16/umd/react-dom.development.js"
                        }
                        : new[]
                        {
                            "https://unpkg.com/react@16/umd/react.production.min.js",
                            "https://unpkg.com/react-dom@16/umd/react-dom.production.min.js"
                        }))
                .Name("React")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory, new[]{
                    "https://unpkg.com/redux@4.0.1/dist/redux.js"}))
                .Name("Redux")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory,new[]{
                    _frameworkConfiguration.DebugLibraries 
                        ? "https://cdn.jsdelivr.net/npm/vue/dist/vue.js"
                        : "https://cdn.jsdelivr.net/npm/vue"}))
                .Name("Vue")
                .Build();

            builder.BuildUpComponent(
                new LibraryComponent(
                    Dependencies.ComponentDependenciesFactory,
                    new[]{ "https://unpkg.com/material-components-web@latest/dist/material-components-web.min.js" },
                    new[]
                    {
                        "https://unpkg.com/material-components-web@latest/dist/material-components-web.min.css",
                        "https://fonts.googleapis.com/icon?family=Material+Icons"
                    }))
                .Name("Material")
                .Build();

            return this;
        }
    }
}
