using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeTicketing.Model;
using Microsoft.AspNetCore.Http;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made to handle those request which are related to Categories table           */
    /*********************************************************************************************/
    [Produces("application/json")]
    [Route("category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly DataContext _context;

        public CategoryController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List categoires
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
            var data = await _context.Categories.ToListAsync();
            return Ok(data);
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <remarks>
        /// This request create a new category.
        /// </remarks>
        /// <param name="name">New category name</param>
        /// <returns>With the created category</returns>
        /// <response code="200">Category is created</response>
        /// <response code="400">Category already exist</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{name}")]
        public async Task<IActionResult> CategoryAdd(string name)
        {
            Category new_cat = new Category();
            new_cat.Name = name;

            var record = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(name));

            if(record != null)                               /* If category already exist, error */
            {
                return BadRequest();
            }

            await _context.AddAsync(new_cat);
            await _context.SaveChangesAsync();

            var data = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(name));
            return Ok(data);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <remarks>
        /// This delete a category 
        /// </remarks>
        /// <param name="name">Category for delete</param>
        /// <returns></returns>
        /// <response code="200">Category is deleted</response>
        /// <response code="400">Category did not exist</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{name}")]
        public async Task<IActionResult> CategoryDelete(string name)
        {
            var record = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(name));

            if(record == null)                              /* If category does not exist, error */
            {
                return BadRequest();
            }

            _context.Remove(record);
            await _context.SaveChangesAsync();

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
        /// <param name="current">Current category name</param>
        /// <param name="to">NBew category name</param>
        /// <returns>With the modified category</returns>
        /// <response code="200">Category is renamed</response>
        /// <response code="400">New category already exist</response>
        /// <response code="404">Current category did not found</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("change/{current}/{to}")]
        public async Task<IActionResult> CategoryChange(string current, string to)
        {
            /*--- Check that current exist ---*/
            var category = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(current));

            if(category == null)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Category did not found: {current}";
                return NotFound(ret);
            }

            /*--- Check that the new name is not exist yet ---*/
            var category2 = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(to));

            if(category2 != null)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Category already exist: {to}";
                return BadRequest(ret);
            }

            /*--- Change the category and return with 200 ---*/
            category.Name = to;

            await _context.SaveChangesAsync();

            return Ok(category);
        }
    }
}
