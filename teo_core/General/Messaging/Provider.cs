using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Messaging
{
    public class Provider
    {
        public void Write(object sender, EventArgsMessage e)
        {
            this.Occur(sender, e);
        }
        public void Write(string text, TMessage type, string position = null)
        {
            this.Occur(this, new EventArgsMessage { Text = text, Type = type, Position = position });
        }
        public void Write(Exception ex, TMessage type = TMessage.ExceptionWorkflow, string position = null)
        {
            if (this.Occur == null) { }
            else if ((type & TMessage.MaskException) == 0) throw new ArgumentException("type should mask with TMessage.MaskException");
            else {
                string txt = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);

                this.Occur(this, new EventArgsMessage { Text = txt, Type = type, Position = position });
            }
        }

        public event EventHandler<EventArgsMessage> Occur;

        /// <summary>
        /// This function returns a setter, that will display something (in this provider) each time it's being set.
        /// </summary>
        /// <param name="type">Type of messages to use when displaying data</param>
        /// <param name="splittolines">if true will split the contents to lines and display each line separetly</param>
        public ISetter<string> GetSetter(TMessage type, bool splittolines = false) { return new set_display(this, type, splittolines); }

        class set_display : ISetter<string>
        {
            readonly Provider DSP;
            readonly TMessage TYP;
            readonly bool SPT;

            public set_display(Provider display, TMessage type, bool splittolines = false)
            {
                this.DSP = display.NotNull();
                this.TYP = type;
                this.SPT = splittolines;
            }
            public void Set(string item)
            {
                if (!SPT) {
                    DSP.Write(item, this.TYP);
                    return;
                }

                var astr = item.Split(__ASEP, StringSplitOptions.None);
                for (int i = 0; i < astr.Length; i++)
                    DSP.Write(astr[i], TYP);
            }

            /// <summary>
            /// Array of line-separators. This array is needed for string.Split - function
            /// when splitting display contents to lines
            /// </summary>
            static string[] __ASEP = new string[] { Environment.NewLine };
        }
    }
}
