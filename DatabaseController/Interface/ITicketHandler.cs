﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Model;
using DatabaseController.DataModel;

namespace DatabaseController.Interface
{
    public interface ITicketHandler
    {
        #region Attribures and utilities
        Message HealthCheck();

        string GetConnectionString();

        Task<Message> AssignUserToCategory(Category category, User user);

        Task<Message> UnassignUserToCategory(Category category, User user);

        Task<Message> AssignUserToTicketAsync(User user, Ticket ticket);

        #endregion

        #region System stuff

        Task<Message> AddSystemAsync(string sysname);

        Task<DataModel.System> GetSystemAsync(string sysname);

        Task<List<DataModel.System>> GetSystemsAsync();

        Task<Message> RemoveSystemAsync(string sysname);

        #endregion

        #region Category stuff
        Task<List<DataModel.Category>> ListCategoriesAsync();

        Task<Message> AddCategoryAsync(string category, string sysname);

        Task<Message> RenameCategoryAsync(string from, string to, string sysname);

        Task<Message> DeleteCategoryAsync(string name, string sysname);

        Task<Message> DeleteCategoryAsync(int id);
        #endregion

        #region Ticket stuff
        Task<List<DataModel.Ticket>> ListTicketsAsync();

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count);

        Task<List<DataModel.Ticket>> ListTicketsAsync(TicketFilterTemplate filter);

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter);

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
        #endregion
    }
}
