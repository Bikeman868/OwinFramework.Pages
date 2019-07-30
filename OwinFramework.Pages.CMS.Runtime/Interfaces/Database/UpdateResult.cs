using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// POCO that returns results from a database update
    /// </summary>
    public class UpdateResult
    {
        /// <summary>
        /// True if the update was succesfull
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; private set; }
        
        /// <summary>
        /// If the update failed then this is the key to a localizable
        /// message to display to the user
        /// </summary>
        [JsonProperty("messageKey")]
        public string UserMessageKey { get; private set; }

        /// <summary>
        /// If the update failed this is a message that developers can
        /// use to diagnose any issues with the code
        /// </summary>
        [JsonProperty("message")]
        public string DebugMessage { get; private set; }

        /// <summary>
        /// Constructs a success result
        /// </summary>
        public UpdateResult()
        {
            Success = true;
        }

        /// <summary>
        /// Constructs a success result
        /// </summary>
        public UpdateResult(string userMessageKey, string debugMessage)
        {
            UserMessageKey = userMessageKey;
            DebugMessage = debugMessage;
        }
    }
}
