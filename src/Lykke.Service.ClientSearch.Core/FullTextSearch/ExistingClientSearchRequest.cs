using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Core.FullTextSearch
{
    public class ExistingClientSearchRequest
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
