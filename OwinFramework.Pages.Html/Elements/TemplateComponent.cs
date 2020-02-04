using System;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is a derivative of Component that renders a set of templates. It can
    /// render the body of a template to a specific region of the page, or write
    /// a single template to multiple parts of the page for templates that support
    /// this.
    /// </summary>
    public class TemplateComponent : Component
    {
        private string _multiPartTemplatePath;
        private string _headTemplatePath;
        private string _scriptTemplatePath;
        private string _styleTemplatePath;
        private string _bodyTemplatePath;
        private string _initializationTemplatePath;

        public TemplateComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            if (childDepth != 0)
            {
                debugComponent.Children = new List<DebugInfo>();

                if (!string.IsNullOrEmpty(_multiPartTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Multi-part template at " + _multiPartTemplatePath,
                            Type = "Template"
                        });
                }

                if (!string.IsNullOrEmpty(_headTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Head template at " + _headTemplatePath,
                            Type = "Template"
                        });
                }

                if (!string.IsNullOrEmpty(_scriptTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Script template at " + _scriptTemplatePath,
                            Type = "Template"
                        });
                }

                if (!string.IsNullOrEmpty(_styleTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Style template at " + _styleTemplatePath,
                            Type = "Template"
                        });
                }

                if (!string.IsNullOrEmpty(_bodyTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Body template at " + _bodyTemplatePath,
                            Type = "Template"
                        });
                }

                if (!string.IsNullOrEmpty(_initializationTemplatePath))
                {
                    debugComponent.Children.Add(
                        new DebugInfo
                        {
                            Name = "Initialization template at " + _initializationTemplatePath,
                            Type = "Template"
                        });
                }
            }

            return base.PopulateDebugInfo<T>(debugComponent, parentDepth, childDepth);
        }

        protected override string GetCommentName()
        {
            string name;

            if (string.IsNullOrEmpty(_multiPartTemplatePath))
            {
                var first = true;
                name = "template set '";

                Action<string, string> add = (n,v) =>
                {
                    if (!string.IsNullOrEmpty(v))
                    {
                        if (first)
                            first = false;
                        else
                            name += ",";
                        name += n + '=' + v;
                    }
                };

                add("head", _headTemplatePath);
                add("script", _scriptTemplatePath);
                add("style", _styleTemplatePath);
                add("body", _bodyTemplatePath);
                add("init", _initializationTemplatePath);
                name += "'";
            }
            else
            {
                name = "multi-part template '" + _multiPartTemplatePath + "'";
            }

            if (Package != null)
                name += " from the '" + Package.Name + "' package";

            return name;
        }

        /// <summary>
        /// Specifies a template to render in all parts of the page
        /// </summary>
        public void MultiPartTemplate(string templatePath)
        {
            _multiPartTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            HeadWriters = new Action<IRenderContext>[] { RenderMultiPartHead };
            ScriptWriters = new Action<IRenderContext>[] { RenderMultiPartScript };
            StyleWriters = new Action<IRenderContext>[] { RenderMultiPartStyle };
            BodyWriters = new Action<IRenderContext>[] { RenderMultiPartBody };
            InitializationWriters = new Action<IRenderContext>[] { RenderMultiPartInitialization };
        }

        /// <summary>
        /// Specifies the path to the template to render in the head of the page
        /// </summary>
        public void HeadTemplate(string templatePath)
        {
            _headTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            HeadWriters = new Action<IRenderContext>[] { RenderHeadTemplate };
        }

        /// <summary>
        /// Specifies the path to the template to render in the scripts part of the page
        /// The template contents will be wrapped in script open/close tags in accordance
        /// with the configured HTML specification to apply.
        /// </summary>
        public void ScriptTemplate(string templatePath)
        {
            _scriptTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            ScriptWriters = new Action<IRenderContext>[] { RenderScriptTemplate };
        }

        /// <summary>
        /// Specifies the path to the template to render in the styles part of the page
        /// The template contents will be wrapped in style open/close tags.
        /// </summary>
        public void StyleTemplate(string templatePath)
        {
            _styleTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            StyleWriters = new Action<IRenderContext>[] { RenderStyleTemplate };
        }

        /// <summary>
        /// Specifies the path to the template to render in the body of the page
        /// </summary>
        public void BodyTemplate(string templatePath)
        {
            _bodyTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.CreateInstances,
                () => CheckForMultiPartTemplate());

            BodyWriters = new Action<IRenderContext>[] { RenderBodyTemplate };
        }

        /// <summary>
        /// Specifies the path to the template to render in the body of the page
        /// </summary>
        public void InitializationTemplate(string templatePath)
        {
            _initializationTemplatePath = templatePath;

            Dependencies.NameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm => AddTemplateDependencies(nm, templatePath));

            InitializationWriters = new Action<IRenderContext>[] { RenderInitializationTemplate };
        }

        private bool _multiPartTemplateChecked = false;

        /// <summary>
        /// Checks if the application developer registered a multi-part template
        /// as a single area template and converts it if necessary
        /// </summary>
        private void CheckForMultiPartTemplate()
        {
            if (_multiPartTemplateChecked) return;
            _multiPartTemplateChecked = true;

            if (!string.IsNullOrEmpty(_multiPartTemplatePath))
                return;

            string templatePath = null;
            var areaCount = 0;

            if (!string.IsNullOrEmpty(_headTemplatePath))
            {
                templatePath = _headTemplatePath;
                areaCount++;
            }
            if (!string.IsNullOrEmpty(_scriptTemplatePath))
            {
                templatePath = _scriptTemplatePath;
                areaCount++;
            }
            if (!string.IsNullOrEmpty(_styleTemplatePath))
            {
                templatePath = _styleTemplatePath;
                areaCount++;
            }
            if (!string.IsNullOrEmpty(_bodyTemplatePath))
            {
                templatePath = _bodyTemplatePath;
                areaCount++;
            }
            if (!string.IsNullOrEmpty(_initializationTemplatePath))
            {
                templatePath = _initializationTemplatePath;
                areaCount++;
            }

            if (areaCount != 1)
                return;

            var template = Dependencies.NameManager.ResolveTemplate(templatePath);
            if (template == null) return;

            var pageAreas = template.GetPageAreas().ToArray();
            if (pageAreas.Length < 2)
                return;

            _multiPartTemplatePath = templatePath;

            HeadWriters = new Action<IRenderContext>[] { RenderMultiPartHead };
            ScriptWriters = new Action<IRenderContext>[] { RenderMultiPartScript };
            StyleWriters = new Action<IRenderContext>[] { RenderMultiPartStyle };
            BodyWriters = new Action<IRenderContext>[] { RenderMultiPartBody };
            InitializationWriters = new Action<IRenderContext>[] { RenderMultiPartInitialization };

            PageAreas = pageAreas;
        }

        private void AddTemplateDependencies(INameManager nm, string templatePath)
        {
            var template = nm.ResolveTemplate(templatePath);

            if (JavascriptFunctions == null)
                JavascriptFunctions = new Action<IJavascriptWriter>[] { w => template.WriteJavascript(w) };
            else
                JavascriptFunctions = JavascriptFunctions.Concat(new Action<IJavascriptWriter>[] { w => template.WriteJavascript(w) }).ToArray();

            if (CssRules == null)
                CssRules = new Action<ICssWriter>[] { w => template.WriteCss(w) };
            else
                CssRules = CssRules.Concat(new Action<ICssWriter>[] { w => template.WriteCss(w) }).ToArray();

            var dataConsumer = template as IDataConsumer;
            if (dataConsumer == null) return;

            // Note that if you change the template and dynamically reload it
            // whilst the website is running you can not change the data dependencies

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

        #region Rendering parts of multi-part templates

        private void RenderMultiPartHead(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_multiPartTemplatePath);
            if (template != null)
            {
                template.WritePageArea(renderContext, PageArea.Head);

                if (template.IsStatic)
                {
                    HeadWriters = new Action<IRenderContext>[] 
                    { 
                        rc => template.WritePageArea(rc, PageArea.Head) 
                    };
                }
            }
        }

        private void WriteScriptTemplate(
            IRenderContext renderContext, 
            ITemplate template, 
            PageArea pageArea)
        {
            renderContext.Html.WriteScriptOpen();
            renderContext.Html.WriteLine();

            template.WritePageArea(renderContext, pageArea);

            renderContext.Html.WriteScriptClose();
            renderContext.Html.WriteLine();
        }

        private void WriteStyleTemplate(
            IRenderContext renderContext,
            ITemplate template,
            PageArea pageArea)
        {
            renderContext.Html.WriteOpenTag("style");
            renderContext.Html.WriteLine();

            template.WritePageArea(renderContext, pageArea);

            renderContext.Html.WriteCloseTag("style");
            renderContext.Html.WriteLine();
        }

        private void RenderMultiPartScript(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_multiPartTemplatePath);
            if (template != null)
            {
                WriteScriptTemplate(renderContext, template, PageArea.Scripts);

                if (template.IsStatic)
                {
                    ScriptWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteScriptTemplate(rc, template, PageArea.Scripts)
                    };
                }
            }
        }

        private void RenderMultiPartStyle(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_multiPartTemplatePath);
            if (template != null)
            {
                WriteStyleTemplate(renderContext, template, PageArea.Styles);

                if (template.IsStatic)
                {
                    StyleWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteStyleTemplate(rc, template, PageArea.Styles)
                    };
                }
            }
        }

        private void RenderMultiPartBody(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_multiPartTemplatePath);
            if (template != null)
            {
                template.WritePageArea(renderContext, PageArea.Body);

                if (template.IsStatic)
                {
                    BodyWriters = new Action<IRenderContext>[] 
                    { 
                        rc => template.WritePageArea(rc, PageArea.Body)
                    };
                }
            }
        }

        private void RenderMultiPartInitialization(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_multiPartTemplatePath);
            if (template != null)
            {
                WriteScriptTemplate(renderContext, template, PageArea.Initialization);

                if (template.IsStatic)
                {
                    InitializationWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteScriptTemplate(rc, template, PageArea.Initialization)
                    };
                }
            }
        }
        
        #endregion

        #region Rendering the template body to various parts of the page

        private void RenderHeadTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_headTemplatePath);
            if (template != null)
            {
                template.WritePageArea(renderContext, PageArea.Body);

                if (template.IsStatic)
                {
                    HeadWriters = new Action<IRenderContext>[] 
                    { 
                        rc => template.WritePageArea(rc, PageArea.Body) 
                    };
                }
            }
        }

        private void RenderScriptTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_scriptTemplatePath);
            if (template != null)
            {
                WriteScriptTemplate(renderContext, template, PageArea.Body);

                if (template.IsStatic)
                {
                    ScriptWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteScriptTemplate(rc, template, PageArea.Body)
                    };
                }
            }
        }

        private void RenderStyleTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_styleTemplatePath);
            if (template != null)
            {
                WriteStyleTemplate(renderContext, template, PageArea.Body);

                if (template.IsStatic)
                {
                    StyleWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteStyleTemplate(rc, template, PageArea.Body)
                    };
                }
            }
        }

        private void RenderBodyTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_bodyTemplatePath);
            if (template != null)
            {
                template.WritePageArea(renderContext, PageArea.Body);

                if (template.IsStatic)
                {
                    BodyWriters = new Action<IRenderContext>[] 
                    { 
                        rc => template.WritePageArea(rc, PageArea.Body)
                    };
                }
            }
        }

        private void RenderInitializationTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.NameManager.ResolveTemplate(_initializationTemplatePath);
            if (template != null)
            {
                WriteScriptTemplate(renderContext, template, PageArea.Body);

                if (template.IsStatic)
                {
                    InitializationWriters = new Action<IRenderContext>[] 
                    { 
                        rc => WriteScriptTemplate(rc, template, PageArea.Body)
                    };
                }
            }
        }

        #endregion
    }
}
