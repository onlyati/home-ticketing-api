using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseController.Interface;
using DatabaseController.Model;
using System.Threading.Tasks;
using DatabaseController.DataModel;
using System.Security.Cryptography;
using System.Text;

namespace DatabaseController.Controller
{
    public class TicketHandler : ITicketHandler
    {
        private string _connectionString;
        private TicketDatabase _context;

        #region Constructor
        /// <summary>
        /// Constructor for creating TicketHandler object. This object can be used for any pre-defined action with database.
        /// </summary>
        /// <param name="connectionString">Database connection string, used by EF</param>
        public TicketHandler(string connectionString)
        {
            // Read input string and try to establish a connection
            if(connectionString == "")
            {
                throw new NullReferenceException("Connection string is missing in the constructor");
            }

            _connectionString = connectionString;
            var optionsBuilder = new DbContextOptionsBuilder<TicketDatabase>();
            optionsBuilder.UseNpgsql(connectionString);
            _context = new TicketDatabase(optionsBuilder.Options);

            // Validate the connection
            try
            {
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
        #endregion

        #region Attributes and utilities
        /// <summary>
        /// This function returns with the specified connection string
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// This function helps to create relationship between user and category
        /// </summary>
        /// <param name="category">Category record</param>
        /// <param name="user">user record</param>
        /// <returns>With OK or NOK message</returns>
        public async Task<Message> AssignUserToCategory(Category category, User user)
        {
            // Object which will return
            Message response = new Message();

            // Check that either category and user exist
            var waitCat = _context.Categories.SingleOrDefaultAsync(s => s.Equals(category));
            var waitkUsr = _context.Users.SingleOrDefaultAsync(s => s.Equals(user));

            var checkCat = await waitCat;
            var checkUsr = await waitkUsr;

            if(checkCat == null || checkUsr == null)
            {
                response.MessageText = "User and/or category did not found";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point the database is eligible for assigment, let's try it
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                Usercategory newUsrCat = new Usercategory();
                newUsrCat.CategoryId = category.Id;
                newUsrCat.UserId = user.Id;

                _context.Usercategories.Add(newUsrCat);
                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// This function helps to remove relationship between user and category
        /// </summary>
        /// <param name="category">Category record</param>
        /// <param name="user">User record</param>
        /// <returns>With OK or NOK message</returns>
        public async Task<Message> UnassignUserToCategory(Category category, User user)
        {
            // Object which will return
            Message response = new Message();

            // Check that assigment exist
            var checkRel = await _context.Usercategories.SingleOrDefaultAsync(s => s.CategoryId.Equals(category.Id) && s.UserId.Equals(user.Id));
            if(checkRel == null)
            {
                response.MessageText = "Relationship does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point the database is eligible for adding new user, let's try it
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var delRel = await _context.Usercategories.SingleOrDefaultAsync(s => s.CategoryId.Equals(category.Id) && s.UserId.Equals(user.Id));
                _context.Usercategories.Remove(delRel);
                _context.SaveChanges();
                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// This function helps to assing user to a ticket
        /// </summary>
        /// <param name="user">User record</param>
        /// <param name="ticket">Ticket record</param>
        /// <returns></returns>
        public async Task<Message> AssignUserToTicketAsync(User user, Ticket ticket)
        {
            // Object which will return
            Message response = new Message();

            // Check that things exist
            var waitUser = _context.Users.SingleOrDefaultAsync(s => s.Equals(user));
            var waitTicket = _context.Tickets.SingleOrDefaultAsync(s => s.Equals(ticket));
            var waitCat = _context.Categories.SingleOrDefaultAsync(s => s.Id.Equals(ticket.CategoryId));

            var checkUser = await waitUser;
            var checkTicket = await waitTicket;

            if(checkUser == null || checkTicket == null)
            {
                response.MessageText = "User or ticket does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // Check that user has access for the category
            var checkCat = await waitCat;

            var checkUsrCat = await _context.Usercategories.SingleOrDefaultAsync(s => s.UserId.Equals(checkUser.Id) && s.CategoryId.Equals(checkCat.Id));
            if(checkUsrCat == null)
            {
                response.MessageText = "User is not member of category where the ticket actually assigned";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point the database is eligible for adding new user, let's try it
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var changeTicket = await _context.Tickets.SingleOrDefaultAsync(s => s.Equals(ticket));
                changeTicket.UserId = user.Id;
                _context.SaveChanges();
                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// This function performa SHA512 on the specified password and return with its hexa values
        /// </summary>
        /// <param name="pw">Original password</param>
        /// <returns></returns>
        public static string HashPassword(string pw)
        {
            var origPwBytes = Encoding.UTF8.GetBytes(pw);

            SHA512 hashPw = new SHA512Managed();
            var hashPwBytes = hashPw.ComputeHash(origPwBytes);

            // Size is 128 because SHA512, 512 bit which is 64 bytes, but due to hexa it is doubled
            var hashHexa = new StringBuilder(128);
            foreach (var b in hashPwBytes)
            {
                hashHexa.Append(b.ToString("X2"));
            }

            return hashHexa.ToString();
        }
        #endregion

        #region System stuff

        /// <summary>
        /// This method can be used to add new system
        /// </summary>
        /// <remarks>
        /// It returns with OK if system did not exist and add action has been successfully ended. Else it return with a NOK message.
        /// </remarks>
        /// <param name="sysname">New system name</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> AddSystemAsync(string sysname)
        {
            // Message which will be sent back
            Message response = new Message();
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Check that ticket already exist
                var record = await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);
                if(record != null)
                {
                    // It is already exist, send back message
                    transaction.Rollback();
                    response.MessageType = MessageType.NOK;
                    response.MessageText = $"System already defined into database with id={record.Id}";
                    return response;
                }

                // Add new system
                DataModel.System newSys = new DataModel.System();
                newSys.Name = sysname;

                _context.Add(newSys);
                _context.SaveChanges();

                // Everything was fine
                transaction.Commit();

                response.MessageType = MessageType.OK;
                response.MessageText = $"System ({sysname}) has been added";

                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// Method to check that system is exist
        /// </summary>
        /// <param name="sysname">System name</param>
        /// <returns>Null if record does not exist, else with object</returns>
        public async Task<DataModel.System> GetSystemAsync(string sysname)
        {
            return await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);
        }

        /// <summary>
        /// Method to list all defined systems
        /// </summary>
        /// <returns>List about the defined system</returns>
        public async Task<List<DataModel.System>> GetSystemsAsync()
        {
            return await _context.Systems.ToListAsync();
        }

        /// <summary>
        /// Method to delete a system
        /// </summary>
        /// <param name="sysname">System name which need to be removed</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> RemoveSystemAsync(string sysname)
        {
            // Create object which will return
            Message respond = new Message();

            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Check that system exist
                var record = await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);
                if(record == null)
                {
                    // No exist, thus cannot be deleted
                    transaction.Rollback();
                    respond.MessageType = MessageType.NOK;
                    respond.MessageText = $"System not defined into database";
                    return respond;
                }

                // Remove from database
                _context.Systems.Remove(record);
                _context.SaveChanges();

                // It run OK
                transaction.Commit();

                respond.MessageType = MessageType.OK;
                respond.MessageText = $"System ({sysname}) has been removed";

                return respond;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Internal error: {ex.Message}";
                return respond;
                throw;
            }
        }
        #endregion

        #region Category stuff
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
        public async Task<Message> AddCategoryAsync(string category, string sysname)
        {
            // Message which will be sent back
            Message response = new Message();

            // Make lock for database to prevent changes meanwhile it works
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Check if already exist
                var record = await (from c in _context.Categories
                                    join s in _context.Systems on c.SystemId equals s.Id
                                    select new DataModel.Category
                                    {
                                        Id = c.Id,
                                        Name = c.Name,
                                        System = s,
                                        SystemId = c.SystemId
                                    }).SingleOrDefaultAsync(s => s.Name == category && s.System.Name == sysname);

                if(record != null)
                {
                    response.MessageType = MessageType.NOK;
                    response.MessageText = $"Category ({category}) is already exist on {sysname} system";
                    transaction.Rollback();
                    return response;
                }

                // Create a new category
                DataModel.Category new_cat = new DataModel.Category();
                new_cat.Name = category;
                new_cat.System = await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);

                // Add the category to database
                await _context.AddAsync(new_cat);
                await _context.SaveChangesAsync();

                // Complete the transaction, so other request can reach it
                transaction.Commit();

                response.MessageType = MessageType.OK;
                response.MessageText = $"Category ({category}) has been added for {sysname} system";

                return response;
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
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
        public async Task<Message> RenameCategoryAsync(string from, string to, string sysname)
        {
            // Object for return value
            Message respond = new Message();

            // Start a transaction, so nothing else will change the table while we are working
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Check that category exist
                var fromRecord = await (from c in _context.Categories
                                        join s in _context.Systems on c.SystemId equals s.Id
                                        select new DataModel.Category
                                        {
                                            Id = c.Id,
                                            Name = c.Name,
                                            SystemId = c.SystemId,
                                            System = s
                                        }).SingleOrDefaultAsync(s => s.Name == from && s.System.Name == sysname);

                // If 'from' does not exist, return with error
                if (fromRecord == null)
                {
                    respond.MessageType = MessageType.NOK;
                    respond.MessageText = $"The specified category ({from}) does not exist for {sysname} system, thus not possible to rename it";
                    transaction.Rollback();
                    return respond;
                }

                // Query the 'to' category name
                var toRecord = await (from c in _context.Categories
                                      join s in _context.Systems on c.SystemId equals s.Id
                                      select new DataModel.Category
                                      {
                                          Id = c.Id,
                                          Name = c.Name,
                                          SystemId = c.SystemId,
                                          System = s
                                      }).SingleOrDefaultAsync(s => s.Name == to && s.System.Name == sysname);

                // if 'to' does exist, return with error
                if (toRecord != null)
                {
                    respond.MessageType = MessageType.NOK;
                    respond.MessageText = $"The specified new name ({to}) is already exist for {sysname} system";
                    transaction.Rollback();
                    return respond;
                }

                var changeRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Name == from && s.SystemId == fromRecord.System.Id);

                // Update the fromRecord, then save the changes
                changeRecord.Name = to;
                await _context.SaveChangesAsync();

                // Commit the transaction, then send the OK message 
                transaction.Commit();

                respond.MessageType = MessageType.OK;
                respond.MessageText = $"Category rename from '{from}' to '{to}' is done";

                return respond;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Internal error: {ex.Message}";
                return respond;
                throw;
            }
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
        public async Task<Message> DeleteCategoryAsync(string name, string sysname)
        {
            // Message for response
            Message respond = new Message();

            // Make lock for database to prevent changes meanwhile it works
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var sys = await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);
                var record = await _context.Categories.SingleOrDefaultAsync(s => s.Name == name && s.SystemId == sys.Id);

                /*
                var record = await (from c in _context.Categories
                                    join s in _context.Systems on c.SystemId equals s.Id
                                    select new Category
                                    {
                                        Id = c.Id,
                                        Name = c.Name,
                                        SystemId = c.SystemId,
                                        System = s
                                    }).SingleOrDefaultAsync(s => s.Name == name && s.System.Name == sysname);
                */
                // If specified category did not exist, return with NOK message
                if (record == null)
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
                respond.MessageText = $"Category ({cat_name}) has been deleted on {sysname}";

                return respond;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Internal error: {ex.Message}";
                return respond;
                throw;
            }
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

            try
            {
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
            catch (Exception ex)
            {
                transaction.Rollback();
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Internal error: {ex.Message}";
                return respond;
                throw;
            }
        }

        /// <summary>
        /// Create a list about the existing categories
        /// </summary>
        /// <returns>List about categories</returns>
        public async Task<List<DataModel.Category>> ListCategoriesAsync()
        {
            return await (from c in _context.Categories
                          join s in _context.Systems on c.SystemId equals s.Id
                          orderby c.Id ascending
                          select new DataModel.Category
                          {
                              Id = c.Id,
                              Name = c.Name,
                              SystemId = c.SystemId,
                              System = s
                          }).ToListAsync();
        }

        /// <summary>
        /// Return with a Category record based on ID and system name
        /// </summary>
        /// <param name="id">Category ID number</param>
        /// <param name="sysname">System name</param>
        /// <returns>Category object or null</returns>
        public async Task<Category> GetCategoryAsync(int id, string sysname)
        {
            return await (from c in _context.Categories 
                          join s in _context.Systems on c.SystemId equals s.Id
                          where c.Id == id && s.Name == sysname
                          select new Category
                          {
                              Id = c.Id,
                              Name = c.Name,
                              SystemId = c.SystemId,
                              System = s
                          }).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Return with a Category record based on its name and system name
        /// </summary>
        /// <param name="name">Category name</param>
        /// <param name="sysname">System name</param>
        /// <returns>Category object or null</returns>
        public async Task<Category> GetCategoryAsync(string name, string sysname)
        {
            return await (from c in _context.Categories
                          join s in _context.Systems on c.SystemId equals s.Id
                          where c.Name == name && s.Name == sysname
                          select new Category
                          {
                              Id = c.Id,
                              Name = c.Name,
                              SystemId = c.SystemId,
                              System = s
                          }).SingleOrDefaultAsync();
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

        #region Ticket stuff
        /// <summary>
        /// This method list all ticket from the database without any filtering
        /// </summary>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<DataModel.Ticket>> ListTicketsAsync()
        {
            return await (from t in _context.Tickets
                          join c in _context.Categories on t.CategoryId equals c.Id
                          join u in _context.Users on t.UserId equals u.Id into uj
                          from allu in uj.DefaultIfEmpty()
                          join s in _context.Systems on t.SystemId equals s.Id
                          orderby t.Id ascending
                          select new DataModel.Ticket
                          {
                              Id = t.Id,
                              Category = c,
                              Reference = t.Reference,
                              Status = t.Status,
                              Time = t.Time,
                              Title = t.Title,
                              User = allu,
                              System = s
                          }).ToListAsync();
        }

        /// <summary>
        /// Return with a list from ticket without any filter, but in limtied row numbers
        /// </summary>
        /// <param name="from">Skip value</param>
        /// <param name="count">Take value</param>
        /// <returns>With a TicketHeader list</returns>
        public async Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count)
        {
            return await (from t in _context.Tickets
                          join c in _context.Categories on t.CategoryId equals c.Id
                          join u in _context.Users on t.UserId equals u.Id into uj
                          from allu in uj.DefaultIfEmpty()
                          join s in _context.Systems on t.SystemId equals s.Id
                          orderby t.Id ascending
                          select new DataModel.Ticket
                          {
                              Id = t.Id,
                              Category = c,
                              Reference = t.Reference,
                              Status = t.Status,
                              Time = t.Time,
                              Title = t.Title,
                              User = allu,
                              System = s
                          }).Skip(from).Take(count).ToListAsync();
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
        public async Task<List<DataModel.Ticket>> ListTicketsAsync(TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Reference) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).ToListAsync();
            }
            else if (filter.Category != "" && filter.Status == "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Reference) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).ToListAsync();
            }
            else if (filter.Category == "" && filter.Status != "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where t.Reference.Contains(filter.Reference) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).ToListAsync();
            }
            else
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where t.Reference.Contains(filter.Reference) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).ToListAsync();
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
        public async Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter)
        {
            if (filter.Category != "" && filter.Status != "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Reference) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).Skip(from).Take(count).ToListAsync();
            }
            else if (filter.Category != "" && filter.Status == "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where c.Name.Equals(filter.Category) && t.Reference.Contains(filter.Reference) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).Skip(from).Take(count).ToListAsync();
            }
            else if (filter.Category == "" && filter.Status != "")
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where t.Reference.Contains(filter.Reference) && t.Status.Equals(filter.Status) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).Skip(from).Take(count).ToListAsync();
            }
            else
            {
                return await (from t in _context.Tickets
                              join c in _context.Categories on t.CategoryId equals c.Id
                              join u in _context.Users on t.UserId equals u.Id into uj
                              from allu in uj.DefaultIfEmpty()
                              join s in _context.Systems on t.SystemId equals s.Id
                              where t.Reference.Contains(filter.Reference) && t.Title.Contains(filter.Title) && s.Name.Equals(filter.System)
                              orderby t.Id ascending
                              select new DataModel.Ticket
                              {
                                  Id = t.Id,
                                  Category = c,
                                  Reference = t.Reference,
                                  Status = t.Status,
                                  Time = t.Time,
                                  Title = t.Title,
                                  User = allu,
                                  System = s
                              }).Skip(from).Take(count).ToListAsync();
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
            if (input.System == "")
                missing += $"System";

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
            var categories = await (from c in _context.Categories
                                    join s in _context.Systems on c.SystemId equals s.Id
                                    select new DataModel.Category
                                    {
                                        Id = c.Id,
                                        Name = c.Name,
                                        SystemId = c.SystemId,
                                        System = s
                                    }).SingleOrDefaultAsync(s => s.Name == input.Category && s.System.Name == input.System);
            if(categories == null)
            {
                respond.MessageType = MessageType.NOK;
                respond.MessageText = $"Specified group ({input.Category}) did not find for {input.System} system";

                if(release)
                    transaction.Rollback();

                return respond;
            }

            // Everything seems OK, let us create TicketData and Log entries
            DataModel.Ticket newTicket = new DataModel.Ticket();
            newTicket.CategoryId = categories.Id;
            newTicket.Reference = input.Reference;
            newTicket.Status = "Open";
            newTicket.Title = input.Title;
            newTicket.Time = DateTime.Now;
            newTicket.System = await _context.Systems.SingleOrDefaultAsync(s => s.Id == categories.System.Id);
            newTicket.User = null;

            DataModel.Log newLog = new DataModel.Log();
            newLog.Details = input.Details;
            newLog.Summary = input.Summary;
            newLog.Time = DateTime.Now;

            // Check that it is an update. If does, then change Ticket of log
            var existTicket = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(input.Reference) && s.Status.Equals("Open") && s.SystemId == categories.System.Id);

            if(existTicket != null)
            {
                // Ticket for this case is already opened, insert log only
                newLog.Ticket = existTicket;
                await _context.Logs.AddAsync(newLog);
            }
            else
            {
                // Ticket for this case is not opened, insert log and ticket data too
                await _context.Tickets.AddAsync(newTicket);
                await _context.SaveChangesAsync();
                newLog.Ticket = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference.Equals(newTicket.Reference) && s.Status.Equals("Open") && s.SystemId == categories.System.Id);
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
        public async Task<Message> CloseTicketAsync(string referenceValue, string sysname)
        {
            // Return value
            Message respond = new Message();

            var sys = await _context.Systems.SingleOrDefaultAsync(s => s.Name == sysname);

            // Check that ID is opened
            var record = await _context.Tickets.SingleOrDefaultAsync(s => s.Reference == referenceValue && s.Status == "Open" && s.SystemId == sys.Id);

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
            var record = await (from t in _context.Tickets
                                join c in _context.Categories on t.CategoryId equals c.Id
                                join u in _context.Users on t.UserId equals u.Id into uj
                                from allu in uj.DefaultIfEmpty()
                                join s in _context.Systems on t.SystemId equals s.Id
                                select new DataModel.Ticket
                                {
                                    Id = t.Id,
                                    Category = c,
                                    CategoryId = t.CategoryId,
                                    Reference = t.Reference,
                                    Status = t.Status,
                                    Time = t.Time,
                                    Title = t.Title,
                                    User = allu,
                                    System = s
                                }).SingleOrDefaultAsync(s => s.Id == newValues.Id && s.Status == "Open");

            var updRecord = await _context.Tickets.SingleOrDefaultAsync(s => s.Id == newValues.Id && s.Status == "Open");

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
                var catRecord = await _context.Categories.SingleOrDefaultAsync(s => s.Name == newValues.Category && s.SystemId == record.System.Id);
                
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
                updRecord.CategoryId = catRecord.Id;
            }

            if(newValues.Refernce != "")
            {
                changeLog += $"Refernce is changed from {record.Reference} to {newValues.Refernce}{Environment.NewLine}";
                updRecord.Reference = newValues.Refernce;
            }

            if(newValues.Title != "")
            {
                changeLog += $"Title is changed from '{record.Title}' to '{newValues.Title}'{Environment.NewLine}";
                updRecord.Title = newValues.Title;
            }

            _context.SaveChanges();

            // Now ticket header is updated, put a new log under this
            TicketCreationTemplate newLog = new TicketCreationTemplate();
            newLog.Category = ticketCategory;
            newLog.Details = changeLog;
            newLog.Reference = record.Reference;
            newLog.Summary = "Ticket has been adjusted";
            newLog.Title = record.Title;
            newLog.System = record.System.Name;

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
        public async Task<TicketDetails> GetDetailsAsync(int id)
        {
            TicketDetails respond = new TicketDetails();
            // Looking for ticket
            respond.Header = await (from t in _context.Tickets
                                    join c in _context.Categories on t.CategoryId equals c.Id
                                    join u in _context.Users on t.UserId equals u.Id into uj
                                    from allu in uj.DefaultIfEmpty()
                                    join s in _context.Systems on t.SystemId equals s.Id
                                    select new DataModel.Ticket
                                    {
                                        Id = t.Id,
                                        Category = c,
                                        Reference = t.Reference,
                                        Status = t.Status,
                                        Time = t.Time,
                                        Title = t.Title,
                                        User = allu,
                                        System = s
                                    }).SingleOrDefaultAsync(s => s.Id == id);
            if(respond.Header == null)
            {
                return null;
            }

            respond.Logs = await (from l in _context.Logs
                                  join t in _context.Tickets on l.TicketId equals t.Id
                                  join u in _context.Users on l.UserId equals u.Id into uj
                                  from allu in uj.DefaultIfEmpty()
                                  where l.TicketId == respond.Header.Id
                                  orderby l.Id ascending
                                  select new DataModel.Log
                                  {
                                      Id = l.Id,
                                      Summary = l.Summary,
                                      Ticket = t,
                                      Details = l.Details,
                                      Time = l.Time,
                                      User = allu
                                  }).ToListAsync();

            return respond;
        }
        
        /// <summary>
        /// This method return with a Ticket header based on its ID
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <returns></returns>
        public async Task<Ticket> GetTicketAsync(int id)
        {
            return await _context.Tickets.SingleOrDefaultAsync(s => s.Id.Equals(id));
        }


        #endregion

        #region User stuff

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="user">User object which must contains every data</param>
        /// <returns>OK or NOK message</returns>
        public async Task<Message> RegisterUserAsync(User user)
        {
            // Object which will return
            Message response = new Message();
            response.MessageText = "";

            // Check that all values are provided and return with NOK if something missing
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrWhiteSpace(user.Username))
                response.MessageText += "Username value is missing; ";

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrWhiteSpace(user.Email))
                response.MessageText += "Email value is missing;";

