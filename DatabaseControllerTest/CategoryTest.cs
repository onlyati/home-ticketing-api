using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController.Controller;
using DatabaseController.Interface;
using DatabaseController.Model;
using DatabaseController.DataModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DatabaseControllerTest
{
    [TestClass]
    public class CategoryTest
    {
        string connString = "Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test";

        /// <summary>
        /// This method try to list all categories. 
        /// </summary>
        /// <remarks>
        /// Test is successful if:
        /// <list type="bullet">
        /// <item>Returned list has elements</item>
        /// <item>The first element in the list, has ID 1 and Name 'System'</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task List_Categoires()
        {
            TicketHandler ticket = new TicketHandler(connString);
            List<Category> list = await ticket.ListCategoriesAsync();

            Assert.AreNotEqual(0, list.Count, "List is empty, there are no categories");
            Assert.AreEqual(148, list[0].Id);
            Assert.AreEqual("System", list[0].Name);
        }

        /// <summary>
        /// This method testing the category add function
        /// </summary>
        /// <remarks>
        /// Test is siccessful if:
        /// <list type="bullet">
        /// <item>The respond Message has OK MessageType</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task Add_Category_OK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            Message response = await ticket.AddCategoryAsync("UnitTest1", await ticket.GetSystemAsync("atihome"));
            await ticket.DeleteCategoryAsync("UnitTest1", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.OK, response.MessageType, "Final: UnitTest1 could not be added");
        }

        /// <summary>
        /// This methog testing the category add function for already existing category
        /// </summary>
        /// <remarks>
        /// This function adds 'UnitTest2' category twice. The second one reponse's MessageType should be NOK.
        /// </remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task Add_Category_NOK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            Message response = await ticket.AddCategoryAsync("UnitTest2", await ticket.GetSystemAsync("atihome"));
            response = await ticket.AddCategoryAsync("UnitTest2", await ticket.GetSystemAsync("atihome"));
            await ticket.DeleteCategoryAsync("UnitTest2", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.NOK, response.MessageType, "Final: Category duplication check failed");
        }

        /// <summary>
        /// Method for category name changes
        /// </summary>
        /// <remarks>
        /// The method does the following:
        /// <list type="bullet">
        /// <item>Create UnitTest3 Category, assert if NOK</item>
        /// <item>Change this category to UnitTest3_Changed</item>
        /// <item>Delete UnitTest3_Changed</item>
        /// <item>Test failed if the change repsond's MessageType is not OK</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Category_OK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            Message response1 = await ticket.AddCategoryAsync("UnitTest3", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.OK, response1.MessageType, "Preparation: UnitTest3 could not be added");

            Message response2 = await ticket.RenameCategoryAsync("UnitTest3", "UnitTest3_Changed", await ticket.GetSystemAsync("atihome"));
            await ticket.DeleteCategoryAsync("UnitTest3_Changed", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.OK, response2.MessageType, "Final: Category change is done");
        }

        /// <summary>
        /// Category change is failed due to new category already exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Category_NOK_1()
        {
            TicketHandler ticket = new TicketHandler(connString);
            Message response1 = await ticket.AddCategoryAsync("UnitTest4", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.OK, response1.MessageType, "Preparation: UnitTest4 could not be added");

            Message response2 = await ticket.RenameCategoryAsync("UnitTest4", "System", await ticket.GetSystemAsync("atihome"));
            await ticket.DeleteCategoryAsync("UnitTest4", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.NOK, response2.MessageType, "Final: Category was renamed to an already exist category");
        }

        /// <summary>
        /// Category change is failed due to current category does not exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Category_NOK_2()
        {
            TicketHandler ticket = new TicketHandler(connString);
            Message response2 = await ticket.RenameCategoryAsync("UnitTest5", "Something", await ticket.GetSystemAsync("atihome"));

            Assert.AreEqual(MessageType.NOK, response2.MessageType, "Final: Non-exist categry have been renamed");
        }

        /// <summary>
        /// Delete category based on name should work
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delete_Category1_OK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response1 = await ticket.AddCategoryAsync("UnitTest6", await ticket.GetSystemAsync("atihome"));
            Assert.AreEqual(MessageType.OK, response1.MessageType, "Preparation failed: UnitTest 6 could not be added");

            var response2 = await ticket.DeleteCategoryAsync("UnitTest6", await ticket.GetSystemAsync("atihome"));
            Assert.AreEqual(MessageType.OK, response2.MessageType, "Final: Category deletion is OK");
        }

        /// <summary>
        /// Delete category failed due to category did not exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delete_Category1_NOK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response = await ticket.DeleteCategoryAsync("UnitTest7", await ticket.GetSystemAsync("atihome"));
            Assert.AreEqual(MessageType.NOK, response.MessageType, "Final: Category could be deleted which was non-existed");
        }

        /// <summary>
        /// Delete category based on ID should work
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delete_Category2_OK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response1 = await ticket.AddCategoryAsync("UnitTest8", await ticket.GetSystemAsync("atihome"));
            Assert.AreEqual(MessageType.OK, response1.MessageType, "Preparation: UnitTest8 could not be added");

            var response2 = await ticket.ListCategoriesAsync();
            int id = 0;
            foreach (var item in response2)
            {
                if (item.Name == "UnitTest8")
                    id = item.Id;
            }
            Assert.AreNotEqual(0, id, "Preparation: Earlier added category could not find");

            var response3 = await ticket.DeleteCategoryAsync(id);
            Assert.AreEqual(MessageType.OK, response3.MessageType, "Final: Category deletion by ID was unsuccessful");
        }

        /// <summary>
        /// Delete category should failed due to ID did not exist
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delete_Category2_NOK()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response = await ticket.DeleteCategoryAsync(-1);
            Assert.AreEqual(MessageType.NOK, response.MessageType, "Final: Category could be deleted by non-existing ID");
        }
    }
}
