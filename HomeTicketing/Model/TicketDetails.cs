using HomeTicketing.Model;

namespace HomeTicketing.Model
{
    /* Model to create JSON when details request is got */
    public class TicketDetails
    {
        public TicketHeader Header { get; set; }
        public Log[] Logs { get; set; }
    }
}
