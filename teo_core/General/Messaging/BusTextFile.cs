using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TEO.General.Messaging
{
    public    class BusTextFile : ABus, IDisposable
    {
        const string 
            STR_FMTMESG = "{0:HH:mm:ss} {1,3}: {3}{2}"
            , STR_PFXWARN = "WARN"
            , STR_PFXEXC0 = "EXCP"
            , STR_PFXEXC1 = "CRITICAL"
            ;

        public readonly string Filepath;

        int STP = 0;
        
        StreamWriter WRT;

        public BusTextFile(Provider sender, string filepath) : base(sender)
        {
            this.Filepath = filepath.NotNull();
            WRT = new StreamWriter(filepath, false, Encoding.UTF8);
        }

        public override void Dispose()
        {
            base.Dispose();
            WRT.Dispose();
        }

        protected override void send_sync(object sender, EventArgsMessage e)
        {
            if (WRT == null) return;

            TMessage typ = e.Type;
            string txt = e.Text, wrn = null;
            if (txt == null) txt = string.Empty;

            if (typ == TMessage.CommandHeader) {
                STP++;
                WRT.WriteLine("  ----------------  ");
            }

            if (string.IsNullOrEmpty(txt)) return;
            else if (typ == TMessage.None) return;
            else if ((typ == TMessage.InlineText)) return;
            else if ((typ == TMessage.UserInput)) return;
#if !DEBUG
            else if (typ == TMessage.QueryData) return;
#endif
            else if ((typ & TMessage.MaskMessageBox) > 0) return;
            else if (typ == TMessage.Warning)
                wrn = STR_PFXWARN + ' ';
            else if (typ == TMessage.ExceptionWorkflow)
                wrn = STR_PFXEXC0 + ' ';
            else if (typ == TMessage.ExceptionCritical)
                wrn = STR_PFXEXC1 + ' ';

            WRT.WriteLine(STR_FMTMESG, DateTime.Now, STP, txt, wrn);
            WRT.Flush();
        }
    }
}
