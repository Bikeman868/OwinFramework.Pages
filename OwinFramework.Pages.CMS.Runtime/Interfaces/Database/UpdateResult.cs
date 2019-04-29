using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool Success { get; private set; }
        
        /// <summary>
        /// If the update failed then this is the key to a localizable
        /// message to display to the user
        /// </summary>
        public string UserMessageKey { get; private set; }

        /// <summary>
        /// If the update failed this is a message that developers can
        /// use to diagnose any issues with the code
        /// </summary>
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
