using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeTicketing.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DatabaseController.Model;
using DatabaseController.Interface;
using DatabaseController.Controller;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace HomeTicketing.Controllers
{
    [Produces("application/json")]
    [Route("category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly IDbHandler _dbHandler;
        private readonly ILogger _logger;

        public CategoryController(IDbHandler ticket, ILogger<CategoryController> logger)
        {
            _dbHandler = ticket;
            _logger = logger;
        }

        /// <summary>
        /// List all categories
        /// </summary>
        /// <remarks>
        /// This request is good to list all existing categories
        /// </remarks>
        /// <returns>List of categories in JSON</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Request is failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/all")]
        public async Task<IActionResult> GetCategories()
        {
            _logger.LogDebug("GET categories are requested");
            var data = await _dbHandler.ListCategoriesAsync();
            if(data == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Listing has failed" });
            }
            _logger.LogInformation("GET categories are completed");
            return Ok(data);
        }

        /// <summary>
        /// List categories based on system name
        /// </summary>
        /// <returns>List or error message in JSON</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Request is failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/system")]
        public async Task<IActionResult> GetCategoriesBySystem(string value = "")
        {
            string system = value;
            _logger.LogDebug($"GET categories/system/{system} is completed");
            var data = await _dbHandler.ListCategoriesAsync(await _dbHandler.GetSystemAsync(system));
            if(data == null)
            {
                _logger.LogDebug($"GET categories/system/{system} is failed");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = "List is failed";
                return BadRequest(ret);
            }
            _logger.LogDebug($"GET categories/system/{system} is completed");
            return Ok(data);
        }

        /// <summary>
        /// List categories based on username
        /// </summary>
        /// <param name="value">Selected user</param>
        /// <returns>List or error message in JSON</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Request is failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("list/user")]
        public async Task<IActionResult> GetCategoriesByUser(string value = "")
        {
            string user = value;
            _logger.LogDebug($"GET categories/system/{user} is completed");
            var data = await _dbHandler.ListCategoriesAsync(await _dbHandler.GetUserAsync(user));
            if (data == null)
            {
                _logger.LogDebug($"GET categories/system/{user} is failed");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = "List is failed";
                return BadRequest(ret);
            }
            _logger.LogDebug($"GET categories/system/{user} is completed");
            return Ok(data);
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <remarks>
        /// This request create a new category.
        /// </remarks>
        /// <param name="system">System where the category must be created</param>
        /// <param name="name">New category name</param>
        /// <returns>With the created category</returns>
        /// <response code="200">Category is created</response>
        /// <response code="400">Category already exist</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpPost("create")]
        public async Task<IActionResult> CategoryAdd(string system, string name)
        {
            _logger.LogDebug($"New category ({name}) is requested");
            var sysrec = await _dbHandler.GetSystemAsync(system);
            var respond = await _dbHandler.AddCategoryAsync(name, sysrec);
            if(respond.MessageType == MessageType.NOK)
            {
                _logger.LogWarning($"{respond.MessageText}");
                GeneralMessage ret = new GeneralMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }
            var data = await _dbHandler.GetCategoryAsync(name, sysrec);
            _logger.LogInformation($"New category ({name}) request is completed");
            return Ok(data);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <remarks>
        /// This delete a category 
        /// </remarks>
        /// <param name="system">System name where category needs to be located</param>
        /// <param name="name">Category for delete</param>
        /// <returns></returns>
        /// <response code="200">Category is deleted</response>
        /// <response code="400">Category did not exist</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpDelete("remove")]
        public async Task<IActionResult> CategoryDelete(string system, string name)
        {
            _logger.LogDebug($"Delete category request ({name}) is requested");
            var sysrec = await _dbHandler.GetSystemAsync(system);
            var respond = await _dbHandler.DeleteCategoryAsync(name, sysrec);
            if(respond.MessageType == MessageType.NOK)                              /* If category does not exist, error */
            {
                _logger.LogWarning($"Category ({name}) does not exist");
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });
            }
            _logger.LogInformation($"Category ({name}) has been deleted");
            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }

        /// <summary>
        /// Change group name
        /// </summary>
        /// <remarks>
        /// Change the group name for a non-exist new name
        /// </remarks>
        /// <param name="system">System where category can be located</param>
        /// <param name="current">Current category name</param>
        /// <param name="to">New category name</param>
        /// <returns>With the modified category</returns>
        /// <response code="200">Category is renamed</response>
        /// <response code="400">New category already exist</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        /// <response code="404">Current category did not found</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpPut("change")]
        public async Task<IActionResult> CategoryChange(string system, string current, string to)
        {
            _logger.LogDebug($"Change category ({current} -> {to}) is requested");
            var sysrec = await _dbHandler.GetSystemAsync(system);
            var respond = await _dbHandler.RenameCategoryAsync(current, to, sysrec);
            if(respond.MessageType == MessageType.NOK)
            {
                _logger.LogWarning(respond.MessageText);
                GeneralMessage ret = new GeneralMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }
            var data = await _dbHandler.GetCategoryAsync(to, sysrec);
            /*--- Change the category and return with 200 ---*/
            _logger.LogInformation($"Change category ({current} -> {to}) has been done");
            return Ok(data);
        }

        /// <summary>
        /// This endpoint is made for assinging users to specified category-system pair
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <param name="categoryName">Category's name</param>
        /// <param name="systemName">System's name</param>
        /// <returns></returns>
        /// <response code="200">Assing done</response>
        /// <response code="400">New category already exist</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("assign/user")]
        public async Task<IActionResult> AssignUserToCategory(string userName = null, string categoryName = null, string systemName = null)
        {
            // Check input data
            if(userName == null || categoryName == null || systemName == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User, category and system names are mandatory" });
            }

            // Query the objects belongs to inputs
            var recSys = await _dbHandler.GetSystemAsync(systemName);
            if(recSys == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid system name" });
            }

            var recCat = await _dbHandler.GetCategoryAsync(categoryName, recSys);
            if(recCat == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid category name" });
            }

            var recUsr = await _dbHandler.GetUserAsync(userName);
            if(recUsr == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid user name" });
            }

            // Do the action and return with the result
            var respond = await _dbHandler.AssignUserToCategory(recCat, recUsr);
            if(respond.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });
            }

            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }

        /// <summary>
        /// This endpoint is made for unassinging users to specified category-system pair
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <param name="categoryName">Category's name</param>
        /// <param name="systemName">System's name</param>
        /// <returns></returns>
        /// <response code="200">Unassign done</response>
        /// <response code="400">New category already exist</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpPut("unassign/user")]
        public async Task<IActionResult> UnassignUserFromCategory(string userName = null, string categoryName = null, string systemName = null)
        {
            // Check input data
            if (userName == null || categoryName == null || systemName == null)
            {
                return BadRequest(new GeneralMessage() { Message = "User, category and system names are mandatory" });
            }

            // Query the objects belongs to inputs
            var recSys = await _dbHandler.GetSystemAsync(systemName);
            if (recSys == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid system name" });
            }

            var recCat = await _dbHandler.GetCategoryAsync(categoryName, recSys);
            if (recCat == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid category name" });
            }

            var recUsr = await _dbHandler.GetUserAsync(userName);
            if (recUsr == null)
            {
                return BadRequest(new GeneralMessage() { Message = "Invalid user name" });
            }

            // Do the action and return with the result
            var respond = await _dbHandler.UnassignUserToCategory(recCat, recUsr);
            if (respond.MessageType == MessageType.NOK)
            {
                return BadRequest(new GeneralMessage() { Message = respond.MessageText });
            }

            return Ok(new GeneralMessage() { Message = respond.MessageText });
        }

        /// <summary>
        /// This endpoint return with a Category record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Listing failed</response>
        /// <response code="401">Not authorized</response>
        /// <response code="403">Not authorized</response>
        [AllowAuthorized(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [HttpGet("get")]
        public async Task<IActionResult> GetCategory(int id = -1)
        {
            if (id == -1)
                return BadRequest(new GeneralMessage() { Message = "ID is missing" });

            var respond = await _dbHandler.GetCategoryAsync(id);
            if (respond == null)
                return BadRequest(new GeneralMessage() { Message = "Get category failed" });

            return Ok(respond);
        }
    }
}
