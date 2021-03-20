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
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public UserController(IDbHandler ticket, ILogger<CategoryController> logger, IConfiguration config)
        {
            _dbHandler = ticket;
            _logger = logger;
            _configuration = config;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Endpoint functions                                                                    */
        /*---------------------------------------------------------------------------------------*/
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
            // Create new user entry
            User regUser = new User();
            regUser.Username = user.Username;
            regUser.Password = user.Password;
            regUser.Email = user.Email;

            // Register the user
            var respond = await _dbHandler.RegisterUserAsync(regUser);
            if(respond.MessageType == MessageType.NOK)
            {
                // Send bad back if something wrong
                _logger.LogDebug($"New user registration is failed: {user.Username}, {user.Email}");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }
            _logger.LogDebug($"New user has been registered: {user.Username}, {user.Email}");
            return Ok(new GeneralMessage() { Message = "User has been registered" });
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
            if(user == null)
            {
                // Login failed
                _logger.LogDebug($"Login failed for {user.Username} due missing login credentials");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = $"Login failed for {user.Username} due missing login credentials";
                return Unauthorized(ret);
            }

            var checkUsr = await _dbHandler.GetUserAsync(user.Username);
            if(checkUsr != null)
            {
                // Check that password hash are the same
                if(DbHandler.HashPassword(user.Password) == checkUsr.Password)
                {
                    // Get the role
                    var role = checkUsr.Role.ToString();

                    // Create claims
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
                    
                    // Create authentication token
                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.UtcNow.AddMinutes(5),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    // Create refresh token
                    var authSigningKey2 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:RefSec"]));

                    var token2 = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.UtcNow.AddMinutes(2),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey2, SecurityAlgorithms.HmacSha256)
                        );

                    // Return with the token
                    _logger.LogDebug($"Login done for {user.Username}");
                    return Ok(new
                    {
                        auth_token = new JwtSecurityTokenHandler().WriteToken(token),
                        refresh_token = new JwtSecurityTokenHandler().WriteToken(token2),
                        auth_expiration = token.ValidTo,
                        refresh_expiration = token2.ValidTo
                    });
                }

                // Login failed
                _logger.LogDebug($"Login failed for {user.Username} due to wrong password");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = "Wrong password";
                return Unauthorized(ret);
            }
            else
            {
                // Login failed
                _logger.LogDebug($"Login failed for {user.Username} due to user does not exist");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = "User does not exist";
                return Unauthorized(ret);
            }
        }

        /// <summary>
        /// Refrsh authentication token
        /// </summary>
        /// <param name="token">Tokens from JSON</param>
        /// <returns></returns>
        /// <response code="200">No need for creating new token</response>
        /// <response code="201">New token was created</response>
        /// <response code="401">Authentication token has expired</response>
        /// <response code="404">User did not found from token</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody]Tokens token)
        {
            // Decode the tokens
            var handler = new JwtSecurityTokenHandler();
            var authToken = handler.ReadJwtToken(token.AuthToken);
            var refreshToken = handler.ReadJwtToken(token.RefreshToken);

            // Check that authentication token has expired already
            DateTime authExpire = authToken.ValidTo;
            if(authExpire < DateTime.UtcNow)
            {
                // If it does, then return by unauthoize
                GeneralMessage ret = new GeneralMessage();
                ret.Message = "Authentication token has expired";
                return Unauthorized(ret);
            }

            // Check that refresh token has expired
            DateTime refreshExpire = refreshToken.ValidTo;
            if(refreshExpire < DateTime.UtcNow)
            {
                // Create new tokens and return with them
                var claims = authToken.Claims;
                var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
                var user = await _dbHandler.GetUserAsync(usernameClaim.Value);

                // User did not found, return with 404
                if(user == null)
                {
                    return NotFound(new GeneralMessage() { Message = "User did not found from token" });
                }

                // Everything looks good, create new tokens and return with them
                var role = user.Role.ToString();
                var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, role)
                    };

                if (role == "Admin")
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, UserRole.User.ToString()));
                }

                // Create authentication token
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token1 = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                // Create refresh token
                var authSigningKey2 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:RefSec"]));

                var token2 = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(2),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey2, SecurityAlgorithms.HmacSha256)
                    );

                // Return with the token
                _logger.LogDebug($"Login done for {user.Username}");
                return Created("refresh-token", new
                {
                    auth_token = new JwtSecurityTokenHandler().WriteToken(token1),
                    refresh_token = new JwtSecurityTokenHandler().WriteToken(token2),
                    auth_expiration = token1.ValidTo,
                    refresh_expiration = token2.ValidTo
                });
            }

            // No need to reissue token, return with Ok
            return Ok(new GeneralMessage() { Message = "No needs for refresh" });
        }

        /// <summary>
        /// This endpoint returns with the data about a user: username and email and role.
        /// </summary>
        /// <param name="username">Username who needs to be queried</param>
        /// <param name="id">ID who needs to be queried</param>
        /// <returns>With a User JSON object</returns>
        /// <response code="200">Data sent back successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Not authorized to get user info</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfo(string username = null, int id = -1)
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

            if(user == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User does not exist" });
            }

            // If username specified
            if (username != null)
            {
                if (user.Username == username || user.Role == UserRole.Admin)
                    return Ok(await _dbHandler.GetUserAsync(username));
                else
                    return Unauthorized(new GeneralMessage() { Message = "Not authorized to list other users" });
            }

            // If ID specified
            if (id != -1)
            {
                if (user.Id == id || user.Role == UserRole.Admin)
                    return Ok(await _dbHandler.GetUserAsync(id));
                else
                    return Unauthorized(new GeneralMessage() { Message = "Not authorized to list other users" });
            }
            return BadRequest(new GeneralMessage() { Message = "Invalid inputs" });
        }

        /// <summary>
        /// This endpoint is for deleting user
        /// </summary>
        /// <param name="username">Username who needs to be removed</param>
        /// <param name="id">User ID who needs to be removed</param>
        /// <returns></returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveUser(string username = null, int id = -1)
        {
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
            {
                return BadRequest(new GeneralMessage() { Message = "User does not exist" });
            }

            // If username is specified
            if (username != null)
            {
                if (username == user.Username || user.Role == UserRole.Admin)
                {
                    await _dbHandler.RemoveUserAsync(username);
                    return Ok(new GeneralMessage() { Message = "User has been removed" });
                }
                else
                {
                    return Unauthorized(new GeneralMessage() { Message = "Not atuhorized to delete user" });
                }

            }

            // If ID is specified
            if(id != -1)
            {
                if (id == user.Id || user.Role == UserRole.Admin)
                {
                    await _dbHandler.RemoveUserAsync(id);
                    return Ok(new GeneralMessage() { Message = "User has been removed" });
                }
                else
                {
                    return Unauthorized(new GeneralMessage() { Message = "Not atuhorized to delete user" });
                }
            }

            return BadRequest(new GeneralMessage() { Message = "Invalid input" });
        }

        /// <summary>
        /// This endpoint is listing all users
        /// </summary>
        /// <returns></returns>
        /// <response code="200">User listing successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var ret = await _dbHandler.GetUsersAsync();
            if (ret == null)
                return BadRequest(new GeneralMessage() { Message = "User listing has failed" });
            return Ok(ret);
        }

        /// <summary>
        /// This endpoint is for listing user based on specified system
        /// </summary>
        /// <param name="name">System name</param>
        /// <returns></returns>
        /// <response code="200">User listing successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/system")]
        public async Task<IActionResult> GetUsersBySystem(string name = null)
        {
            // Check that input specified
            if(name == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Missing system name" });
            }

            // Get the system
            var checkSys = await _dbHandler.GetSystemAsync(name);
            if (checkSys == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invlid system name" });
            }

            // Get the list and return
            var response = await _dbHandler.GetUsersAsync(checkSys);
            if(response == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User listing is failed" });
            }

            return Ok(response);
        }


        /// <summary>
        /// This endpoint is for listing user based on specified category
        /// </summary>
        /// <param name="name">Category name</param>
        /// <param name="system">System name</param>
        /// <returns></returns>
        /// <response code="200">User listing successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/category")]
        public async Task<IActionResult> GetUsersBySystem(string name = null, string system = null)
        {
            // Check that input is specified
            if(name == null || system == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Category name and system name must be specified" });
            }

            // Check that either category or system exist
            var checkSys = await _dbHandler.GetSystemAsync(system);
            if(checkSys == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Specified system does not exist" });
            }

            var checkCat = await _dbHandler.GetCategoryAsync(name, checkSys);
            if(checkCat == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Specified category does not exist" });
            }

            // Everything seems fine, do the action
            var response = await _dbHandler.GetUsersAsync(checkCat);
            if(response == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User listing is failed" });
            }

            return Ok(response);
        }

        /// <summary>
        /// This endpoint is for changing user information like email or password
        /// </summary>
        /// <param name="changedUser">Object for new values</param>
        /// <returns></returns>
        /// <response code="200">Change was successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("change/general")]
        public async Task<IActionResult> ModifyUser([FromBody]UserLoginInfo changedUser)
        {
            // Check that new data is provided
            if(changedUser == null)
            {
                return BadRequest(new GeneralMessage() { Message = "No input data is provided" });
            }

            // Get user who requested it
            var re = Request;
            var headers = re.Headers;
            var tokenString = headers["Authorization"];

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString[0].Split(' ')[1]);

            var claims = token.Claims;
            var usernameClaim = claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
            var user = await _dbHandler.GetUserAsync(usernameClaim.Value);

            // If user does not exist, then BadRequest
            if (user == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User does not exist" });
            }

            // Check that requester is same then the modify user or role is Admin
            if(user.Username != changedUser.Username && user.Role == UserRole.User)
            {
                return Unauthorized(new GeneralMessage() { Message = "Not authorized to change different user" });
            }

            // Everything look cool, let change
            var response = await _dbHandler.ChangeUserAsync(user.Id, new User() { Username = changedUser.Username, Email = changedUser.Email, Password = changedUser.Password });
            if(response.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = response.MessageText });
            }

            // Everything was fine, return woth OK
            return Ok(new GeneralMessage() { Message = "Values has been changed" });
        }

        /// <summary>
        /// This endpoint is to chaneging user role for Admin users
        /// </summary>
        /// <param name="username">User who needs to be adjusted</param>
        /// <param name="role">New role of user, can be User or Admin</param>
        /// <returns></returns>
        /// <response code="200">Change was successfully</response>
        /// <response code="400">Invlaid input</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("change/role")]
        public async Task<IActionResult> ModifyUserRole(string username, string role)
        {
            // Check that specified user exist
            var checkUsr = await _dbHandler.GetUserAsync(username);
            if(checkUsr == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User does not exist" });
            }

            // Validate the role
            if(role != "User" && role != "Admin")
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid role type" });
            }

            // Convert role string into an Enum
            UserRole roleType = (UserRole)Enum.Parse(typeof(UserRole), role);

            // Do the change
            var response = await _dbHandler.ChangeUserRole(checkUsr, roleType);
            if(response.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = response.MessageText });
            }

            return Ok(new GeneralMessage() { Message = "Role has been changed" });
        }


    }
}
