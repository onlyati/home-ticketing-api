using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseController.Interface;
using DatabaseController.Model;
using System.Threading.Tasks;

namespace DatabaseController.Controller
{
    public class TicketHandler : ITicketHandler
    {
        private string _connectionString;
        private DataContext _context;

        #region Constructor
        /// <summary>
        /// Constructor for creating TicketHandler object. This object can be used for any pre-defined action with database.
        /// </summary>
        /// <param name="connectionString">Database connection string, used by EF</param>
        public TicketHandler(string connectionString)
        {
            if(connectionString == "")
            {
                throw new NullReferenceException("Connection string is missing in the constructor");
            }

            _connectionString = connectionString;

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseMySql(connectionString);
            _context = new DataContext(optionsBuilder.Options);
        }
        #endregion

        #region Attributes
        /// <summary>
        /// This function returns with the specified connection string
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }
        #endregion

        #region Category Stuff
        /// <summary>
        /// Add new category if not exist yet
        /// </summary>
        /// <remarks>
        /// Method does the following steps:
        /// <list type="number">
        /// <item>Make a lock to prevent any other changes</item>
        /// <item>Check that category name alredy exist. If it does, return with NOK message</item>
        /// <item>Add new category, save the changes</item>
        /// <item>Release the lock and send an OK message back</item>
        /// </list>
        /// </remarks>
        /// <param name="category">Name of category</param>
        /// <returns>With OK message if created, else with NOK message with explanation</returns>
        public async Task<Message> AddCategoryAsync(string category)
        {
            // Message which will be sent back
            Message response = new Message();

            // Make lock for database to prevent changes meanwhile it works
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Create a new category
            Category new_cat = new Category();
            new_cat.Name = category;

            // Check if it already exist
            var record = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(category));

            if (record != null)                               /* If category already exist, error */
            {
                // If it does, then throw back error message
                response.MessageType = MessageType.NOK;
                response.MessageText = "Category already exist";
                transaction.Rollback();
                return response;
            }

            // if it does not, then add the category to database
            await _context.AddAsync(new_cat);
            await _context.SaveChangesAsync();

            // Complete the transaction, so other request can reach it
            transaction.Commit();

            response.MessageType = MessageType.OK;
            response.MessageText = $"Category ({category}) has been added";

