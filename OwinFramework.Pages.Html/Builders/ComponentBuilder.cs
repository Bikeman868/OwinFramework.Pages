using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
        }

        IComponentDefinition IComponentBuilder.Component()
        {
            return new ComponentDefinition(_nameManager, _assetManager);
        }

        private class ComponentDefinition: IComponentDefinition
        {
            private readonly INameManager _nameManager;
            private readonly IAssetManager _assetManager;
            private readonly BuiltComponent _component;

            public ComponentDefinition(
                INameManager nameManager,
                IAssetManager assetManager)
            {
                _nameManager = nameManager;
                _assetManager = assetManager;
                _component = new BuiltComponent();
            }

            IComponentDefinition IComponentDefinition.Name(string name)
            {
                _component.Name = name;
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(IPackage package)
            {
                _component.Package = package;
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(string packageName)
            {
                _component.Package = _nameManager.ResolvePackage(packageName);

                if (_component.Package == null)
                    throw new ComponentBuilderException(
                        "Package names must be registered before components can refer to them. " +
                        "There is no registered package '" + packageName + "'");
                return this;
            }

            IComponentDefinition IComponentDefinition.AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                _component.AssetDeployment = assetDeployment;
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo<T>()
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo(Type dataType)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.DataContext(string dataContextName)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.DeployIn(IModule module)
            {
                _component.Module = module;
                return this;
            }

            IComponentDefinition IComponentDefinition.DeployIn(string moduleName)
            {
                _nameManager.AddResolutionHandler(() =>
                    {
                        _component.Module = _nameManager.ResolveModule(moduleName);
                    });
                return this;
            }

            public IComponent Build()
            {
                _nameManager.Register(_component);
                return _component;
            }

            public IComponentDefinition Render(string assetName, string defaultHtml)
            {
                if (_component.Writers == null)
                    _component.Writers = new List<Action<IRenderContext>>();

                _component.Writers.Add(rc => 
                    {
                        var localozedHtml = _assetManager.GetLocalizedText(rc, assetName, defaultHtml);
                        rc.Html.WriteLine(localozedHtml);
                    });

                return this;
            }
        }

        private class BuiltComponent: Component
        {
            public List<Action<IRenderContext>> Writers;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                if (renderContext.IncludeComments)
                    renderContext.Html.WriteComment(
                        (string.IsNullOrEmpty(Name) ? "Unnamed" : Name) + 
                        (Package == null ? " component" : " component from the " + Package.Name + " package"));

                if (Writers != null)
                {
                    foreach (var writer in Writers)
                        writer(renderContext);
                }

                return WriteResult.Continue();
            }
        }
    }
}
