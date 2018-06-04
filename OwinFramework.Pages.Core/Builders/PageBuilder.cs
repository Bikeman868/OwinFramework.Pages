using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;

namespace OwinFramework.Pages.Core.Builders
{
    internal class PageBuilder: IPageBuilder
    {
        private readonly IRequestRouter _requestRouter;

        public PageBuilder(
            IRequestRouter requestRouter)
        {
            _requestRouter = requestRouter;
        }

        public IPageDefinition Page(Type declaringType)
        {
            return new PageDefinition(declaringType, _requestRouter);
        }

        private class PageDefinition: IPageDefinition
        {
            private readonly IRequestRouter _requestRouter;
            private readonly Webpage _page = new Webpage();
            private readonly Type _declaringType;

            private IRequestFilter _filter;
            private int _filterPriority;
            private string _path;
            private Methods[] _methods;

            public PageDefinition(
                Type declaringType,
                IRequestRouter requestRouter)
            {
                _declaringType = declaringType;
                _requestRouter = requestRouter;
            }

            IPageDefinition IPageDefinition.Name(string name)
            {
                return this;
            }

            IPageDefinition IPageDefinition.AssetDeployment(AssetDeployment assetDeployment)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Module(IModule module)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Module(string moduleName)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Path(string path)
            {
                _path = path;
                return this;
            }

            IPageDefinition IPageDefinition.Methods(params Methods[] methods)
            {
                _methods = methods;
                return this;
            }

            IPageDefinition IPageDefinition.RequestFilter(IRequestFilter filter, int priority)
            {
                _filter = filter;
                _filterPriority = priority;
                return this;
            }

            IPageDefinition IPageDefinition.Layout(ILayout layout)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Layout(string name)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Component(string regionName, IComponent component)
            {
                return this;
            }

            IPageDefinition IPageDefinition.Component(string regionName, string componentName)
            {
                return this;
            }

            IPageDefinition IPageDefinition.RegionLayout(string regionName, ILayout layout)
            {
                return this;
            }

            IPageDefinition IPageDefinition.RegionLayout(string regionName, string layoutName)
            {
                return this;
            }

            IPage IPageDefinition.Build()
            {
                if (_filter == null)
                {
                    if (_methods == null)
                    {
                        _filter = new FilterByPath(_path);
                    }
                    else
                    {
                        if (_path == null)
                        {
                            _filter = new FilterByMethod(_methods);
                        }
                        else
                        {
                            _filter = new FilterAllFilters(new FilterByMethod(_methods), new FilterByPath(_path));
                        }
                    }
                }

                _requestRouter.Register(_page, _filter, _filterPriority, _declaringType);

                return _page;
            }
        }

        private class Webpage : IPage
        {
            public string RequiredPermission { get; set; }
            public bool AllowAnonymous { get; set; }
            public Func<IOwinContext, bool> AuthenticationFunc { get; set; }

            Task IRunable.Run(IOwinContext context)
            {
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Not implemented yet");
            }
        }

    }
}
