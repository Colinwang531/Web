using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    /// <summary>
    /// 用户类
    /// </summary>
    [ProtoContract]
    public class User
    {
        public enum Command
        {
            NEW_REQ = 1,
            NEW_REP = 2,
            DELETE_REQ = 3,
            DELETE_REP = 4,
            MODIFY_REQ = 5,
            MODIFY_REP = 6,
            QUERY_REQ = 7,
            QUERY_REP = 8,
            LOGIN_REQ = 9,
            LOGIN_REP = 10
        }
        public Command command { get; set; }
        public UserRequest userrequest { get; set; }
        public UserResponse userresponse { get; set; }
    }
}
