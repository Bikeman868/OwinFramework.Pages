using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is a derivative of Component that renders a template.
    /// </summary>
    public class TemplateComponent : Component
    {
        private string _templatePath;
        private ITemplate _template;

        public TemplateComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<PageArea> GetPageAreas()
        {
            return new[] { PageArea.Body };
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            if (childDepth != 0 && _template != null)
            {
                debugComponent.Children = new List<DebugInfo> 
                { 
                    new DebugInfo
                    {
                        Name = _templatePath,
                        Instance = _template,
                        Type = "Template"
                    }
                };
            }

            return base.PopulateDebugInfo<T>(debugComponent, parentDepth, childDepth);
        }

        protected override string GetCommentName()
        {
            return
                "'" + _templatePath + "' template" +
                (Package == null ? string.Empty : " from the '" + Package.Name + "' package");
        }

        /// <summary>
        /// Specifies the path to the template to render
        /// </summary>
        public void Template(string templatePath)
        {
            _templatePath = templatePath;
            Name = templatePath;

            Dependencies.NameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, 
                nm =>
                {
                    _template = nm.ResolveTemplate(_templatePath);

                    var dataConsumer = _template as IDataConsumer;
                    if (dataConsumer != null)
                    {
                        var needs = dataConsumer.GetConsumerNeeds();
                        if (needs == null)
                            return;

                        var thisDataConsumer = this as IDataConsumer;
                        if (needs.DataDependencies != null)
                        {
                            foreach (var dependency in needs.DataDependencies)
                                thisDataConsumer.HasDependency(dependency.DataType, dependency.ScopeName);
                        }

                        if (needs.DataSupplyDependencies != null)
                        {
                            foreach (var dataSupply in needs.DataSupplyDependencies)
                                thisDataConsumer.HasDependency(dataSupply);
                        }

                        if (needs.DataSupplierDependencies != null)
                        {
                            foreach (var dataSupplier in needs.DataSupplierDependencies)
                                thisDataConsumer.HasDependency(dataSupplier.Item1, dataSupplier.Item2);
                        }
                    }
                });

            HtmlWriters = new Action<IRenderContext>[] { RenderTemplate };
        }

        private void RenderTemplate(IRenderContext renderContext)
        {
            _template.WritePageArea(renderContext, PageArea.Body);
        }
    }
}
