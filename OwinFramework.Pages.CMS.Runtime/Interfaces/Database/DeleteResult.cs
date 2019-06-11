namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// POCO that returns results from a database delete
    /// </summary>
    public class DeleteResult: UpdateResult
    {
        /// <summary>
        /// Constructs a success result
        /// </summary>
        public DeleteResult()
        {
        }

        /// <summary>
        /// Constructs a success result
        /// </summary>
        public DeleteResult(string userMessageKey, string debugMessage)
            : base(userMessageKey, debugMessage)
        {
        }
    }
}
