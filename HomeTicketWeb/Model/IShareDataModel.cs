using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Model
{
    /*********************************************************************************************/
    /* Interface for general 'data saving' objects which are used by DI                          */
    /*********************************************************************************************/
    public interface IShareDataModel
    {
        // Set every component (which can be) to null or default
        public void SetNull();
    }
}
