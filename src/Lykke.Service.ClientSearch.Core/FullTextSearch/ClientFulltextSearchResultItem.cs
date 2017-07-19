using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Core.FullTextSearch
{
    public class ClientFulltextSearchResultItem
    {
        public String Name { get; set; }
        public String Address { get; set; }

        public IList<ClientFulltextSearchResultBackOfficeItem> BackOfficeResultItems { get; set; }
    }
}
