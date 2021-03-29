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
    [Produces("application/json")]
    [Route("ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public TicketController(IDbHandler ticket, ILogger<CategoryController> logger, IConfiguration config)
        {
            _dbHandler = ticket;
            _logger = logger;
            _configuration = config;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Endpoint functions                                                                    */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// Endpoint for listing ticket entries
        /// </summary>
        /// <param name="category">Category for filtering</param>
        /// <param name="system">System name for filtering</param>
        /// <param name="status">Status for filtering</param>
        /// <param name="reference">Reference value for filtering</param>
        /// <param name="title">Title for filtering</param>
        /// <param name="skip">How much entry you want skip?</param>
        /// <param name="count">How much entry you want get?</param>
        /// <param name="unassigned">Do you want list unassigned tickets?</param>
        /// <param name="username">Do you want list tickets which owned by someone?</param>
        /// <returns></returns>
        /// <response code="200">List has been sent back</response>
        /// <response code="400">Ticket listing has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/filter")]
        public async Task<IActionResult> ListTickets(string username = null, bool unassigned = false, string category = null, string system = null, string status = null, string reference = null, string title = null, int skip = -1, int count = -1)
        {
            TicketFilterTemplate filter = new TicketFilterTemplate();
            if (!string.IsNullOrEmpty(category)) filter.Category = category;
            if (!string.IsNullOrEmpty(reference)) filter.Reference = reference;
            if (!string.IsNullOrEmpty(status)) filter.Status = status;
            if (!string.IsNullOrEmpty(system)) filter.System = system;
            if (!string.IsNullOrEmpty(title)) filter.Title = title;

            User user = null;
            if (username != null)
                user = await _dbHandler.GetUserAsync(username);

            if (unassigned)
                user = null;

            List<Ticket> respond;
            if (skip != -1 && count != -1)
            {
                if(username == null && !unassigned)
                    respond = await _dbHandler.ListTicketsAsync(skip, count, filter);
                else
                    respond = await _dbHandler.ListTicketsAsync(skip, count, filter, user);
            }
            else
            {
                if (username == null && !unassigned)
                    respond = await _dbHandler.ListTicketsAsync(filter);
                else
                    respond = await _dbHandler.ListTicketsAsync(filter, user);
            }

            if(respond == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Ticket listing has failed" });
            }

            return Ok(respond);
        }


        /// <summary>
        /// Endpoint to get every details (header + logs) about a ticket
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <returns></returns>
        /// <response code="200">List has been sent back</response>
        /// <response code="400">Ticket listing has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("details")]
        public async Task<IActionResult> GetTicketDetails(int id)
        {
            var respond = await _dbHandler.GetDetailsAsync(id);
            if (respond == null)
                return BadRequest(new GeneralMessage() { Message = "Get details is failed" });
            return Ok(respond);
        }

        /// <summary>
        /// Endpoint to request ticket close action
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <param name="reference">Reference value</param>
        /// <param name="system">System name where the reference value needs to be found</param>
        /// <returns></returns>
        /// <response code="200">Ticket closed</response>
        /// <response code="400">Ticket close has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("close")]
        public async Task<IActionResult> CloseTicket(int id = -1, string reference = null, string system = null)
        {
            // Get user name who wants to get info
            var re = Request;
            var headers = re.Headers;
            var tokenString = headers["Authorization"];

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString[0].Split(' ')[1]);

            var claims = token.Claims;
            var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
            var user = await _dbHandler.GetUserAsync(usernameClaim.Value);

            if (user == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User does not exist" });
            }

            // Close the ticket 
            if (id != -1)
            {
                // Get ticket, return with BadRequest of requestor can't close the ticket
                var ticket = await _dbHandler.GetTicketAsync(id);
                if(ticket == null)
                    return BadRequest(new GeneralMessage() { Message = "Ticket does not exist" });

                if (ticket.UserId != user.Id && user.Role != UserRole.Admin)
                    return BadRequest(new GeneralMessage() { Message = "Only Admin or ticket owner can close ticket" });

                // Close the ticket
                var respond = await _dbHandler.CloseTicketAsync(id, user);
                if (respond.MessageType == MessageType.NOK)
                    return BadRequest(new GeneralMessage() { Message = respond.MessageText});

                return Ok(new GeneralMessage() { Message = respond.MessageText });
            }
            else if (reference != null && system != null)
            {
                // Get ticket, return with BadRequest of requestor can't close the ticket
                var ticketList = await _dbHandler.GetTicketAsync(reference, system);
                if (ticketList == null)
                    return BadRequest(new GeneralMessage() { Message = "Ticket does not exist" });

                var ticket = ticketList.FirstOrDefault(s => s.Status.Equals("Open"));
                if (ticket == null)
                    return BadRequest(new GeneralMessage() { Message = "Ticket does not exist" });

                if (ticket.UserId != user.Id && user.Role != UserRole.Admin)
                    return BadRequest(new GeneralMessage() { Message = "Only Admin or ticket owner can close ticket" });

                // Close the ticket
                var respond = await _dbHandler.CloseTicketAsync(ticket.Id, user);
                if (respond.MessageType == MessageType.NOK)
                    return BadRequest(new GeneralMessage() { Message = respond.MessageText });

                return Ok(new GeneralMessage() { Message = respond.MessageText });
            }
            else
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid input parameter. Specify ID or Reference value and system name" });
            }
        }

        /// <summary>
        /// This endpoint provide interface for opening tickets
        /// </summary>
        /// <param name="info">JSON about ticket details</param>
        /// <returns></returns>
        /// <response code="200">Ticet created</response>
        /// <response code="400">Ticket creation has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket(TicketCreationInput info)
        {
            var sys = await _dbHandler.GetSystemAsync(info.System);
            if (sys == null)
                return BadRequest(new GeneralMessage() { Message = "Invalid system name" });

            var cat = await _dbHandler.GetCategoryAsync(info.CategoryName, sys);
            if (cat == null)
                return BadRequest(new GeneralMessage() { Message = "Invalid category" });

            TicketCreationTemplate template = new TicketCreationTemplate();
            template.Category = cat;
            template.Details = info.Details;
            template.Reference = info.Reference;
            template.Summary = info.Summary;
            template.Title = info.Title;

            if (info.Details == null)
                template.Details = info.Summary;

            // Get user who wants remove user
            var re = Request;
            var headers = re.Headers;
            var tokenString = headers["Authorization"];

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString[0].Split(' ')[1]);

            var claims = token.Claims;
            var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
            var user = await _dbHandler.GetUserAsync(usernameClaim.Value);

            template.CreatorUser = user;

            var respond = await _dbHandler.CreateTicketAsync(template);
            if(respond.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });
            }
            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }

        /// <summary>
        /// This endpoint is responsible for change meta data modifications
        /// </summary>
        /// <param name="input">JSON object about the new values</param>
        /// <returns></returns>
        /// <response code="200">Ticet changed</response>
        /// <response code="400">Ticket change has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("change")]
        public async Task<IActionResult> ChangeTicket([FromBody]TicketChangeInput input = null)
        {
            // Check that every input is there
            if(input == null)
                return BadRequest(new GeneralMessage() { Message = "Missing input data" });

            Category cat = null;
            if (!string.IsNullOrEmpty(input.System) && !string.IsNullOrWhiteSpace(input.System) && !string.IsNullOrEmpty(input.CategoryName) && !string.IsNullOrWhiteSpace(input.CategoryName))
            {
                var sys = await _dbHandler.GetSystemAsync(input.System);
                if (sys == null)
                    return BadRequest(new GeneralMessage() { Message = "Invalid system is defined" });

                cat = await _dbHandler.GetCategoryAsync(input.CategoryName, sys);
                if (cat == null)
                    return BadRequest(new GeneralMessage() { Message = "Invalid category is defined" });
            }

            input.Category = cat;

            // Get user who wants remove user
            var re = Request;
            var headers = re.Headers;
            var tokenString = headers["Authorization"];

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString[0].Split(' ')[1]);

            var claims = token.Claims;
            var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
            var user = await _dbHandler.GetUserAsync(usernameClaim.Value);

            input.ChangederUser = user;

            var respond = await _dbHandler.ChangeTicketAsync(input);
            if(respond.MessageType == MessageType.NOK)
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });

            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }

        /// <summary>
        /// This endpoint responsible for ticket assignment
        /// </summary>
        /// <param name="ticketid">Which ticket needs to be assigned?</param>
        /// <param name="username">Where the ticket should be assigned?</param>
        /// <returns></returns>
        /// <response code="200">Ticet changed</response>
        /// <response code="400">Ticket change has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("assign")]
        public async Task<IActionResult> AssignTicket(int ticketid = -1, string username = null)
        {
            if (ticketid == -1 || username == null)
                return BadRequest(new GeneralMessage() { Message = "Missing input: ticket ID and username are mandatory" });

            var ticket = await _dbHandler.GetTicketAsync(ticketid);
            var user = await _dbHandler.GetUserAsync(username);

            var response = await _dbHandler.AssignUserToTicketAsync(user, ticket);
            if (response.MessageType == MessageType.NOK)
                return BadRequest(new GeneralMessage() { Message = response.MessageText });

            return Ok(new GeneralMessage() { Message = response.MessageText });
        }

        /// <summary>
        /// This endpoint is responsible for unsaaigment of tickets
        /// </summary>
        /// <param name="ticketid">Which ticket needs to be unassigned?</param>
        /// <returns></returns>
        /// <response code="200">Ticet changed</response>
        /// <response code="400">Ticket change has failed</response>
        /// <response code="401">Authorization failed</response>
        /// <response code="403">Authorization failed</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("unassign")]
        public async Task<IActionResult> UnassignTicket(int ticketid = -1)
        {
            if (ticketid == -1)
                return BadRequest(new GeneralMessage() { Message = "Missing ticket ID as input" });

            // Get user who wants remove user
            var re = Request;
            var headers = re.Headers;
            var tokenString = headers["Authorization"];

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString[0].Split(' ')[1]);

            var claims = token.Claims;
            var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
            var user = await _dbHandler.GetUserAsync(usernameClaim.Value);
            if (user == null)
                return Unauthorized(new GeneralMessage() { Message = "User does not exist" });

            // Get the ticket
            var ticket = await _dbHandler.GetTicketAsync(ticketid);
            if (ticket == null)
                return BadRequest(new GeneralMessage() { Message = "Ticket does not exist" });

            var respond = await _dbHandler.UnassignUserFromTicketAsync(user, ticket);
            if (respond.MessageType == MessageType.NOK)
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });

            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }
    }
}
