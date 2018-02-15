using Autofac;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
