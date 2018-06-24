using System.Threading;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class IdManager: IIdManager
    {
        private int _priorId;

        int IIdManager.GetUniqueId()
        {
            return Interlocked.Increment(ref _priorId);
        }
    }
}
