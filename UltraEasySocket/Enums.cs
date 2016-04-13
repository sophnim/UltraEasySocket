using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraEasySocket
{
    public enum ReturnCode
    {
        SUCCESS,
        FAIL_ALREADY_CLOSED,
        ERROR_INVALID_SESSIONID,
        ERROR_SESSION_CLOSED
    }

    public enum CallbackEventType
    {
        ACCEPT_SUCCESS,
        ACCEPT_FAIL,
        CONNECT_SUCCESS,
        CONNECT_FAIL,
        SESSION_RECEIVE_DATA,
        SESSION_CLOSED,
    }
    
}
