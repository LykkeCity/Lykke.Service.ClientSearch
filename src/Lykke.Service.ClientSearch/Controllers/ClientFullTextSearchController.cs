using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.ClientSearch.Core.FullTextSearch;
using Lykke.Service.ClientSearch.Services.FullTextSearch;

namespace Lykke.Service.ClientSearch.Controllers
{
    /// <summary>
    /// Controller for personal data
    /// </summary>
    //[Authorize]

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

    }
}
