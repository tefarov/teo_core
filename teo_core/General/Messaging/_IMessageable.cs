using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General.Messaging;

namespace TEO.General.Messaging
{
    public interface IMessageable
    {
        event EventHandler<EventArgsMessage> Message;
    }
}
