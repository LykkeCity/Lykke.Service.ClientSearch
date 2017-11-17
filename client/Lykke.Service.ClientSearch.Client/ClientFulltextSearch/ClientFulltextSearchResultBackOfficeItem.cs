using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Client
{
    public class ClientFulltextSearchResultBackOfficeItem
    {
        public String ClientId { get; set; }
        public String BackOfficeName { get; set; }
        public String BackOfficeAddress { get; set; }
        public bool AlreadyProcessed { get; set; }
        public bool ThisClientDepositProcessed { get; set; }
        public bool IsPreselectedForStatementClient { get; set; }
        public String IdClassName { get; set; }
        public String AccumulatedDeposit { get; set; } = "";
        public double AccumulatedDepositNum { get; set; } = 0;
        public String DepositLimit { get; set; } = "";
        public double DepositLimitNum { get; set; } = 0;

        public String Score { get; set; }

        public ClientSources ClientSource = ClientSources.None;

    }
}
