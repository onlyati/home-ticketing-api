using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HomeTicketing.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DatabaseController.Interface;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made for health check of API                                                 */
    /*********************************************************************************************/
    [Route("health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly ITicketHandler _ticket;
        private readonly ILogger _logger;

        public HealthController(ITicketHandler ticket, ILogger<HealthController> logger)
        {
            _ticket = ticket;
            _logger = logger;
        }

        /// <summary>
        /// Check that API is alive or not
        /// </summary>
        /// <remarks>
        /// This request is good to check that API is alive
        /// </remarks>
        /// <returns>Alive message</returns>
        /// <response code="200">API is alive</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            GeneralMessage OkMsg = new GeneralMessage();
            OkMsg.Message = "I am still alive";
            _logger.LogInformation("Health checking was successfully");
            return Ok(OkMsg);
        }
    }
}
