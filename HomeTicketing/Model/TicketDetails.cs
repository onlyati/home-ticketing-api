using HomeTicketing.Model;

namespace HomeTicketing.Model
{
    /// <summary>
    /// Model to create JSON when details request is got
    /// </summary>
    public class TicketDetails
    {
        /// <summary>
        /// Contains the base summary and the meta data
        /// </summary>
        public TicketHeader Header { get; set; }
        /// <summary>
        /// List about the logs which belongs to ticket
        /// </summary>
        public Log[] Logs { get; set; }
    }
}
