using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using vb = Microsoft.VisualBasic;

namespace TEO.General.Messaging
{
    public class BusMsgBox : ABus, IDisposable
    {
        public string Header;
        public BusMsgBox(Provider sender) : base(sender) { }

        protected override void send_sync(object sender, EventArgsMessage e)
        {
            var typ = e.Type;
            var txt = e.Text;
            vb.MsgBoxStyle stl = vb.MsgBoxStyle.Information;

            if (string.IsNullOrEmpty(txt)) return;
            else if ((typ & TMessage.MaskMessageBox) == 0) return;
            else if ((typ & TMessage.MaskException) > 0)
                stl = vb.MsgBoxStyle.Critical;

            vb.Interaction.MsgBox(txt, stl, Header);
        }
    }
}
