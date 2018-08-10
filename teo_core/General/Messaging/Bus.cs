using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Messaging
{
    public abstract class ABus : IDisposable
    {
        Provider SND;
        public readonly bool IsAsync = false;

        public ABus(Provider sender, bool isasync = false)
        {
            SND = sender.NotNull();
            this.IsAsync = isasync;

            this.Activate();
        }

        public virtual void Dispose()
        {
            this.Deactivate();
        }

        public virtual void Activate()
        {
            if (this.IsAsync)
                SND.Occur += this.send_async;
            else
                SND.Occur += this.send_sync;
        }
        public virtual void Deactivate()
        {
            SND.Occur -= this.send_sync;
            SND.Occur -= this.send_async;
        }

        protected virtual void send_sync(object sender, EventArgsMessage e) { throw new NotImplementedException("Sychronous messaging is not supported"); }
        protected virtual void send_async(object sender, EventArgsMessage e) { throw new NotImplementedException("Asychronous messaging is not supported"); }
    }
}
