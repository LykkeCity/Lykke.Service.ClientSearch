using System;
using Lykke.Service.ClientSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using System.Net;

namespace Lykke.Service.ClientSearch.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly Indexer _indexer;

        public IsAliveController(Indexer indexer)
        {
            _indexer = indexer;
        }

        /// <summary>
        /// Checks service is alive
        /// </summary>
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        [ProducesResponseType(typeof(IsAliveResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Get()
        {
            if (_indexer.IsIndexReady)
            {
                return Ok(
                    new IsAliveResponse
                        {
                            Name = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationName,
                            Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                            Env = Program.EnvInfo,
                        }
                    );
            }
            else
            {
                return StatusCode(ControllersCommon.ServiceNotReadyCode, ErrorResponse.Create($"Service is unhealthy: {ControllersCommon.ServiceNotReadyMsg}"));
            }
        }

    }
}
