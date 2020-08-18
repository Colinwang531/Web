using ShipWeb.ProtoBuffer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Interface
{
    public interface IDealerService
    {
        public void SendMessage(MSG msg,string identity);
        public MSG ReviceMessage();
    }
}
