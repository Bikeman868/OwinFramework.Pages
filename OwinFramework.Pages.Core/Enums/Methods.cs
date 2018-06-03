namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Specifies an http request method
    /// </summary>
    public enum Methods
    {
        /// <summary>
        /// Request to return headers only with no body
        /// </summary>
        Head,

        /// <summary>
        /// Request to return the resource identified by the Url
        /// </summary>
        Get,

        /// <summary>
        /// Request to create a new resource identified by the Url
        /// </summary>
        Post,

        /// <summary>
        /// Request to replace the resource identified by the Url
        /// </summary>
        Put,

        /// <summary>
        /// Request to delete the resource identified by the Url
        /// </summary>
        Delete,

        /// <summary>
        /// CORS pre-flight security check
        /// </summary>
        Options
    }
}