            if (string.IsNullOrEmpty(user.Password) || string.IsNullOrWhiteSpace(user.Password))
                response.MessageText += "Password value is missing";

            if(response.MessageText != "")
            {
                response.MessageType = MessageType.NOK;
                return response;
            }

            // Check if user already exist
            var testUser = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(user.Username));
            if(testUser != null)
            {
                response.MessageText = "Username already reserved";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point the database is eligible for adding new user, let's try it
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Create password hash
                user.Password = HashPassword(user.Password);

                await _context.AddAsync(user);
                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// Remove user based on user ID number
        /// </summary>
        /// <param name="id">ID of user</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> RemoveUserAsync(int id)
        {
            // Object which will return
            Message response = new Message();

            // Check that user exist
            var testUser = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
            if(testUser == null)
            {
                response.MessageText = "User does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point, database is eligible to remove the user
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Delete the user
                var delUser = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
                _context.Users.Remove(delUser);
                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// Remove used based on its username
        /// </summary>
        /// <param name="username">User's name</param>
        /// <returns>OK or NOK message</returns>
        public async Task<Message> RemoveUserAsync(string username)
        {
            // Object which will return
            Message response = new Message();

            // Check that user exist
            var testUser = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
            if (testUser == null)
            {
                response.MessageText = "User does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point, database is eligible to remove the user
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                // Delete the user
                var delUser = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
                _context.Users.Remove(delUser);
                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// Change user data: email or password. Username must not be changed.
        /// </summary>
        /// <param name="id">ID of the user where needs to change</param>
        /// <param name="user">Contains the new values</param>
        /// <returns>OK or a NOK message</returns>
        public async Task<Message> ChangeUserAsync(int id, User user)
        {
            // Object which will return
            Message response = new Message();

            // Check that user exist
            var testUser = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
            if (testUser == null)
            {
                response.MessageText = "User does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point, database is eligible to remove the user
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var changeUser = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
                if(!string.IsNullOrEmpty(user.Email) && !string.IsNullOrWhiteSpace(user.Email))
                {
                    changeUser.Email = user.Email;
                }

                if(!string.IsNullOrEmpty(user.Password) && !string.IsNullOrWhiteSpace(user.Password))
                {
                    changeUser.Password = HashPassword(user.Password);
                }

                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        public async Task<Message> ChangeUserAsync(string username, User user)
        {
            // Object which will return
            Message response = new Message();

            // Check that user exist
            var testUser = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
            if (testUser == null)
            {
                response.MessageText = "User does not exist";
                response.MessageType = MessageType.NOK;
                return response;
            }

            // At this point, database is eligible to remove the user
            var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var changeUser = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
                if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrWhiteSpace(user.Email))
                {
                    changeUser.Email = user.Email;
                }

                if (!string.IsNullOrEmpty(user.Password) && !string.IsNullOrWhiteSpace(user.Password))
                {
                    changeUser.Password = HashPassword(user.Password);
                }

                _context.SaveChanges();

                // Everything was, commit then return with OK value
                transaction.Commit();

                response.MessageText = "New user has been added";
                response.MessageType = MessageType.OK;
                return response;
            }
            catch (Exception ex)
            {
                // Something bad happened
                transaction.Rollback();
                response.MessageType = MessageType.NOK;
                response.MessageText = $"Internal error: {ex.Message}";
                return response;
                throw;
            }
        }

        /// <summary>
        /// This function returns with the hashed password of the user
        /// </summary>
        /// <param name="id">User's ID number</param>
        /// <returns>With hash as string or null in case of error</returns>
        public async Task<string> GetHashedPasswordAsync(int id)
        {
            // Object which will return
            string hashedPw = null;

            try
            {
                // Query user, if does not exist return with error
                var user = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
                if (user == null)
                    return hashedPw;

                // Everything was OK
                hashedPw = user.Password;
                return hashedPw;
            }
            catch (Exception ex)
            {
                // Something bad happened
                return hashedPw;
                throw;
            }
        }

        /// <summary>
        /// This function returns with the hashed password of the user
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>Witht hash as string or null in case of error</returns>
        public async Task<string> GetHashedPasswordAsync(string username)
        {
            // Object which will return
            string hashedPw = null;

            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
                if (user == null)
                    return hashedPw;

                // Everything was OK
                hashedPw = user.Password;
                return hashedPw;
            }
            catch (Exception ex)
            {
                // Something bad happened
                return hashedPw;
                throw;
            }
        }

        /// <summary>
        /// Return with all deatil of a user
        /// </summary>
        /// <param name="username">User's name</param>
        /// <returns>If OK then User object, else null</returns>
        public async Task<User> GetUserAsync(string username)
        {
            // Object which will return
            User respond = null;

            try
            {
                respond = await _context.Users.SingleOrDefaultAsync(s => s.Username.Equals(username));
                // Everything was OK
                return respond;
            }
            catch (Exception ex)
            {
                // Something bad happened
                return null;
                throw;
            }
        }

        /// <summary>
        /// Return with all deatil of a user
        /// </summary>
        /// <param name="id">User's ID number</param>
        /// <returns>If OK then User object, else null</returns>
        public async Task<User> GetUserAsync(int id)
        {
            // Object which will return
            User respond = null;

            try
            {
                respond = await _context.Users.SingleOrDefaultAsync(s => s.Id.Equals(id));
                // Everything was OK
                return respond;
            }
            catch (Exception ex)
            {
                // Something bad happened
                return null;
                throw;
            }
        }

        /// <summary>
        /// Provide list of all user with their data
        /// </summary>
        /// <returns>List or null</returns>
        public async Task<List<User>> GetUsersAsync()
        {
            // Object which will return
            List<User> respond = null;

            try
            {
                respond = await _context.Users.ToListAsync();
                if (respond == null)
                    respond = new List<User>();
                // Everything was OK
                return respond;
            }
            catch (Exception ex)
            {
                // Something bad happened
                return null;
                throw;
            }
        }
        #endregion
    }
}
