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
    }
}
