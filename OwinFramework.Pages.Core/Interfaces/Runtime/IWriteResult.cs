using System;
using System.Threading.Tasks;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Specifies the outcome of the writing operation
    /// </summary>
    public interface IWriteResult
    {
        /// <summary>
        /// Writers can set this flag to indicate that they completely wrote
        /// this section of the page and there is no need to process the
        /// request through the remaining elements. The place where this
        /// is used most often is writing the page title. Generally sepeaking
        /// you only want one element to write into the title and when this
        /// is done the rest of the pipeline can be skipped for efficiency.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Adds the result of writing operations together creating an aggregate
        /// </summary>
        IWriteResult Add(IWriteResult priorWriteResult);

        /// <summary>
        /// Blocks the current task until all pending write tasks have finished writing
        /// </summary>
        /// <param name="cancel">Pass true to cancel any tasks that can be cancelled</param>
        void Wait(bool cancel = false);
    }
}
