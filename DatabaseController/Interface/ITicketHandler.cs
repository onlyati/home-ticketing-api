using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Model;

namespace DatabaseController.Interface
{
    public interface ITicketHandler
    {
        Message HealthCheck();

        string GetConnectionString();

        Task<Message> AddSystemAsync(string sysname);

        Task<DataModel.System> GetSystemAsync(string sysname);

        Task<List<DataModel.System>> GetSystemsAsync();

        Task<Message> RemoveSystemAsync(string sysname);

        Task<List<DataModel.Category>> ListCategoriesAsync();

        Task<Message> AddCategoryAsync(string category, string sysname);

        Task<Message> RenameCategoryAsync(string from, string to, string sysname);

        Task<Message> DeleteCategoryAsync(string name, string sysname);

        Task<Message> DeleteCategoryAsync(int id);

        Task<List<DataModel.Ticket>> ListTicketsAsync();

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count);

        Task<List<DataModel.Ticket>> ListTicketsAsync(TicketFilterTemplate filter);

        Task<List<DataModel.Ticket>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter);

        Task<Message> CreateTicketAsync(TicketCreationTemplate input);

        Task<Message> CloseTicketAsync(int id);

        Task<Message> CloseTicketAsync(string referenceValue, string sysname);

        Task<Message> ChangeTicketAsync(TicketChangeTemplate newValues);

        Task<TicketDetails> GetDetailsAsync(int id);
    }
}
