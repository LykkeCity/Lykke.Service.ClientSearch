using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Core.FullTextSearch
{
    public class ClientFulltextSearchResultBackOfficeItem
    {
        public String ClientId { get; set; }
        public String AssetId { get; set; }
        public String BackOfficeName { get; set; }
        public String BackOfficeAddress { get; set; }
    }
}
