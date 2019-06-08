using System;
using System.Collections.Generic;
using System.Diagnostics;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageData: IPageData
    {
        private class State
        {
            public AssetDeployment AssetDeployment;
            public IDataContextBuilder DataContextBuilder;
            public IModule Module;
            public string MessagePrefix;

            public State Clone()
            {
                return new State
                {
                    AssetDeployment = AssetDeployment,
                    DataContextBuilder = DataContextBuilder,
                    Module = Module,
                    MessagePrefix = MessagePrefix + "  "
                };
            }
        }

        public class ElementRegistration
        {
            public IElement Element;
            public AssetDeployment AssetDeployment;
            public IModule Module;
        }

        public readonly List<ElementRegistration> Elements = new List<ElementRegistration>();
        public IDataContextBuilder RootDataContextBuilder { get; private set; }

        private readonly Stack<State> _stateStack = new Stack<State>();
        private readonly Page _page;
        private State _currentState;

        public PageData(IPageDependenciesFactory dependencies, Page page)
        {
            _page = page;

            var assetDeployment = page.AssetDeployment;
            if (page.Module != null)
                assetDeployment = page.Module.AssetDeployment;

            if (assetDeployment == AssetDeployment.Inherit)
                assetDeployment = AssetDeployment.PerModule;

            if (assetDeployment == AssetDeployment.PerModule && page.Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            RootDataContextBuilder = dependencies.DataContextBuilderFactory.Create(page);

            _currentState = new State
            {
                MessagePrefix = page.Name + ": ",
                AssetDeployment = assetDeployment,
                DataContextBuilder = RootDataContextBuilder,
                Module = page.Module
            };
        }

        /// <summary>
        /// This method is called from element constructors to notify the page of an element
        /// on the page. These constructors will typically construct other elements which will
        /// also call this method, hence these calls are made in a nested heirachy
        /// </summary>
        public IDataContextBuilder BeginAddElement(IElement element, IDataScopeRules dataScopeRules)
        {
            // Data scope rules define the Type+Scope combinations that should be resolved at this
            // level and also the suppliers/supplies that were configured by that application to
            // be injected into the data context.
            dataScopeRules = dataScopeRules ?? element as IDataScopeRules;

            Log("Adding element '" + element + "' to page" + 
                (dataScopeRules == null ? ""  : " using data scope rules from '" + dataScopeRules + "'"));

            // Figure out how the assets will be deployed for this element. Elements can inherit from
            // their parent element, follow the module that are in or have an explicit value set.
            var assetDeployment = element.AssetDeployment;

            if (element.Module != null)
                assetDeployment = element.Module.AssetDeployment;

            var module = element.Module ?? _currentState.Module;

            if (assetDeployment == AssetDeployment.Inherit)
                assetDeployment = _currentState.AssetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            Log(element + " asset deployment resolved to '" + assetDeployment + 
                (assetDeployment == AssetDeployment.PerModule ? "' in module '" + module + "'" : "'"));

            // The page keeps a list of the elements on the page so that it can extract the static
            // assets later and build css and js files for each module etc
            Elements.Add(new ElementRegistration
            {
                Element = element,
                AssetDeployment = assetDeployment,
                Module = module
            });

            // If this element depends on a library component then this component needs to be
            // added to the page.
            var libraryConsumer = element as ILibraryConsumer;
            if (!ReferenceEquals(libraryConsumer, null))
            {
                var dependentComponents = libraryConsumer.GetDependentComponents();
                if (!ReferenceEquals(dependentComponents, null))
                {
                    foreach (var component in dependentComponents)
                    {
                        Log(element + " element needs " + component);
                        _page.AddComponent(component);
                    }
                }
            }

            // Elements are in a tree structure and inherit from thier parents, so we need
            // to keep a state stack and push/pop as we traverse the tree to keep track of
            // the proper state for each stage of the processing
            Log("Pushing new state onto stack");
            _stateStack.Push(_currentState);
            _currentState = _currentState.Clone();

            // If this element introdices a new scope (basically regions do this) then
            // we need to create a new scope for resolving data needs and update the
            // current state
            if (dataScopeRules != null)
            {
                Log("Adding data context builder based on " + dataScopeRules);
                _currentState.DataContextBuilder = _currentState.DataContextBuilder.AddChild(dataScopeRules);
            }

            // If this element needs specifc data then add it to the data context builder.
            var dataConsumer = element as IDataConsumer;
            if (dataConsumer != null)
            {
                Log(element + " element is a data consumer, adding its needs");
                _currentState.DataContextBuilder.AddConsumer(dataConsumer);
            }

            return _currentState.DataContextBuilder;
        }

        /// <summary>
        /// This method is called from element constructors after they have finished
        /// constructng their child elements
        /// </summary>
        public void EndAddElement(IElement element)
        {
            Log("Added element " + element + " to " + _page.Name);
            Log("Poping state off the stack");
            _currentState = _stateStack.Pop();
        }

        public void Log(string message)
        {
#if DEBUG
            if (string.IsNullOrEmpty(message)) return;
            Trace.WriteLine(_currentState.MessagePrefix + message);
#endif
        }
    }
}