using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    /// <summary>
    /// Represents a command that may have some continuation after it's execution is being finished
    /// </summary>
    public interface ICommandTunnelable
    {
        ICommand Next { get; }
    }
}
