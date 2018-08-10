using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public class Command1 : ICommand, ICommandTunnelable
    {
        public int CUR = 1;

        public string Name, Text;
        public ICommand Next { get; set; }
        public TCommand Type => TCommand.Sequential;

        public ExecuteResult Execute(Context context)
        {
            throw new ExceptionRuntime(this.Text, false);
        }

        public override string ToString()
        {
            return string.Format("Command1: {0}", this.Name, CUR);
        }
    }
}
