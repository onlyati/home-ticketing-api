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
    }
}
