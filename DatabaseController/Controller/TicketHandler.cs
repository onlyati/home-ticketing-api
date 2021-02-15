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
        public async Task<List<TicketHeader>> ListTickets()
        {
            var data = await (from t in _context.Tickets 
                              join c in _context.Categories on t.Category equals c.Id 
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
        public async Task<List<TicketHeader>> ListTickets(int from, int count)
        {
            var data = await (from t in _context.Tickets 
                              join c in _context.Categories on t.Category equals c.Id 
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
        public async Task<List<TicketHeader>> ListTickets(TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category != "" && filter.Status == "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else if (filter.Category == "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
                                  select new TicketHeader { Id = t.Id, Title = t.Title, Time = t.Time, Category = c.Name, Status = t.Status, Reference = t.Reference })
                                 .ToListAsync();
                return data;
            }
            else
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where t.Reference.Contains(filter.Refernce) && t.Title.Contains(filter.Title)
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
        public async Task<List<TicketHeader>> ListTickets(int from, int count, TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                var data = await (from t in _context.Tickets
                                  join c in _context.Categories on t.Category equals c.Id
                                  where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Refernce) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title)
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
        public async Task<Message> CreateTicket(TicketCreationTemplate input)
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
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            // Validate and looking for category number
            var categories = await _context.Categories.SingleOrDefaultAsync(s => s.Name.Equals(input.Category));
            if(categories == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Specified group ({input.Category}) did not find";
                transaction.Rollback();
                return respond;
            }

            // Everything seems OK, let us create TicketData and Log entries
            TicketData newTicketData = new TicketData()
            {
                Category = categories.Id,
                Reference = input.Reference,
                Status = "Open",
                Title = input.Title
            };
            Log newLog = new Log()
            {
                 Details = input.Details,
                 Summary = input.Summary,
                 Ticket = newTicketData,   
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
            transaction.Commit();

            // Assemble OK message and send back
            respond.MessageType = MessageType.OK;
            respond.MessageText = "Ticket has been opened";

            return respond;
        }

        public async Task<Message> CloseTicket(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> CloseTicket(string referenceValue)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> ChangeTicket(TicketChangeTemplate newValues)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> GetDetails(int id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
