using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Core.FullTextSearch
{
    public class ClientFulltextSearchRequestItem
    {
        public int OrderNumber { get; set; } = 0;
        public String AssetId { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
    }
}
