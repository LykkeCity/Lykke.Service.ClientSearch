using Lykke.Service.ClientSearch.Client.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Client
{
    /// <summary>
    /// Public interface of service for client search
    /// </summary>
    public interface IClientFulltextSearchService
    {
        /// <summary>
        /// Searches for existng clients by fisrt name and last name and date of birth
        /// </summary>
        /// <returns>Collection of client ids</returns>
        Task<IEnumerable<string>> FindExistingClientsAsync(ExistingClientSearchRequest req);
    }
}