            return response;
        }

        /// <summary>
        /// This method change a category name.
        /// </summary>
        /// <remarks>
        /// This method does the following:
        /// <list type="number">
        /// <item>Put a lock to the table to aboid concurrent changes</item>
        /// <item>Check that 'from' does exist. If it does not, return with NOK message</item>
        /// <item>Check that 'to' does not exist. If it does, return with NOK messgae</item>
        /// <item>Do the change</item>
        /// <item>Relase the lock and send an OK message</item>
        /// </list>
        /// </remarks>
        /// <param name="from">Existing category name</param>
        /// <param name="to">Non-existing category name</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> RenameCategoryAsync(string from, string to)
        {
            // Object for return value
            Message respond = new Message();

            // Start a transaction, so nothing else will change the table while we are working
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Query the 'from' category name
            var fromRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(from));

            // If 'from' does not exist, return with error
            if(fromRecord == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"The specified category ({from}) does not exist, thus not possible to rename it";
                transaction.Rollback();
                return respond;
            }

            // Query the 'to' category name
            var toRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(to));

            // if 'to' does exist, return with error
            if(toRecord != null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"The specified new name ({to}) is already exist";
                transaction.Rollback();
                return respond;
            }

            // Update the fromRecord, then save the changes
            fromRecord.Name = to;
            await _context.SaveChangesAsync();

            // Commit the transaction, then send the OK message 
            transaction.Commit();

            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Category rename from '{from}' to '{to}' is done";

            return respond;
        }

        /// <summary>
        /// This method deletes entry from categories where the name is match
        /// </summary>
        /// <remarks>
        /// This method does the following:
        /// <list type="number">
        /// <item>Put lock onto the table</item>
        /// <item>Check that category exist. If it does not, return with NOK message</item>
        /// <item>Delete record from table</item>
        /// <item>Release the lock and return with OK message</item>
        /// </list>
        /// </remarks>
        /// <param name="name">Name of category</param>
        /// <returns>OK or a NOK Message</returns>
        public async Task<Message> DeleteCategoryAsync(string name)
        {
            // Message for response
            Message respond = new Message();

            // Make lock for database to prevent changes meanwhile it works
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Query based on category name
            var record = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(name));

            // If specified category did not exist, return with NOK message
            if(record == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Specified category, by '{name}' name, did not exist";
                transaction.Rollback();
                return respond;
            }

            // Else save information about record and perform remove and save
            string cat_name = $"{record.Id} - {record.Name}";
            _context.Categories.Remove(record);
            await _context.SaveChangesAsync();

            // Release the lock, commit the changes
            transaction.Commit();

            // Everything is fine, let us send an OK message
            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Category ({cat_name}) has been deleted";

            return respond;
        }

        /// <summary>
        /// This method deletes entry from categories where the ID is match
        /// </summary>
        /// /// <remarks>
        /// This method does the following:
        /// <list type="number">
        /// <item>Put lock onto the table</item>
        /// <item>Check that category exist. If it does not, return with NOK message</item>
        /// <item>Delete record from table</item>
        /// <item>Release the lock and return with OK message</item>
        /// </list>
        /// </remarks>
        /// <param name="id">ID of category</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> DeleteCategoryAsync(int id)
        {
            // Message for response
            Message respond = new Message();

            // Make lock for database to prevent changes meanwhile it works
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Query based on category name
            var record = await _context.Categories.SingleOrDefaultAsync(s => s.Id == id);

            // If specified category did not exist, return with NOK message
            if (record == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Specified category, by '{id}' ID, did not exist";
                transaction.Rollback();
                return respond;
            }

            // Else save information about record and perform remove and save
            string cat_name = $"{record.Id} - {record.Name}";
            _context.Categories.Remove(record);
            await _context.SaveChangesAsync();

            // Release the lock, commit the changes
            transaction.Commit();

            // Everything is fine, let us send an OK message
            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Category ({cat_name}) has been deleted";

            return respond;
        }

        /// <summary>
        /// Create a list about the existing categories
        /// </summary>
        /// <returns>List about categories</returns>
        public async Task<List<Category>> ListCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        #endregion

        #region HealtCheck
        /// <summary>
        /// This method is use to apply for a functional check. It returns with an OK message
        /// </summary>
        /// <returns>Type: Message</returns>
        public Message HealthCheck()
        {
            Message hc = new Message();
            hc.MessageType = MessageType.OK;
            hc.MessageText = "API is alive";

            return hc;
        }
        #endregion

        #region TicketStuff
        /// <summary>
        /// This method list all ticket from the database without any filtering
        /// </summary>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<TicketHeader>> ListTicketsAsync()
        {
            var data = await (from t in _context.Tickets 
                              join c in _context.Categories on t.Category equals c.Id
                              orderby t.Id ascending
                              select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                             .ToListAsync();
            return data;
        }

        /// <summary>
        /// Return with a list from ticket without any filter, but in limtied row numbers
        /// </summary>
        /// <param name="from">Skip value</param>
        /// <param name="count">Take value</param>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<TicketHeader>> ListTicketsAsync(int from, int count)
        {
            var data = await (from t in _context.Tickets 
                              join c in _context.Categories on t.Category equals c.Id
                              orderby t.Id ascending
                              select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                             .Skip(from)
                             .Take(count)
                             .ToListAsync();
            return data;
        }

        /// <summary>
        /// List tickets by filtered with input template
        /// </summary>
        /// <remarks>
        /// Possible filter options:
        /// <list type="bullet">
        /// <item>Category (exact match)</item>
        /// <item>Reference (contains)</item>
        /// <item>Status (exact match)</item>
        /// <item>Title (contains)</item>
        /// </list>
        /// </remarks>
        /// <param name="filter">Filter object</param>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<TicketHeader>> ListTicketsAsync(TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category != "" && filter.Status == "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category == "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
        }

        /// <summary>
        /// List tickets by filtered with input template with specified limit number
        /// </summary>
        /// <remarks>
        /// Possible filter options:
        /// <list type="bullet">
        /// <item>Category (exact match)</item>
        /// <item>Reference (contains)</item>
        /// <item>Status (exact match)</item>
        /// <item>Title (contains)</item>
        /// </list>
        /// </remarks>
        /// <param name="filter">Filter object</param>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<TicketHeader>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .Skip(from)
                                 .Take(count)
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category != "" && filter.Status == "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .Skip(from)
                                 .Take(count)
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category == "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .Skip(from)
                                 .Take(count)
                                 .ToListAsync();
                return data;
            }
            else
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
                                  orderby t.Id ascending
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .Skip(from)
                                 .Take(count)
                                 .ToListAsync();
                return data;
            }
        }

        /// <summary>
        /// Method to creat ticket
        /// </summary>
        /// <remarks>
        /// Following information are mandatory to create ticket:
        /// <list type="bullet">
        /// <item>Summary</item>
        /// <item>Category</item>
        /// <item>Rerence</item>
        /// <item>Details (optional)</item>
        /// <item>Title</item>
        /// </list>
        /// </remarks>
        /// <param name="input">Input object</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> CreateTicketAsync(TicketCreationTemplate input)
        {
            // Create object for respond message
            Message respond = new Message();

            // Validates data from the request
            string missing = "";

            if (input.Category == "")
                missing += $"Category;";
            if (input.Details == "")
                input.Details = input.Summary;
            if(input.Reference == "")
                missing += $"Reference;";
            if(input.Summary == "")
                missing += $"Summary;";
            if(input.Title == "")
                missing += $"Summary;";

            if(missing != "")
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Missing values: {missing}";
                return respond;
            }

            // Make a lock for database
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction;
            bool release = true;
            if (_context.Database.CurrentTransaction == null)
                transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            else
            {
                release = false;
                transaction = _context.Database.CurrentTransaction;
            }


            // Validate and looking for category number
            var categories = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(input.Category));
            if(categories == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Specified group ({input.Category}) did not find";

                if(release)
                    transaction.Rollback();

                return respond;
            }

            // Everything seems OK, let us create TicketData and Log entries
            TicketData newTicketData = new TicketData()
            {
                Category = categories.Id,
                Reference = input.Reference,
                Status = "Open",
                Title = input.Title,
                Time = DateTime.Now.ToString("yyyyMMddHHmmss")
            };
            Log newLog = new Log()
            {
                 Details = input.Details,
                 Summary = input.Summary,
                 Ticket = newTicketData,   
                 Time = DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            // Check that it is an update. If does, then change Ticket of log
            var existTicket = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(input.Reference) && s.Status.Equals("Open"));

            if(existTicket != null)
            {
                // Ticket for this case is already opened, insert log only
                newLog.Ticket = existTicket;
                await _context.Logs.AddAsync(newLog);
            }
            else
            {
                // Ticket for this case is not opened, insert log and ticket data too
                await _context.Tickets.AddAsync(newTicketData);
                await _context.Logs.AddAsync(newLog);
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Change is done, release the lock
            if(release)
                transaction.Commit();

            // Assemble OK message and send back
            respond.MessageType = MessageType.OK;
            respond.MessageText = "Ticket has been opened";

            return respond;
        }

        /// <summary>
        /// Close ticket based on ID
        /// </summary>
        /// <remarks>
        /// This method change the ticket status from 'Open' to 'Close' based on the specified ID
        /// </remarks>
        /// <param name="id">Ticket ID which need to be closed</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> CloseTicketAsync(int id)
        {
            // Return value
            Message respond = new Message();

            // Check that ID is opened
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Id == id && s.Status == "Open");

            // If there is no opened ticket, then return NOK message
            if(record == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Open ticket was not found with {id} ID";
                return respond;
            }

            // Make a lock for the table to prevent concurrent changes
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Change the status and update the database
            record.Status = "Close";
            await _context.SaveChangesAsync();

            // Release the lock and return with OK message
            transaction.Commit();

            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Ticket with {id} ID has been closed";

            return respond;
        }

        /// <summary>
        /// Close ticket based on reference value
        /// </summary>
        /// <remarks>
        /// This method change the ticket status from 'Open' to 'Close' based on the specified reference value
        /// </remarks>
        /// <param name="referenceValue">Reference value</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> CloseTicketAsync(string referenceValue)
        {
            // Return value
            Message respond = new Message();

            // Check that ID is opened
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference == referenceValue && s.Status == "Open");

            // If there is no opened ticket, then return NOK message
            if (record == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Open ticket was not found with {referenceValue} reference value";
                return respond;
            }

            // Make a lock for the table to prevent concurrent changes
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Change the status and update the database
            record.Status = "Close";
            await _context.SaveChangesAsync();

            // Release the lock and return with OK message
            transaction.Commit();

            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Ticket with {referenceValue} refrence vaue has been closed";

            return respond;
        }

        /// <summary>
        /// Change ticket header information
        /// </summary>
        /// <remarks>
        /// By this method, change header can be adjusted for Category, Title and Reference values
        /// </remarks>
        /// <param name="newValues">Template for changed tickets</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> ChangeTicketAsync(TicketChangeTemplate newValues)
        {
            // Return value
            Message respond = new Message();

            // Initial log entry about the change
            string changeLog = $"Changed values:{Environment.NewLine}";

            // Check ID is specified
            if(newValues.Id < 0)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"ID is not specified";
                return respond;
            }

            // Make a lock to avoid concurrent changes
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Check that ticket exist with specified ID
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Id == newValues.Id && s.Status == "Open");

            // If it does not, return with NOK message
            if(record == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Open ticket did not found with the specified ID: {newValues.Id}";
                transaction.Rollback();
                return respond;
            }

            string ticketCategory = "";
            // Change the values and record them into a new log
            if(newValues.Category != "")
            {
                // Check that category exist
                var catRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Name == newValues.Category);
                
                if(catRecord == null)
                {
                    respond.MessageType = MessageType.NOK;
                    respond.MessageText = $"Category {newValues.Category} does not exist, changes are undo";
                    transaction.Rollback();
                    return respond;
                }

                // Do the change
                ticketCategory = catRecord.Name;
                changeLog += $"Category from {record.Category} to {newValues.Category}{Environment.NewLine}";
                record.Category = catRecord.Id;
            }

            if(newValues.Refernce != "")
            {
                changeLog += $"Refernce is changed from {record.Reference} to {newValues.Refernce}{Environment.NewLine}";
                record.Reference = newValues.Refernce;
            }

            if(newValues.Title != "")
            {
                changeLog += $"Title is changed from '{record.Title}' to '{newValues.Title}'{Environment.NewLine}";
            }

            // Now ticket header is updated, put a new log udner this
            TicketCreationTemplate newLog = new TicketCreationTemplate();
            newLog.Category = ticketCategory;
            newLog.Details = changeLog;
            newLog.Reference = record.Reference;
            newLog.Summary = "Ticket has been adjusted";
            newLog.Title = record.Title;

            // Add the log entry for the ticket
            Message crtTicket = await CreateTicketAsync(newLog);
            if(crtTicket.MessageType != MessageType.OK)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"During the change, log udpate was unsuccessful, changes are undo";
                transaction.Rollback();
                return respond;
            }

            // Seems everything OK, commit the changes and return a OK message
            transaction.Commit();

            respond.MessageType = MessageType.OK;
            respond.MessageText = $"Ticket ({newValues.Id}) has been changed";

            return respond;
        }

        /// <summary>
        /// Get details
        /// </summary>
        /// <remarks>
        /// This method, return with details. Details contains:
        /// <list type="bullet">
        /// <item>Ticket header</item>
        /// <item>Log list which are blongs to the ticket</item>
        /// </list>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>With Ticket is OK, else with null</returns>
        public async Task<Ticket> GetDetailsAsync(int id)
        {
            // Create the return value
            Ticket respond = new Ticket();

            // Check that ID exist
            var header = await (from t in _context.Tickets
                                where t.Id == id
                                select new TicketData { Category = t.Category, Id = t.Id, Reference = t.Reference, Status = t.Status, Time = t.Time, Title = t.Title })
                               .SingleOrDefaultAsync();

            if(header == null)
            {
                return null;
            }

            // Query logs
            var logs = await (from l in _context.Logs
                              where l.Ticket == header
                              select new Log { Details = l.Details, Id = l.Id, Summary = l.Summary, Ticket = l.Ticket, Time = l.Time })
                            .ToListAsync();

            // Create the return object
            respond.Logs = logs;

            respond.Header = new TicketHeader();
            respond.Header.Id = header.Id;
            respond.Header.Reference = header.Reference;
            respond.Header.Status = header.Status;
            respond.Header.Time = header.Time;
            respond.Header.Title = header.Title;

            var catRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Id == header.Category);
            respond.Header.Category = catRecord.Name;

            return respond;
        }
        #endregion
    }
}
