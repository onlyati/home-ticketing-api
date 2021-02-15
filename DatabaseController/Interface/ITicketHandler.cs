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

        Task<List<Category>> ListCategoriesAsync();

        Task<Message> AddCategoryAsync(string category);

        Task<Message> RenameCategoryAsync(string from, string to);

        Task<Message> DeleteCategoryAsync(string name);

        Task<Message> DeleteCategoryAsync(int id);

        Task<List<TicketHeader>> ListTickets();

        Task<List<TicketHeader>> ListTickets(int from, int count);

        Task<List<TicketHeader>> ListTickets(TicketFilterTemplate filter);

        Task<List<TicketHeader>> ListTickets(int from, int count, TicketFilterTemplate filter);

        Task<Message> CreateTicket(TicketCreationTemplate input);

        Task<Message> CloseTicket(int id);

        Task<Message> CloseTicket(string referenceValue);

        Task<Message> ChangeTicket(TicketChangeTemplate newValues);

        Task<Message> GetDetails(int id);
    }
}
