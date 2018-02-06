﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Flurl.Http;
using System.Net.Http;
using Lykke.Service.ClientSearch.Client.Model;

namespace Lykke.Service.ClientSearch.Client
{
    public class ClientFulltextSearchService : IClientFulltextSearchService
    {
        private readonly string _serviceUrl;
        private readonly ILog _log;

        public ClientFulltextSearchService(string serviceUrl, ILog log)
        {
            _serviceUrl = serviceUrl;
            _log = log;
        }

        public async Task<IEnumerable<string>> FindExistingClients(ExistingClientSearchRequest req)
        {
            return await PostDataAsync<IEnumerable<string>>(req, "searchForExistingClient");
        }

        #region Helpers

        private IFlurlClient GetClient(string action)
        {
            return $"{_serviceUrl}/api/ClientFullTextSearch/{action}".WithHeader("api-key", "");
        }

        private async Task<TResponse> PostDataAsync<TResponse>(object request, string action)
        {
            try
            {
                return await GetClient(action).PostJsonAsync(request).ReceiveJson<TResponse>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(ClientFulltextSearchService), action, request.ToJson(), ex);
                throw;
            }
        }

        #endregion

    }
}
