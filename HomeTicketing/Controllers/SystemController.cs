using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeTicketing.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DatabaseController.Model;
using DatabaseController.Interface;
using DatabaseController.Controller;
using DatabaseController.DataModel;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made to handle those request which are related to Categories table           */
    /*********************************************************************************************/
    [Produces("application/json")]
    [Route("system")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public SystemController(IDbHandler ticket, ILogger<CategoryController> logger, IConfiguration config)
        {
            _dbHandler = ticket;
            _logger = logger;
            _configuration = config;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Endpoint functions                                                                    */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// This endpoint is for listing all defined systems
        /// </summary>
        /// <returns></returns>
        /// <response code="200">System listing successfully</response>
        /// <response code="400">System listing failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/all")]
        public async Task<IActionResult> GetAllSystem()
        {
            var response = await _dbHandler.GetSystemsAsync();
            if(response == null)
            {
                return BadRequest(new GeneralMessage() { Message = "System listing failed" });
            }
            return Ok(response);
        }

        /// <summary>
        /// This endpoint is for adding new system into the tool
        /// </summary>
        /// <param name="name">New system's name</param>
        /// <returns></returns>
        /// <response code="200">System creation successfully</response>
        /// <response code="400">System creation failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSystem(string name = null)
        {
            // Check that input is provided
            if(name == null)
            {
                return BadRequest(new GeneralMessage() { Message = "New system name is not specified" });
            }

            // Do the request
            var response = await _dbHandler.AddSystemAsync(name);
            if(response.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = response.MessageText });
            }

            return Ok(new GeneralMessage() { Message = response.MessageText });
        }

        /// <summary>
        /// This endpoint is for deleting system
        /// </summary>
        /// <param name="name">System's name</param>
        /// <returns></returns>
        /// <response code="200">System deletion successfully</response>
        /// <response code="400">System deletion failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpDelete("remove")]
        public async Task<IActionResult> DeleteSystem(string name = null)
        {
            // Check if input is provided
            if(name == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Missing system name" });
            }

            // Do the action
            var response = await _dbHandler.RemoveSystemAsync(name);
            if(response.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = response.MessageText });
            }

            return Ok(new GeneralMessage() { Message = response.MessageText });
        }

        /// <summary>
        /// This endpoint is for renaming system
        /// </summary>
        /// <param name="name">System's name</param>
        /// <param name="newName">New name of the system</param>
        /// <returns></returns>
        /// <response code="200">System rename was successfully</response>
        /// <response code="400">System rename was failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("rename")]
        public async Task<IActionResult> RenameSystem(string name = null, string newName = null)
        {
            // Check that input is provided
            if(name == null || newName == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Input data is missing" });
            }

            // Do the action
            var response = await _dbHandler.RenameSystemAsync(name, newName);
            if(response.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = response.MessageText });
            }

            return Ok(new GeneralMessage() { Message = response.MessageText });
        }
    }
}
