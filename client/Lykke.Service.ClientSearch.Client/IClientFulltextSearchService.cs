using Lykke.Service.ClientSearch.Client.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Client
{
    public interface IClientFulltextSearchService
    {
        /// <summary>
        /// Searches for existng clients by fisrt name and last name and date of birth
        /// </summary>
        /// <returns>Collection of client ids</returns>
        Task<IEnumerable<string>> FindExistingClients(ExistingClientSearchRequest req);
    }
}
