using System;
using Common.Log;

namespace Lykke.Service.ClientSearch.Client
{
    public class ClientSearchClient : IClientSearchClient, IDisposable
    {
        private readonly ILog _log;

        public ClientSearchClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
