using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Client
{
    public class ClientFulltextSearchResultItem
    {
        public String Name { get; set; } = "";
        public String Address { get; set; } = "";
        public String Message { get; set; } = "";
        public String Amount { get; set; } = "";
        public Double AmountNum { get; set; }
        public String TransactionDate { get; set; } = "";
        public String TransactionId { get; set; } = "";
        public String TransactionClientId { get; set; } = "";
        public int IdForClassName { get; set; }
        public bool Processed { get; set; } = false;
        public String BankStatementItemId { get; set; }

        public IList<ClientFulltextSearchResultBackOfficeItem> BackOfficeResultItems { get; set; }
    }
}
