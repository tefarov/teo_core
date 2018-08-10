using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public abstract class ACommand : ICommand, ICommandTunnelable, IGui
    {
        public TCommand Type { get; set; }
        public string Text { get; set; }

        protected ACommand()
        {
            this.Type =  TCommand.Sequential;
        }

        public abstract ExecuteResult Execute(Context context);

        #region ICommandTunnelable 
        public ICommand Next { get; set; }
        #endregion
    }
}
