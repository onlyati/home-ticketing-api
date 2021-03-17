using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController.Controller;
using DatabaseController.Interface;
using DatabaseController.Model;
using DatabaseController.DataModel;
using System.Threading.Tasks;
using System.Linq;

namespace DatabaseControllerTest
{
    [TestClass]
    public class UserTest
    {
        string connString = "Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test";

        #region Register test
        /// <summary>
        /// Test invalid parameter #1
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Register_User_Missing1()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response = await ticket.RegisterUserAsync(null);
            Assert.AreEqual(MessageType.NOK, response.MessageType);
        }

        /// <summary>
        /// Test invalid parameter #2
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Register_User_Missing2()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var response = await ticket.RegisterUserAsync(new User());
            Assert.AreEqual(MessageType.NOK, response.MessageType);
        }

        /// <summary>
        /// Test basic functions of user: register, duplicated error, get by username and id, delete by id
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Register_Delete_User()
        {
            // Register a new user id
            TicketHandler ticket = new TicketHandler(connString);
            User user = new User();
            user.Username = "unit-test-1";
            user.Email = "unit-test-1@atihome.local";
            user.Password = "unit";
            var response = await ticket.RegisterUserAsync(user);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User creation is failed");

            // Register once more, should cause failure
            response = await ticket.RegisterUserAsync(user);
            Assert.AreEqual(MessageType.NOK, response.MessageType, "Duplicated used was added");

            // Get user based on its username
            var user1 = await ticket.GetUserAsync(user.Username);
            Assert.IsNotNull(user1, "Registered user did not found");

            // Get user based on its id
            var user2 = await ticket.GetUserAsync(user1.Id);
            Assert.AreEqual(user1, user2, "Difference getuser detected");

            // Delete user, cleanup for the next run
            response = await ticket.RemoveUserAsync(user1.Id);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User could not deleted");
        }

        /// <summary>
        /// Check that hash give the same result
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void Hash_Password()
        {
            var hash1 = TicketHandler.HashPassword("abcd");
            var hash2 = TicketHandler.HashPassword("abcd");
            Assert.AreEqual(hash1, hash2, "Hash are not same");
            Assert.AreEqual(128, hash2.Length, "Hahs is not 128 bytes long");
        }

        /// <summary>
        /// Check get password
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Register_Password_User()
        {
            // Register new user ID
            TicketHandler ticket = new TicketHandler(connString);
            User user = new User();
            user.Username = "unit-test-2";
            user.Email = "unit-test-2@atihome.local";
            user.Password = "unit";
            var response = await ticket.RegisterUserAsync(user);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User creation is failed");

            // Get user
            var user1 = await ticket.GetUserAsync("unit-test-2");
            Assert.IsNotNull(user1, "Registered user did not found");

            // Get hashed password and compare
            var hashed_pw = TicketHandler.HashPassword("unit");
            Assert.AreEqual(user1.Password, hashed_pw, "Hashed passwords are not same");
            Assert.AreEqual(128, user1.Password.Length, "Hash is not 128 bytes long");

            // Delete user, cleanup for the next run
            response = await ticket.RemoveUserAsync(user1.Id);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User could not deleted");
        }
        #endregion

        #region Change test
        /// <summary>
        /// This test creates a user, change password and compare the old and new and calculated passwords
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Password()
        {
            // Register new user ID for test
            TicketHandler ticket = new TicketHandler(connString);
            User user = new User();
            user.Username = "unit-test-3";
            user.Email = "unit-test-3@atihome.local";
            user.Password = "unit";
            var response = await ticket.RegisterUserAsync(user);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User creation is failed");

            // Get user
            var user1 = await ticket.GetUserAsync(user.Username);
            Assert.IsNotNull(user1, "User could not get");
            string oldPw = user1.Password;

            // Be assumed that good password is set
            Assert.AreEqual(TicketHandler.HashPassword("unit"), user1.Password, "Wrong password is set originally");

            // Change password
            User chgPw = new User();
            chgPw.Password = "sajt";
            var chg = await ticket.ChangeUserAsync(user1.Id, chgPw);
            Assert.AreEqual(MessageType.OK, chg.MessageType, "Password change has failed");

            var user2 = await ticket.GetUserAsync(user.Username);
            Assert.AreEqual(TicketHandler.HashPassword("sajt"), user2.Password, "Password is not match with calculated one");
            Assert.AreNotEqual(oldPw, user2.Password, "Password is not changed");

            // Delete user, cleanup for the next run
            response = await ticket.RemoveUserAsync(user.Username);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User could not deleted");
        }

        /// <summary>
        /// Test email change method
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Email()
        {
            // Register new user ID for test
            TicketHandler ticket = new TicketHandler(connString);
            User user = new User();
            user.Username = "unit-test-4";
            user.Email = "unit-test-4@atihome.local";
            user.Password = "unit";
            var response = await ticket.RegisterUserAsync(user);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User creation is failed");

            // Get user
            var user1 = await ticket.GetUserAsync(user.Username);
            Assert.IsNotNull(user1, "User could not get");

            string oldEmail = user.Email;

            // Change email
            User chgEmail = new User();
            chgEmail.Email = "unit-test-4-changed@atihome.local";
            var chg = await ticket.ChangeUserAsync(user1.Username, chgEmail);
            Assert.AreEqual(MessageType.OK, chg.MessageType, "Email change has failed");

            var user2 = await ticket.GetUserAsync(user.Username);
            Assert.AreEqual("unit-test-4-changed@atihome.local", user2.Email, "Email is not match with the new one");
            Assert.AreNotEqual(oldEmail, user2.Email, "Email is not changed");

            // Delete user, cleanup for the next run
            response = await ticket.RemoveUserAsync(user.Username);
            Assert.AreEqual(MessageType.OK, response.MessageType, "User could not deleted");
        }

        /// <summary>
        /// Check proper NOK messages
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Fail1()
        {
            TicketHandler ticket = new TicketHandler(connString);

            var chg = await ticket.ChangeUserAsync(null, null);
            Assert.AreEqual(MessageType.NOK, chg.MessageType);
        }

        /// <summary>
        /// Check proper NOK messages
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Change_Fail2()
        {
            TicketHandler ticket = new TicketHandler(connString);

            var chg = await ticket.ChangeUserAsync(-88, null);
            Assert.AreEqual(MessageType.NOK, chg.MessageType);
        }
        #endregion

        #region List test
        /// <summary>
        /// List all users
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ListAll()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var list = await ticket.GetUsersAsync();
            Assert.IsNotNull(list);
            Assert.AreNotEqual(0, list.Count);
            Assert.IsNotNull(list.SingleOrDefault(s => s.Username.Equals("Dispatcher")));
        }

        /// <summary>
        /// List all users whihc belongs to a test system
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ListAll_By_System()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var list = await ticket.GetUsersAsync(await ticket.GetSystemAsync("test-system-1"));
            Assert.IsNotNull(list);
            Assert.AreNotEqual(0, list.Count);
            Assert.IsNotNull(list.SingleOrDefault(s => s.Username.Equals("test-user-1")));
            Assert.IsNotNull(list.SingleOrDefault(s => s.Username.Equals("test-user-2")));
        }

        /// <summary>
        /// List all users which belongs to a test category on test system
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ListAll_By_Category()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var list = await ticket.GetUsersAsync(await ticket.GetCategoryAsync("System", await ticket.GetSystemAsync("test-system-1")));
            Assert.IsNotNull(list);
            Assert.AreNotEqual(0, list.Count);
            Assert.IsNotNull(list.SingleOrDefault(s => s.Username.Equals("test-user-1")));
            Assert.IsNotNull(list.SingleOrDefault(s => s.Username.Equals("test-user-2")));
        }
        #endregion

        #region User utilites test
        /// <summary>
        /// Assign to to a test category, then unassing it
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Assign_Category()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var user = await ticket.GetUserAsync("test-assign");
            Assert.IsNotNull(user, "User (test-assign) was not located");

            var category = await ticket.GetCategoryAsync("System", await ticket.GetSystemAsync("test-system-1"));
            Assert.IsNotNull(category, "Category could not located");

            var response1 = await ticket.AssignUserToCategory(category, user);
            Assert.AreEqual(MessageType.OK, response1.MessageType, "User has not assigned to category");

            var response2 = await ticket.UnassignUserToCategory(category, user);
            Assert.AreEqual(MessageType.OK, response2.MessageType, "User has not unassigned from category");
        }

        /// <summary>
        /// This test assign a user to a ticket
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Assing_Ticket()
        {
            TicketHandler ticket = new TicketHandler(connString);
            var user = await ticket.GetUserAsync("test-user-1");
            var disp = await ticket.GetUserAsync("Dispatcher");

            TicketFilterTemplate filter = new TicketFilterTemplate();
            filter.System = "test-system-1";
            filter.Reference = "test-ref-1";
            filter.Status = "Open";
            var ticketList = await ticket.ListTicketsAsync(filter);

            var response1 = await ticket.AssignUserToTicketAsync(user, ticketList[0]);
            Assert.IsNotNull(response1);
            Assert.AreEqual(MessageType.OK, response1.MessageType);

            var response2 = await ticket.AssignUserToTicketAsync(disp, ticketList[0]);
            Assert.IsNotNull(response2);
            Assert.AreEqual(MessageType.OK, response2.MessageType);
        }
        #endregion
    }
}
