using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomeTicketing.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DatabaseController.Model;
using DatabaseController.Interface;
using DatabaseController.Controller;
using Microsoft.AspNetCore.Authorization;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made to handle those request which are related to Categories table           */
    /*********************************************************************************************/
    [Produces("application/json")]
    [Route("category")]
    [Authorize]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly ITicketHandler _ticket;
        private readonly ILogger _logger;

        public CategoryController(ITicketHandler ticket, ILogger<CategoryController> logger)
        {
            _ticket = ticket;
            _logger = logger;
        }

        /// <summary>
        /// List categories
        /// </summary>
        /// <remarks>
        /// This request is good to list all existing categories
        /// </remarks>
        /// <returns>List of categories in JSON</returns>
        /// <response code="200">Request is completed</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            _logger.LogDebug("GET categories are requested");
            var data = await _ticket.ListCategoriesAsync();
            _logger.LogInformation("GET categories are completed");
            return Ok(data);
        }

        /// <summary>
        /// List categories based on system name
        /// </summary>
        /// <param name="system">Selected system</param>
        /// <returns>List or error message in JSON</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Request is failed</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("system/{system}")]
        public async Task<IActionResult> GetCategoriesBySystem(string system)
        {
            _logger.LogDebug($"GET categories/system/{system} is completed");
            var data = await _ticket.ListCategoriesAsync(await _ticket.GetSystemAsync(system));
            if(data == null)
            {
                _logger.LogDebug($"GET categories/system/{system} is failed");
                ErrorMessage ret = new ErrorMessage();
                ret.Message = "List is failed";
                return BadRequest(ret);
            }
            _logger.LogDebug($"GET categories/system/{system} is completed");
            return Ok(data);
        }

        /// <summary>
        /// List categories based on username
        /// </summary>
        /// <param name="user">Selected system</param>
        /// <returns>List or error message in JSON</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Request is failed</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("user/{user}")]
        public async Task<IActionResult> GetCategoriesByUser(string user)
        {
            _logger.LogDebug($"GET categories/system/{user} is completed");
            var data = await _ticket.ListCategoriesAsync(await _ticket.GetUserAsync(user));
            if (data == null)
            {
                _logger.LogDebug($"GET categories/system/{user} is failed");
                ErrorMessage ret = new ErrorMessage();
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpPost("{system}/{name}")]
        public async Task<IActionResult> CategoryAdd(string system, string name)
        {
            _logger.LogDebug($"New category ({name}) is requested");
            var sysrec = await _ticket.GetSystemAsync(system);
            var respond = await _ticket.AddCategoryAsync(name, sysrec);
            if(respond.MessageType == MessageType.NOK)
            {
                _logger.LogWarning($"{respond.MessageText}");
                ErrorMessage ret = new ErrorMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }
            var data = await _ticket.GetCategoryAsync(name, sysrec);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpDelete("{system}/{name}")]
        public async Task<IActionResult> CategoryDelete(string system, string name)
        {
            _logger.LogDebug($"Delete category request ({name}) is requested");
            var sysrec = await _ticket.GetSystemAsync(system);
            var respond = await _ticket.DeleteCategoryAsync(name, sysrec);
            if(respond.MessageType == MessageType.NOK)                              /* If category does not exist, error */
            {
                _logger.LogWarning($"Category ({name}) does not exist");
                return BadRequest();
            }
            _logger.LogInformation($"Category ({name}) has been deleted");
            return Ok();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* Type: POST /category/change/{current}/{to}                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This method change group name                                                         */
        /*---------------------------------------------------------------------------------------*/
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
        /// <response code="404">Current category did not found</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAuthorized(UserRole.Admin)]
        [HttpPut("change/{system}/{current}/{to}")]
        public async Task<IActionResult> CategoryChange(string system, string current, string to)
        {
            _logger.LogDebug($"Change category ({current} -> {to}) is requested");
            var sysrec = await _ticket.GetSystemAsync(system);
            var respond = await _ticket.RenameCategoryAsync(current, to, sysrec);
            if(respond.MessageType == MessageType.NOK)
            {
                _logger.LogWarning(respond.MessageText);
                ErrorMessage ret = new ErrorMessage();
                ret.Message = respond.MessageText;
                return BadRequest(ret);
            }

            /*--- Change the category and return with 200 ---*/
            _logger.LogInformation($"Change category ({current} -> {to}) has been done");
            return Ok(await _ticket.GetCategoryAsync(to, sysrec));
        }
    }
}
