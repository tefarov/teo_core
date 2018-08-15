using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;
using TEO.Commanding;
using TEO.Commanding.IO;

namespace APP.Commanding
{
    class Parser : TEO.Commanding.Parser_2
    {
        public Parser(TEO.Commanding.Environment.Environment parent) : base ( parent)
        {
            // Display provider
            var dsp = Program.Core.Display;
            // file-factory
            var ffil = Program.Core.FFiles;
            // a setter that displays lines of text on a display as info
            var sinf = dsp.GetSetter(TEO.General.Messaging.TMessage.Info, true);

            DFBAB["msg"] = CmdMessagebox.Factory(dsp);
            DFBAB["exit"] = get_singleton(() => Program.Continue = false);

            DFBAB["display.file"] = CmdReadText.Factory(sinf, ffil);
        }
    }
}
