namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// POCO that returns results from a database create record
    /// </summary>
    public class CreateResult: UpdateResult
    {
        /// <summary>
        /// The ID of the newly created record
        /// </summary>
        public long NewRecordId { get; private set; }
        
        /// <summary>
        /// Constructs a success result
        /// </summary>
        public CreateResult(long newRecordId)
        {
            NewRecordId = newRecordId;
        }

        /// <summary>
        /// Constructs a success result
        /// </summary>
        public CreateResult(string userMessageKey, string debugMessage)
            : base(userMessageKey, debugMessage)
        {
        }
    }
}
