using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using TEO.General.Messaging;
using TEO.General.Configurating;

namespace TEO.Commanding
{
    public class CmdMessagebox : ACommand
    {
        const string
            // configuration section names
            STR_SECMAIN = "main"

            // configuration keynames
            , STR_KEYTEXT = "text"
            ;

        Provider DSP;

        public string Message;
        TMessage TYP = TMessage.MessageBox;

        public CmdMessagebox(Provider display) { DSP = display.NotNull(); }

        public override ExecuteResult Execute(Context context)
        {
            DSP.Write(this.Message, TYP);
            return new ExecuteResult(true);
        }

        public static IFactory<Input, IBatchable> Factory(Provider display) { return new factory(display); }
        class factory : IFactory<Input, IBatchable>
        {
            Provider DSP;

            public factory(Provider display) { DSP = display.NotNull(); }
            public IBatchable Create(Input command)
            {
                var itm = new CmdMessagebox(DSP);

                if (!command.TrySet(ref itm.Message, 1, "text"))
                    return null;

                bool err = false;
                if (command.TrySet(ref err, 2, "error") && err)
                    itm.TYP = TMessage.ExceptionBox;

                return itm;
            }
        }
    }
}
