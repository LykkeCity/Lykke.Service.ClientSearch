using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.ClientSearch.Core.FullTextSearch;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.ClientSearch.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class ClientFullTextSearchController : Controller
    {
        private const int _serviceNotReadyCode = 503;
        private const string _serviceNotReadyMsg = "Search index is not ready yet";

        private readonly Indexer _indexer;
        private readonly SearcherForExistingClients _searcherForExistingClients;
        private readonly IndexInfo _indexInfo;

        /// <summary>
        /// Controller to fulltext search for clients
        /// </summary>
        public ClientFullTextSearchController(
            Indexer indexer,
            SearcherForExistingClients searcherForExistingClients,
            IndexInfo indexInfo
            )
        {
            _indexer = indexer;
            _searcherForExistingClients = searcherForExistingClients;
            _indexInfo = indexInfo;
        }

        /// <summary>
        /// Returns client id as a result of search by name and date of birth 
        /// </summary>
        [HttpPost]
        [Route("searchForExistingClient")]
        public IActionResult SearchForExistingClient([FromBody] ExistingClientSearchRequest req)
        {
            string errMsg = "Valid client name and date of birth are required";
            if (req == null)
            {
                return BadRequest(errMsg);
            }
            if (!_indexer.IndexCreated)
            {
                return StatusCode(_serviceNotReadyCode, _serviceNotReadyMsg);
            }

            IList<string> result = _searcherForExistingClients.Search(req.Name, req.DateOfBirth);
            if (result == null)
            {
                return BadRequest(errMsg);
            }
            return Json(result);
        }

        /// <summary>
        /// Returns indexed data for a client
        /// </summary>
        [HttpGet]
        [Route("showClientData/{clientId}")]
        public IActionResult showClientData(string clientId)
        {
            string errMsg = "Valid client id is required";
            if (String.IsNullOrWhiteSpace(clientId))
            {
                return BadRequest(errMsg);
            }
            if (!_indexer.IndexCreated)
            {
                return StatusCode(_serviceNotReadyCode, _serviceNotReadyMsg);
            }

            IndexedData result = _indexInfo.GetIndexedData(clientId);
            return Json(result);
        }

    }
}
