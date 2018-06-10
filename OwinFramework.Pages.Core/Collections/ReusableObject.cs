using System;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// Base class for objects that implement IReusable. These objects are
    /// pooled and reused when the application disposes of the instance instead
    /// of being actually disposed.
    /// </summary>
    public class ReusableObject : Disposable, IReusable
    {
        /// <summary>
        /// Instances can set this flag to false if they can not be resued,
        /// for example if the configuration changed and they are no longer 
        /// valid
        /// </summary>
        public virtual bool IsReusable { get { return true; } }

        private Action<IReusable> _disposeAction;

        /// <summary>
        /// This method gets called when a reusable object is taken out of
        /// the pool and is about to be reused. In this method the code
        /// should restore the object to the same initial state that it
        /// was in just after construction.
        /// </summary>
        /// <param name="disposeAction">When the object is disposed it should
        /// invoke this action to put itself back into the pool of 
        /// instances that are available for reuse</param>
        protected IReusable Initialize(Action<IReusable> disposeAction)
        {
            _disposeAction = disposeAction;
            IsDisposed = false;
            IsDisposing = false;

            return this;
        }

        /// <summary>
        /// Invokes the dispose action when this instance is disposed. This
        /// puts the object back into the pool for reuse
        /// </summary>
        protected override void Disposed()
        {
            if (_disposeAction != null) 
                _disposeAction(this);
        }
    }
}
