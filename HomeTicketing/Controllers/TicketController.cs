using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeTicketing.Model;
using Microsoft.AspNetCore.Http;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file has been made to handle those requests, which are related to Tickets and Logs   */
    /*********************************************************************************************/
    [Produces("application/json")]
    [Route("ticket")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        /*---------------------------------------------------------------------------------------*/
        /* Read the actual context (connection to database table and information)                */
        /*---------------------------------------------------------------------------------------*/
        private readonly DataContext _context;

        public TicketController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List all ticket
        /// </summary>
        /// <remarks>
        /// This request list all existing ticket without any filter
        /// </remarks>
        /// <returns>JSON array about the tickets</returns>
        /// <response code="200">Request is completed</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetTicket()
        {
            var data = await (from t in _context.Tickets
                              join c in _context.Categories
                              on t.Category equals c.Id
                              select new
                              {
                                  Id = t.Id,
                                  Title = t.Title,
                                  Time = t.Time,
                                  Category = c.Name,
                                  Status = t.Status,
                                  Reference = t.Reference
                              }).ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// List ticket with filter
        /// </summary>
        /// <remarks>
        /// This request filtering the tickts based on the input and return with the list.
        /// </remarks>
        /// <param name="_input">Filters in JSON</param>
        /// <returns>JSON array about the tickets</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="404">No ticket was found based on filters</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredTicket(TicketFilter _input)
        {
            /*--- Set default values, if null is specified ---*/
            Ticket input = new Ticket();

            if (_input.Status == null)
                input.Status = "";
            else
                input.Status = _input.Status;

            if (_input.Reference == null)
                input.Reference = "";
            else
                input.Reference = _input.Reference;

            if (_input.Title == null)
                input.Title = "";
            else
                input.Title = _input.Title;

            var category = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(_input.Category));

            if (category != null)
            {
                input.Category = category.Id;

                /*--- Listing records which are proper for fitlers ---*/
                var records = await (from t in _context.Tickets
                                     where t.Category == (input.Category) && t.Reference.Contains(input.Reference) && t.Status.Contains(input.Status) && t.Title.Contains(input.Title)
                                     select new
                                     {
                                         Id = t.Id,
                                         Category = t.Category,
                                         Reference = t.Reference,
                                         Status = t.Status,
                                         Time = t.Time,
                                         Title = t.Title
                                     }).ToListAsync();
                /*--- If none found, return with 404, else return with 200 ---*/
                if (records.Count == 0)
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = "No entry if found for filters";
                    return NotFound(ret);
                }

                /*--- Need to change due to group trasnlation ---*/
                TicketHeader[] tickets = new TicketHeader[records.Count];
                for (int i = 0; i < records.Count; i++)
                {
                    tickets[i] = new TicketHeader();
                    tickets[i].Id = records[i].Id;
                    tickets[i].Reference = records[i].Reference;
                    tickets[i].Status = records[i].Status;
                    tickets[i].Time = records[i].Time;
                    tickets[i].Title = records[i].Title;
                    tickets[i].Category = category.Name;
                }

                return Ok(tickets);
            }
            else
            {
                /*--- Listing records which are proper for fitlers ---*/
                var records = await (from t in _context.Tickets
                                     where t.Reference.Contains(input.Reference) && t.Status.Contains(input.Status) && t.Title.Contains(input.Title)
                                     select new
                                     {
                                         Id = t.Id,
                                         Category = t.Category,
                                         Reference = t.Reference,
                                         Status = t.Status,
                                         Time = t.Time,
                                         Title = t.Title
                                     }).ToListAsync();
                /*--- If none found, return with 404, else return with 200 ---*/
                if (records.Count == 0)
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = "No entry if found for filters";
                    return NotFound(ret);
                }

                /*--- Need to change due to group trasnlation ---*/
                TicketHeader[] tickets = new TicketHeader[records.Count];
                for (int i = 0; i < records.Count; i++)
                {
                    tickets[i] = new TicketHeader();
                    tickets[i].Id = records[i].Id;
                    tickets[i].Reference = records[i].Reference;
                    tickets[i].Status = records[i].Status;
                    tickets[i].Time = records[i].Time;
                    tickets[i].Title = records[i].Title;

                    var act_categpry = await _context.Categories.SingleOrDefaultAsync(s => s.Id == records[i].Category);

                    if (act_categpry == null)
                        tickets[i].Category = null;
                    else
                        tickets[i].Category = act_categpry.Name;
                }

                return Ok(tickets);
            }
        }

        /// <summary>
        /// Details about a ticket
        /// </summary>
        /// <remarks>
        /// This request is coming back with a fully detailed ticket, whoch contains the header and all log entries
        /// </remarks>
        /// <param name="id">Ticket ID</param>
        /// <returns>Reutnr with ticket details</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="404">No ticket was found with specified ID</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("details/id/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            /*--- Looking for ticket by ID ---*/
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Id == id);

            /*--- If not found, then return with error ---*/
            if(record == null)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Ticket did not found based on ID: {id}";
                return NotFound(ret);
            }

            /*--- Assembly a structure for response JSON and upload with data ---*/
            TicketDetails data = new TicketDetails();
            data.Header = new TicketHeader();
            data.Header.Id = record.Id;
            data.Header.Reference = record.Reference;
            data.Header.Status = record.Status;
            data.Header.Time = record.Time;
            data.Header.Title = record.Title;

            var category = await _context.Categories.SingleOrDefaultAsync(s => s.Id == record.Category);
            if (category == null)
                data.Header.Category = null;
            else
                data.Header.Category = category.Name;

            /*--- Query log entries which belongs to ticket ID ---*/
            var logs = await (from l in _context.Logs
                              where l.Ticket.Id.Equals(record.Id)
                              select new
                              {
                                  Summary = l.Summary,
                                  Details = l.Details,
                                  Time = l.Time,
                                  Id = l.Id
                              }).OrderBy(x => x.Time).ToListAsync();

            /*--- There should not exist ticket without log, so no need to handle exception ---*/
            data.Logs = new Log[logs.Count];
            for(int i = 0; i < logs.Count; i++)                  /* Log entires after each other */
            {
                data.Logs[i] = new Log();
                data.Logs[i].Details = logs[i].Details;
                data.Logs[i].Summary = logs[i].Summary;
                data.Logs[i].Time = logs[i].Time;
                data.Logs[i].Id = logs[i].Id;
            }

            return Ok(data);
        }

        /// <summary>
        /// Create ticket
        /// </summary>
        /// <remarks>
        /// This request has purpose the generate a new ticket or add a new log for an already exist ticket
        /// </remarks>
        /// <param name="_input"></param>
        /// <returns>With the generated ticket header</returns>
        /// <response code="200">REquest is completed</response>
        /// <response code="400">Mandatory filed missing or category does not exist</response>
        /// <response code="500">Internal API error, ticket creation was unsuccessful</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateTicket(TicketCreateHeader _input)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                /*--- Prepare the Ticket object for database ---*/
                Ticket input = new Ticket();
                input.Reference = _input.Reference;
                input.Title = _input.Title;

                input.Status = "Open";
                input.Time = DateTime.Now.ToString("yyyyMMddHHmmss");

                /*--- Check that category exist ---*/
                var category = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(_input.Category));
                if (category == null)
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = $"Invalid category name: {_input.Category}";
                    return BadRequest(ret);
                }

                input.Category = category.Id;

                /*--- Check for mandatory fields ---*/
                if (_input.Category == null || _input.Reference == null || _input.Summary == null || _input.Title == null ||
                    _input.Category == "" || _input.Reference == "" || _input.Summary == "" || _input.Title == "")
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = "At least one of the following fields are null: Category, Reference, Summary, Title";
                    return BadRequest(ret);
                }

                /*--- If not open, then create ---*/
                var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(input.Reference) && s.Status.Equals("Open"));

                if (record == null)
                {
                    await _context.AddAsync(input);
                    await _context.SaveChangesAsync();
                }

                record = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(input.Reference) && s.Status.Equals("Open"));

                /*--- Update log entry ---*/
                Log new_log = new Log();
                new_log.Summary = _input.Summary;
                new_log.Ticket = record;

                new_log.Time = DateTime.Now.ToString("yyyyMMddHHmmss");

                if (_input.Details == null)
                    new_log.Details = _input.Summary;
                else
                    new_log.Details = _input.Details;

                await _context.AddAsync(new_log);

                /*--- Save everything, then return with 200 ---*/
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var data = await (from t in _context.Tickets
                                  join c in _context.Categories
                                  on t.Category equals c.Id
                                  where t.Id.Equals(record.Id)
                                  select new
                                  {
                                      Id = t.Id,
                                      Title = t.Title,
                                      Time = t.Time,
                                      Category = c.Name,
                                      Status = t.Status,
                                      Reference = t.Reference
                                  }).ToListAsync();

                return Ok(data[0]);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Interal error on server: {ex.Message}";
                return StatusCode(500, ret);
            }
        }

        /// <summary>
        /// Close ticket
        /// </summary>
        /// <remarks>
        /// This request is good to close ticket based on ID or reference value
        /// </remarks>
        /// <param name="type">Can be 'id' or 'rv'</param>
        /// <param name="value">Specified ID or reference value</param>
        /// <returns>With 200 if OK else with error message</returns>
        /// <response code="200">Request is completed</response>
        /// <response code="400">Type is neither 'id' nor 'rv'</response>
        /// <response code="404">Not ticket found with the specified value</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        [HttpPut("close/{type}/{value}")]
        public async Task<IActionResult> CloseTicket(string type, string value)
        {
            
            if (type == "id")
            {
                /*--- Looking for open tickets ---*/
                var data = await _context.Tickets.SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(value) && s.Status.Equals("Open"));

                /*--- If ticket not found, then error ---*/
                if (data == null)
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = $"Not found opened incident with ID: {value}";
                    return NotFound(ret);
                }

                /*--- If found, then put into close ---*/
                data.Status = "Closed";
                await _context.SaveChangesAsync();

                return Ok();
            }
            else if (type == "rv")
            {
                /*---Looking for ticket based on refefence value-- - */
                var data = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(value) && s.Status.Equals("Open"));

                /*--- If ticket not found, then error ---*/
                if (data == null)
                {
                    ErrorMessage ret = new ErrorMessage();
                    ret.Message = $"Not found opened incident with reference value: {value}";
                    return NotFound(ret);
                }

                /*--- If found, then put into close ---*/
                data.Status = "Closed";
                await _context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Invalid type: {type}";
                return BadRequest(ret);
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: PUT  /ticket/change                                                             */
        /* Assigned JSON: contains the values for "to" status. ID is mandatory.                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* It can change ticket header: Category, Title and Reference                            */
        /*---------------------------------------------------------------------------------------*/
        /// <summary>
        /// Change ticket parameters
        /// </summary>
        /// <remarks>
        /// By this request some parameter can be chanegd for a specified ticket. Parameter's values like: title, reference and category
        /// </remarks>
        /// <param name="_input"></param>
        /// <returns>With the modified ticket JSON if OK, else error message</returns>
        /// <response code="200">Change is completed</response>
        /// <response code="400">ID is missing</response>
        /// <response code="404">Ticket did not found with the specified ID</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("change")]
        public async Task<IActionResult> ChangeTicket(TicketChangeData _input)
        {
            string update_text = "Changed values:\n";
            bool update_flag = false;
            /*--- If ID is missing, then error ---*/
            if(_input.Id == 0)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = "ID is missing";
                return BadRequest(ret);
            }

            /*--- Looking for the entry ---*/
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Id.Equals(_input.Id));

            if(record == null)
            {
                ErrorMessage ret = new ErrorMessage();
                ret.Message = $"Ticket did not found for specified ID: {_input.Id.ToString()}";
                return NotFound(ret);
            }

            /*--- Change the required variables ---*/
            if(_input.Title != null)
            {
                update_flag = true;
                update_text = update_text + $"Title from '{record.Title}' to '{_input.Title}'\n";
                record.Title = _input.Title;
            }
            if(_input.Reference != null)
            {
                update_flag = true;
                update_text = update_text + $"Reference from '{record.Reference}' to '{_input.Reference}'\n";
                record.Reference = _input.Reference;
            }

            if(_input.Category != null)
            {
                var category = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(_input.Category));
                if (category != null)
                {
                    update_flag = true;
                    update_text = update_text + $"Category to {category.Name}\n";
                    record.Category = category.Id;
                }
            }

            /*--- Write update log ---*/
            if (update_flag)
            {
                Log update = new Log();
                update.Ticket = record;
                update.Summary = "Change in ticket header";
                update.Details = update_text;
                update.Time = DateTime.Now.ToString("yyyyMMddHHmmss");

                await _context.AddAsync(update);
                await _context.SaveChangesAsync();
            }

            var data = await (from t in _context.Tickets
                              join c in _context.Categories
                              on t.Category equals c.Id
                              where t.Id.Equals(record.Id)
                              select new
                              {
                                  Id = t.Id,
                                  Title = t.Title,
                                  Time = t.Time,
                                  Category = c.Name,
                                  Status = t.Status,
                                  Reference = t.Reference
                              }).ToListAsync();

            return Ok(data[0]);
        }
    }
}
