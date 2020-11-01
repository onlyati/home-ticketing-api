using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeTicketing.Model;

namespace HomeTicketing.Controllers
{
    /*********************************************************************************************/
    /* This file has been made to handle those requests, which are related to Tickets and Logs   */
    /*********************************************************************************************/
    [Route("[controller]")]
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: GET /ticket                                                                     */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This method listing all existing ticket and return the caller the output              */
        /*---------------------------------------------------------------------------------------*/
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: POST /ticket/filter                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This method listing all existing ticket and return the caller the output              */
        /*---------------------------------------------------------------------------------------*/
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredTicket(TicketReq _input)
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: GET /ticket/details/id/{id}                                                     */
        /* Assigned JSON: filter informations                                                    */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This method collect the main Ticket and the assigned Log entries for a specified id.  */
        /*---------------------------------------------------------------------------------------*/
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: POST /ticket/create                                                             */
        /* Assigned JSON: information about the ticket                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* It open a new ticket or update and existing one.                                      */
        /*---------------------------------------------------------------------------------------*/
        [HttpPost]
        public async Task<IActionResult> CreateTicket(TicketReq _input)
        {
            var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                /*--- Prepare the Ticket object for database ---*/
                Ticket input = new Ticket();
                input.Id = _input.Id;
                input.Reference = _input.Reference;
                input.Status = _input.Status;
                input.Time = _input.Time;
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

        /*---------------------------------------------------------------------------------------*/
        /* Properties:                                                                           */
        /* -----------                                                                           */
        /* Type: POST /close/id/{id}                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* ------------                                                                          */
        /* This method close ticket based on ID or reference value, if it can.                   */
        /*---------------------------------------------------------------------------------------*/
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
        [HttpPut("change")]
        public async Task<IActionResult> ChangeTicket(TicketHeader _input)
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
                return BadRequest(ret);
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
