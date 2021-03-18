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

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made to handle those request which are related to Categories table           */
    /*********************************************************************************************/
    [Produces("application/json")]
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly ITicketHandler _ticket;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public UserController(ITicketHandler ticket, ILogger<CategoryController> logger, IConfiguration config)
        {
            _ticket = ticket;
            _logger = logger;
            _configuration = config;
        }

        /// <summary>
        /// This endpoint is repsonsible to register new users
        /// </summary>
        /// <param name="user">User information in JSON: username, email, password</param>
        /// <returns>With Ok or a BadRequest</returns>
        /// <response code="200">User has been created</response>
        /// <response code="400">User creation failed, see error message</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody]UserLoginInfo user)
        {
            User regUser = new User();
            regUser.Username = user.Username;
            regUser.Password = user.Password;
            regUser.Email = user.Email;
            var respond = await _ticket.RegisterUserAsync(regUser);
            if(respond.MessageType == MessageType.NOK)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }
            return Ok();
        }

        /// <summary>
        /// This endpoint is repsonsible to login for all users
        /// </summary>
        /// <param name="user">User information in JSON: username, password</param>
        /// <returns>With Ok or a BadRequest</returns>
        /// <response code="200">Login was successful</response>
        /// <response code="401">Login is failed</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody]UserLoginInfo user)
        {
            // Check that user exist
            var checkUsr = await _ticket.GetUserAsync(user.Username);
            if(checkUsr != null)
            {
                // Check that password hash are the same
                if(TicketHandler.HashPassword(user.Password) == checkUsr.Password)
                {
                    // Get the role and create a token
                    var role = checkUsr.Role.ToString();

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, role)
                    };

                    if(role == "Admin")
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, UserRole.User.ToString()));
                    }
                    
                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddMinutes(5),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }

                ErrorMessage ret = new ErrorMessage();
                ret.Message = "Wrong password";
                return Unauthorized(ret);
            }
            else
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = "User does not exist";
                return Unauthorized(ret);
            }
        }
    }
}
