using System;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    /// <summary>
    /// Receives changes in website design from other webservers so that
    /// all servers render the website identically
    /// </summary>
    public interface ILiveUpdateReceiver: IDisposable
    {
        ILiveUpdateRecipient Subscribe(Action<MessageDto> onMessageReceived = null);
    }
}
