using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Model;
using DatabaseController.DataModel;

namespace DatabaseController.Interface
{
    public interface IDbHandler
    {
        #region Attribures and utilities
        Message HealthCheck();

        string GetConnectionString();

        Task<Message> AssignUserToCategory(Category category, User user);

        Task<Message> UnassignUserToCategory(Category category, User user);

        Task<Message> AssignUserToTicketAsync(User user, Ticket ticket);

        Task<Message> ChangeUserRole(User user, UserRole role);

        #endregion

        #region System stuff

        Task<Message> AddSystemAsync(string sysname);

        Task<DataModel.System> GetSystemAsync(string sysname);

        Task<List<DataModel.System>> GetSystemsAsync();

        Task<Message> RemoveSystemAsync(string sysname);

        Task<Message> RenameSystemAsync(string sysname, string newname);

        #endregion

        #region Category stuff
        Task<List<DataModel.Category>> ListCategoriesAsync();

        Task<List<DataModel.Category>> ListCategoriesAsync(User user);

        Task<List<DataModel.Category>> ListCategoriesAsync(DataModel.System system);

        Task<Message> AddCategoryAsync(string category, DataModel.System system);

        Task<Message> RenameCategoryAsync(string from, string to, DataModel.System system);

        Task<Message> DeleteCategoryAsync(string name, DataModel.System system);

        Task<Message> DeleteCategoryAsync(int id);

        Task<Category> GetCategoryAsync(int id, DataModel.System system);

        Task<Category> GetCategoryAsync(string name, DataModel.System system);
        #endregion

        #region Ticket stuff
        Task<List<DataModel.Ticket>> ListTicketsAsync();

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count);

        Task<List<DataModel.Ticket>> ListTicketsAsync(TicketFilterTemplate filter);

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter);

        Task<DataModel.Ticket> GetTicketAsync(int id);

        Task<Message> CreateTicketAsync(TicketCreationTemplate input);

        Task<Message> CloseTicketAsync(int id);

        Task<Message> CloseTicketAsync(string referenceValue, string sysname);

        Task<Message> ChangeTicketAsync(TicketChangeTemplate newValues);

        Task<TicketDetails> GetDetailsAsync(int id);
        #endregion

        #region User stuff
        Task<Message> RegisterUserAsync(User user);

        Task<Message> RemoveUserAsync(int id);

        Task<Message> RemoveUserAsync(string username);

        Task<Message> ChangeUserAsync(int id, User user);

        Task<Message> ChangeUserAsync(string username, User user);

        Task<string> GetHashedPasswordAsync(int id);

        Task<string> GetHashedPasswordAsync(string username);

        Task<User> GetUserAsync(string username);

        Task<User> GetUserAsync(int id);

        Task<List<User>> GetUsersAsync();

        Task<List<User>> GetUsersAsync(Category category);

        Task<List<User>> GetUsersAsync(DataModel.System system);
        #endregion
    }
}
