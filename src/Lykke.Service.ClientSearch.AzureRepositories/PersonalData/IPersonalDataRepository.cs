using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.AzureRepositories.PersonalData
{
    public interface IPersonalDataRepository
    {
        Task<PersonalDataEntity> GetAsync(string id);

    }
}
