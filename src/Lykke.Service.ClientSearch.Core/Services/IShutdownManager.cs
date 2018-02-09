using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}