using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseController.Model
{
    /// <summary>
    /// Class for messages to send back for requests
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Type of message. Can be: OK, NOK
        /// </summary>
        /// <example>OK, NOK</example>
        public string MessageText { get; set; }

        /// <summary>
        /// Text of message
        /// </summary>
        public MessageType MessageType { get; set; }
    }

    public enum MessageType
    {
        OK,
        NOK
    }
}
