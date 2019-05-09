using System;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// Represents an object that wants to know when there are changes in the
    /// website design. Dispose of this instance to unsubscribe to these 
    /// changes permenantly
    /// </summary>
    public interface ILiveUpdateRecipient: IDisposable
    {
        /// <summary>
        /// Set to the method to call when changes notifications are received.
        /// Set this to null to temporarily stop notifications. When you set
        /// this property to null and notifications will be permenantly lost
        /// </summary>
        Action<MessageDto> OnMessageReceived { get; set; }
    }
}
