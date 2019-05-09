using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    /// <summary>
    /// Transmits changes in website design to other webservers so that
    /// all servers render the website identically
    /// </summary>
    public interface ILiveUpdateSender
    {
        void Send(MessageDto updateMessage);
    }
}
