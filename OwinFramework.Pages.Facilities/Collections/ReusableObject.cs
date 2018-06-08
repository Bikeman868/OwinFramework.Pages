using System;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Base class for objects that implement IReusable. These objects are
    /// pooled and reused when the application disposes of the instance instead
    /// of being actually disposed.
    /// </summary>
    public class ReusableObject : Disposable, IReusable
    {
        public virtual bool IsReusable { get { return true; } }

        private Action<IReusable> _disposeAction;

        protected IReusable Initialize(Action<IReusable> disposeAction)
        {
            _disposeAction = disposeAction;
            IsDisposed = false;
            IsDisposing = false;

            return this;
        }

        protected override void Disposed()
        {
            if (_disposeAction != null) 
                _disposeAction(this);
        }
    }
}
