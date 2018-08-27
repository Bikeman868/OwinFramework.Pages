using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    public class DataContextBuilder
    {
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IRenderContext _renderContext;

        private readonly Stack<IDataContext> _stack = new Stack<IDataContext>();
        private IDataContext _current;

        public DataContextBuilder(
            IDataContextFactory dataContextFactory,
            IRenderContext renderContext)
        {
            _dataContextFactory = dataContextFactory;
            _renderContext = renderContext;
        }

        public IDataContext Current { get { return _current; } }

        public void Push(IDataScopeProvider scope)
        {
            if (_current == null)
            {
                _current = _dataContextFactory.Create(_renderContext, scope);
            }
            else
            {
                _stack.Push(_current);
                _current = _current.CreateChild(scope);
            }
            _renderContext.AddDataContext(scope.Id, _current);
        }

        public void Pop()
        {
            _current = _stack.Count > 0 ? _stack.Pop() : null;
        }
    }
}
