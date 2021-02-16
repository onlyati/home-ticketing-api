using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController.Controller;
using DatabaseController.Interface;
using DatabaseController.Model;
using System.Threading.Tasks;

namespace DatabaseControllerTest
{
    [TestClass]
    public class TicketTest
    {
        string connString = "server=atihome.local;user=ati;password=abcd;database=test_entity_exist2;";

        #region Ticket list tests
        [TestMethod]
        public async Task List_Ticket_1()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var respond = await ticket.ListTicketsAsync();

            Assert.AreNotEqual(0, respond.Count, "Final: 0 listed ticket");
        }

        [TestMethod]
        public async Task List_Ticket_2()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var respond = await ticket.ListTicketsAsync(2, 5);

            Assert.AreEqual(5, respond.Count, "Final: Not 2 ticket has returned");
            Assert.AreEqual(3, respond[0].Id, "Final: Not the 3. element is the first one");
        }

        [TestMethod]
        public async Task List_Ticket_3()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Testing without any filter
            var respond11 = await ticket.ListTicketsAsync();
            var respond12 = await ticket.ListTicketsAsync(new TicketFilterTemplate());

            Assert.AreEqual(respond11.Count, respond12.Count, "Final #1: List all ticket and list with empty filter are not same");
        }

        [TestMethod]
        public async Task List_Ticket_4()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Filter with only one thing
            TicketFilterTemplate filter1 = new TicketFilterTemplate();
            filter1.Category = "Hardware";
            var respond1 = await ticket.ListTicketsAsync(filter1);
            Assert.AreNotEqual(0, respond1.Count, "Final #1: Filter with category is failed");

            TicketFilterTemplate filter2 = new TicketFilterTemplate();
            filter2.Status = "Open";
            var respond2 = await ticket.ListTicketsAsync(filter2);
            Assert.AreNotEqual(0, respond2.Count, "Final #2: Filter with status is failed");

            TicketFilterTemplate filter3 = new TicketFilterTemplate();
            filter3.Refernce = "test";
            filter3.Status = "Open";
            var respond3 = await ticket.ListTicketsAsync(filter3);
            Assert.AreNotEqual(0, respond2.Count, "Final #3: Filter with multiple filters is failed");
        }

        [TestMethod]
        public async Task List_Ticket_5()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Filter and expect null return
            TicketFilterTemplate filter1 = new TicketFilterTemplate();
            filter1.Category = "Sajt";
            var respond1 = await ticket.ListTicketsAsync(filter1);
            Assert.AreEqual(0, respond1.Count, "Final #1: This should have 0 element");

            TicketFilterTemplate filter2 = new TicketFilterTemplate();
            filter2.Refernce = "non-exist";
            var respond2 = await ticket.ListTicketsAsync(filter2);
            Assert.AreEqual(0, respond2.Count, "Final #2: This should have 0 element");
        }

        [TestMethod]
        public async Task List_Ticket_6()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Filter and count lines
            TicketFilterTemplate filter = new TicketFilterTemplate();
            filter.Refernce = "test";
            var respond = await ticket.ListTicketsAsync(2, 5, filter);

            Assert.AreEqual(5, respond.Count, "Final: Not 5 element has returned");
            Assert.AreEqual(9, respond[0].Id, "Not the 3. element return as first");
        }
        #endregion

        #region Create&Close ticket
        [TestMethod]
        public async Task Create_Close_Ticket_1()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Open ticket
            TicketCreationTemplate crt = new TicketCreationTemplate();
            crt.Category = "Teszt";
            crt.Details = "Here is the details";
            crt.Reference = "unit-test-1";
            crt.Summary = "Test of ticket";
            crt.Title = "Test ticket";

            var respond1 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Final: Ticket could not be created");

            // Close by reference value
            var respond2 = await ticket.CloseTicketAsync(crt.Reference);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Final: Ticket could not be closed based on reference value");
        }

        [TestMethod]
        public async Task Create_Close_Ticket_2()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Open ticket
            TicketCreationTemplate crt = new TicketCreationTemplate();
            crt.Category = "Teszt";
            crt.Details = "Here is the details";
            crt.Reference = "unit-test-2";
            crt.Summary = "Test of ticket";
            crt.Title = "Test ticket";

            var respond1 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Final: Ticket could not be created");

            // Looking for its ID
            TicketFilterTemplate filter = new TicketFilterTemplate();
            filter.Status = "Open";
            filter.Refernce = "unit-test-2";

            var respond2 = await ticket.ListTicketsAsync(filter);
            Assert.AreEqual(1, respond2.Count, "Preparation: Not one ticket was found for the filter");

            // Close ticket by ID
            var respond3 = await ticket.CloseTicketAsync(respond2[0].Id);
            Assert.AreEqual(MessageType.OK, respond3.MessageType, "Final: Ticket could not be closed based on ID");
        }
        #endregion

        #region Ticket details test
        [TestMethod]
        public async Task Details_Test_1()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Open ticket and send 2 further update
            TicketCreationTemplate crt = new TicketCreationTemplate();
            crt.Category = "Teszt";
            crt.Details = "Here is the details";
            crt.Reference = "unit-test-3";
            crt.Summary = "Test of ticket";
            crt.Title = "Test ticket";

            var respond1 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Final: Ticket could not be created");

            var respond2 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond2.MessageType, "Final: Ticket could not be created");

            var respond3 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond2.MessageType, "Final: Ticket could not be created");

            // Looking for its ID
            TicketFilterTemplate filter = new TicketFilterTemplate();
            filter.Status = "Open";
            filter.Refernce = "unit-test-3";

            var respond4 = await ticket.ListTicketsAsync(filter);
            Assert.AreEqual(1, respond4.Count, "Preparation: Not one ticket was found for the filter");

            // Get details about it
            var respond5 = await ticket.GetDetailsAsync(respond4[0].Id);

            Assert.IsNotNull(respond5, "Final: Details returned with null");
            Assert.IsNotNull(respond5.Header, "Final: Header is null");
            Assert.IsNotNull(respond5.Logs, "Final: Logs is null");
            Assert.AreEqual(3, respond5.Logs.Count, "Not 3 update in the ticket");

            // Close by refernce value
            var respond6 = await ticket.CloseTicketAsync(crt.Reference);
            Assert.AreEqual(MessageType.OK, respond6.MessageType, "Close was failed");
        }
        #endregion

        #region Change ticket test
        [TestMethod]
        public async Task Change_Ticket_Test_1()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Open ticket and send 2 further update
            TicketCreationTemplate crt = new TicketCreationTemplate();
            crt.Category = "Teszt";
            crt.Details = "Here is the details";
            crt.Reference = "unit-test-change-1";
            crt.Summary = "Test of ticket";
            crt.Title = "Test ticket";

            var respond1 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Preparation: Ticket could not be created");

            // Query for the opened change
            TicketFilterTemplate filter1 = new TicketFilterTemplate();
            filter1.Status = "Open";
            filter1.Refernce = crt.Reference;

            var respond12 = await ticket.ListTicketsAsync(filter1);
            Assert.AreEqual(1, respond12.Count, "Preparation: opened ticket was not found");

            // Change the category to Tesztike
            TicketChangeTemplate chg = new TicketChangeTemplate();
            chg.Id = respond12[0].Id;
            chg.Category = "Tesztike";

            var respond2 = await ticket.ChangeTicketAsync(chg);
            Assert.AreEqual(MessageType.OK, respond2.MessageType, "Final: Ticket change was failed");

            // Query the latest changed ticket
            TicketFilterTemplate filter2 = new TicketFilterTemplate();
            filter2.Status = "Open";
            filter2.Refernce = crt.Reference;
            filter2.Category = "Tesztike";

            var respond3 = await ticket.ListTicketsAsync(filter2);
            Assert.AreEqual(1, respond3.Count, "Final: No ticket was found with the changed category name");

            // Close ticket
            var respond4 = await ticket.CloseTicketAsync(crt.Reference);
            Assert.AreEqual(MessageType.OK, respond4.MessageType, "Post: Close was failed");
        }

        [TestMethod]
        public async Task Change_Ticket_Test_2()
        {
            TicketHandler ticket = new TicketHandler(connString);

            // Open ticket and send 2 further update
            TicketCreationTemplate crt = new TicketCreationTemplate();
            crt.Category = "Teszt";
            crt.Details = "Here is the details";
            crt.Reference = "unit-test-change-2";
            crt.Summary = "Test of ticket";
            crt.Title = "Test ticket";

            var respond1 = await ticket.CreateTicketAsync(crt);
            Assert.AreEqual(MessageType.OK, respond1.MessageType, "Preparation: Ticket could not be created");

            // Query for the opened change
            TicketFilterTemplate filter1 = new TicketFilterTemplate();
            filter1.Status = "Open";
            filter1.Refernce = crt.Reference;

            var respond12 = await ticket.ListTicketsAsync(filter1);
            Assert.AreEqual(1, respond12.Count, "Preparation: opened ticket was not found");

            // Try to change for a non-existing category
            TicketChangeTemplate chg = new TicketChangeTemplate();
            chg.Id = respond12[0].Id;
            chg.Category = "non-exist";

            var respond2 = await ticket.ChangeTicketAsync(chg);
            Assert.AreEqual(MessageType.NOK, respond2.MessageType, "Final: Ticket was changed to the a non-exist category");

            // Close the ticket
            await ticket.CloseTicketAsync(crt.Reference);
        }
        #endregion
    }
}
