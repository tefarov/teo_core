using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Messaging
{
    public class BusConsole : ABus
    {
        const string STR_FMTCONS = "{0:HH:mm:ss} {1,3}: {2}";

        /// <summary>
        /// Is new line
        /// </summary>
        bool INL = true; 
        int STP = 0;
        string STR_PROGRSS = null;

        public BusConsole(Provider sender) : base(sender) { }
        protected override void send_sync(object sender, EventArgsMessage e)
        {
            TMessage typ = e.Type;
            string txt = e.Text;
            if (txt == null) txt = string.Empty;

            // this processes some system-messages
#if !DEBUG
            if (typ == TMessage.QueryData) return ;
#endif
            if (typ == TMessage.Progress) {
                if (STR_PROGRSS != txt) {
                    if (!string.IsNullOrEmpty(STR_PROGRSS)) Console.WriteLine();
                    Console.Write(STR_FMTCONS + "  ", DateTime.Now, STP, STR_PROGRSS = txt);
                }
                Console.Write('.');
                return;
            }
            else if (STR_PROGRSS != null) {
                STR_PROGRSS = null;
                Console.WriteLine();
            }
            else if (!INL && typ != TMessage.InlineText)
                Console.WriteLine();

            if (string.IsNullOrEmpty(txt)) return;
            else if (typ == TMessage.None) return;
            else if ((typ & TMessage.MaskMessageBox) > 0) return;
            else if (typ == TMessage.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if ((typ & TMessage.MaskException) > 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (typ == TMessage.CommandResult)
                Console.ForegroundColor = ConsoleColor.White;
            else if (typ == TMessage.UserInput)
                Console.ForegroundColor = ConsoleColor.White;
            else if (typ == TMessage.QueryData)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (typ == TMessage.CommandHeader) {
                Console.WriteLine("  --------------  ");
                STP++;
            }



            if (typ == TMessage.UserInput) {
                Console.Write(txt);
                Console.Write(" >> ");
            }
            else if (typ == TMessage.InlineText) {
                Console.Write(txt);
                INL = false;
            }
            else if (typ == TMessage.CommandReport) {
                Console.WriteLine(STR_FMTCONS, DateTime.Now, STP++, "Отчёт сформирован");
            }
            else
                Console.WriteLine(STR_FMTCONS, DateTime.Now, STP, txt);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
