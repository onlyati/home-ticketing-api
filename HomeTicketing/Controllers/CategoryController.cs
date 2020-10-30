using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeTicketing.Model;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made to handle those request which are related to Categories table           */
    /*********************************************************************************************/
    [Route("[controller]")]
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: GET /category                                                                   */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* List all defined categories                                                           */
        /*---------------------------------------------------------------------------------------*/
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var data = await _context.Categories.ToListAsync();
            return Ok(data);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: POST /category/add/{name}                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* Add new category with specified {name} value.                                         */
        /*---------------------------------------------------------------------------------------*/
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: POST /category/delete/{name}                                                    */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* Delete category if exist with {name} value                                            */
        /*---------------------------------------------------------------------------------------*/
        [HttpPut("delete/{name}")]
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
