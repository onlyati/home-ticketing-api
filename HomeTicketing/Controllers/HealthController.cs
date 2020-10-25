using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HomeTicketing.Model;
using Microsoft.EntityFrameworkCore;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file is made for health check of API                                                 */
    /*********************************************************************************************/
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly DataContext _context;

        public HealthController(DataContext context)
        {
            _context = context;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: GET /health                                                                     */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This function send back and OK message. It can be used as health check of API.        */
        /*---------------------------------------------------------------------------------------*/
        [HttpGet]
        public IActionResult HealthCheck()
        {
            ErrorMessage OkMsg = new ErrorMessage();
            OkMsg.Message = "I am still alive";
            return Ok(OkMsg);
        }
    }
}
