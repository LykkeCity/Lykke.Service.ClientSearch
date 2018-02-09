using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.ClientSearch.Controllers
{
    static internal class ControllersCommon
    {
        internal const int ServiceNotReadyCode = 503;
        internal const string ServiceNotReadyMsg = "Search index is not ready yet";
    }
}
