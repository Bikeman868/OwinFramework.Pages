using System.Threading.Tasks;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Specifies the outcome of the writing operation
    /// </summary>
    public interface IWriteResult
    {
        /// <summary>
        /// When this is not null the rendering process will start a new text
        /// output buffer for the next rendering operation and will wait for
        /// this task to complete before flushing all of the buffers to the 
        /// response stream.
        /// When this is null it indicates that the result was written synchronously
        /// and the same text writer can be reused for the next part of the
        /// rendering process.
        /// </summary>
        Task TaskToWaitFor { get; }

        /// <summary>
        /// Writers can set this flag to indicate that they completely wrote
        /// this section of the page and there is no need to process the
        /// request through the remaining elements. The place where this
        /// is used most often is writing the page title. Generally sepeaking
        /// you only want one element to write into the title and when this
        /// is done the rest of the pipeline can be skipped for efficiency.
        /// </summary>
        bool IsComplete { get; }
    }
}
