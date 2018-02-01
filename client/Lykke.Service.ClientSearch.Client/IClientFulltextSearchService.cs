using Lykke.Service.ClientSearch.Client.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Client
{
    public interface IClientFulltextSearchService
    {
        Task<IEnumerable<ClientFulltextSearchResultItem>> FindMatchingClients(IEnumerable<ClientFulltextSearchRequestItem> dataToFullTextSearch);
        Task<IEnumerable<string>> FindExistingClients(ExistingClientSearchRequest req);
    }
}
