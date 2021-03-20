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
    public class SystemTest
    {
        string connString = "Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test";

        /// <summary>
        /// Test system methods with good result
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SystemTest_Add_Del()
        {
            DbHandler ticket = new DbHandler(connString);

            string newSystem = "Unit-Test-1";

            Message respond1 = await ticket.AddSystemAsync(newSystem);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "New system could not be added");

            DatabaseController.DataModel.System respond2 = await ticket.GetSystemAsync(newSystem);
            Assert.AreEqual(newSystem, respond2.Name, "The created group name is different");
            Assert.IsNotNull(respond2.Id, "ID column is NULL");
            Assert.AreNotEqual(0, respond2.Id, "ID is not set for record");

            List<DatabaseController.DataModel.System> respond3 = await ticket.GetSystemsAsync();
            Assert.AreNotEqual(0, respond3.Count, "No system has been listed");

            Message respond4 = await ticket.RemoveSystemAsync(newSystem);
            Assert.AreEqual(MessageType.OK, respond4.MessageType, "New system could not be deleted");
        }

        /// <summary>
        /// Check listing with not found result
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SystemTest_Null()
        {
            DbHandler ticket = new DbHandler(connString);

            var respond = await ticket.GetSystemAsync(null as string);
            Assert.AreEqual(null, respond);
        }

        /// <summary>
        /// Check listing with not found result
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SystemTest_Null2()
        {
            DbHandler ticket = new DbHandler(connString);

            var respond = await ticket.GetSystemAsync("sdfsdfds");
            Assert.AreEqual(null, respond);
        }

        [TestMethod]
        public async Task SystemRename_Test()
        {
            DbHandler ticket = new DbHandler(connString);
            var respond1 = await ticket.AddSystemAsync("test-rename-1");
            Assert.AreEqual(MessageType.OK, respond1.MessageType);

            var sys1 = await ticket.GetSystemAsync("test-rename-1");

            var respond2 = await ticket.RenameSystemAsync("test-rename-1", "test-rename-2");
            Assert.AreEqual(MessageType.OK, respond2.MessageType);

            var sys2 = await ticket.GetSystemAsync("test-rename-2");

            Assert.AreEqual(sys1.Id, sys2.Id);

            await ticket.RemoveSystemAsync("test-rename-2");

        }
    }
}
