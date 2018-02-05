﻿using System;
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
        /// <summary>
        /// Controller to fulltext search for clients
        /// </summary>
        public ClientFullTextSearchController()
        {
        }

        /*
        /// <summary>
        /// fulltext search for clients
        /// </summary>
        [HttpPost]
        [Route("search")]
        public IEnumerable<ClientFulltextSearchResultItem> Search([FromBody] IList<ClientFulltextSearchRequestItem> requestItems)
        {
            return Searcher.Search(requestItems);
        }
        */

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
            IList<string> result = SearcherForExistingClients.Search(req.Name, req.DateOfBirth);
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
            IndexedData result = IndexInfo.GetIndexedData(clientId);
            return Json(result);
        }

    }
}