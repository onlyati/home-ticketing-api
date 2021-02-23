using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseController.Controller;
using DatabaseController.Interface;
using DatabaseController.Model;

namespace DatabaseControllerTest
{
    [TestClass]
    public class ConstructorTest
    {
        /// <summary>
        /// This method test, that the constructor gives back an exception if input string is empty
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.NullReferenceException), "Connection string is missing in the constructor")]
        public void Null_Constructor()
        {
            TicketHandler ticket = new TicketHandler("");
            Assert.ThrowsException<System.NullReferenceException>(() => ticket);
        }

        /// <summary>
        /// This method test, when an invalid input string is added. The EF should throw and exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Invalid_Constructor()
        {
            TicketHandler ticket = new TicketHandler("kjsdfklfjsdfklé");
            Assert.ThrowsException<System.ArgumentException>(() => ticket);
        }

        /// <summary>
        /// This is a normal constructor with a valid connections string. Should work.
        /// </summary>
        [TestMethod]
        public void Normal_Constructor()
        {
            TicketHandler ticket = new TicketHandler("Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test");
            Message test = ticket.HealthCheck();

            Message exptected = new Message();
            exptected.MessageType = MessageType.OK;
            exptected.MessageText = "API is alive";

            Assert.AreEqual(test.MessageText, exptected.MessageText);
            Assert.AreEqual(test.MessageType, exptected.MessageType);
        }

        /// <summary>
        /// This method try to read the connection string which was passed via constructor
        /// </summary>
        [TestMethod]
        public void Get_Connection_String()
        {
            TicketHandler ticket = new TicketHandler("Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test");

            Assert.AreEqual("Host=atihome.local;Database=TestTicketing;Username=ticket_test_api;Password=test", ticket.GetConnectionString());
        }
    }
}
