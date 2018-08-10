using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public class CmdTest : ACommand
    {
        TEO.General.Messaging.Provider DSP;
        public CmdTest(General.Messaging.Provider display) { DSP = display.NotNull(); }
        public override ExecuteResult Execute(Context context)
        {
            var tsx = new Task(this.exe_x);
            var tsy = new Task(this.exe_y);

            tsx.Start();
            tsy.Start();

            System.Threading.Thread.Sleep(1000);
            ISC = false;

            return new ExecuteResult(true);
        }

        bool ISC = true;

        void exe_x()
        {
            while (ISC) {
                DSP.Write(null, new General.Messaging.EventArgsMessage("_", General.Messaging.TMessage.InlineText));
                System.Threading.Thread.Sleep(3);
            }
            DSP.Write(null, new General.Messaging.EventArgsMessage("X done"));
        }
        void exe_y()
        {
            while (ISC) {
                DSP.Write(null, new General.Messaging.EventArgsMessage(".", General.Messaging.TMessage.InlineText));
                System.Threading.Thread.Sleep(4);
            }
            DSP.Write(null, new General.Messaging.EventArgsMessage("y done"));
        }
    }
}
