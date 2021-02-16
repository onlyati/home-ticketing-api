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

        Task<List<TicketHeader>> ListTicketsAsync();

        Task<List<TicketHeader>> ListTicketsAsync(int from, int count);

        Task<List<TicketHeader>> ListTicketsAsync(TicketFilterTemplate filter);

        Task<List<TicketHeader>> ListTicketsAsync(int from, int count, TicketFilterTemplate filter);

        Task<Message> CreateTicketAsync(TicketCreationTemplate input);

        Task<Message> CloseTicketAsync(int id);

        Task<Message> CloseTicketAsync(string referenceValue);

        Task<Message> ChangeTicketAsync(TicketChangeTemplate newValues);

        Task<Ticket> GetDetailsAsync(int id);
    }
}
