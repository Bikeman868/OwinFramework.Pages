using System;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// Base class for objects that implement IDisposable
    /// </summary>
    public class Disposable : IDisposable
    {
        /// <summary>
        /// Implements IDisposable
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// This flag is set when executing inside the Dispose method
        /// </summary>
        public bool IsDisposing { get; protected set; }

        /// <summary>
        /// Implementes IDisposable
        /// </summary>
        public void Dispose()
        {
            DoDispose(false);
        }

        /// <summary>
        /// Override this method to define the disposable behaviour of
        /// your class.
        /// </summary>
        /// <param name="destructor">True when called by the garbage collector 
        /// and false when called by the application. Note that when the garbage
        /// collector calls method the objects that this instance has
        /// references to could already be disposed</param>
        protected virtual void Dispose(bool destructor)
        {
        }

        /// <summary>
        /// If your class has a finalizer you can call this from the finalizer
        /// passing true. Do not call this in any other circumstances
        /// </summary>
        protected void DoDispose(bool destructor)
        {
            if (IsDisposing || IsDisposed) return;

            IsDisposing = true;
            try
            {
                Dispose(destructor);
            }
            finally
            {
                IsDisposed = true;

                if (!destructor)
                    Disposed();
            }
        }

        /// <summary>
        /// Override this if there is something you need to do afer the
        /// object has been disposed
        /// </summary>
        protected virtual void Disposed()
        {
        }
    }
}
