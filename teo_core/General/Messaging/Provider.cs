using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Messaging
{
    public class Provider : IDisposable 
    {
        const int
            // Timespans to wait for next cycle in busy, normal and lazy mode
            INT_SPNBUSY = 10
            , INT_SPNNORM = 100
            , INT_SPNLAZY = 500

            // If we didn't find any messages this number of times, goto normal mode
            , INT_COUTNRM = 200
            // If we didn't find any messages this number of times, goto lazy mode
            , INT_COUTLAZ = 220
            ;

        /// <summary>
        /// Count the times when we didn't find any messages to display
        /// </summary>
        int CMG = 1;
        /// <summary>
        /// Timespan to sleep
        /// </summary>
        int SPN = INT_SPNBUSY;

        public event EventHandler<EventArgsMessage> Occur;

        System.Collections.Concurrent.ConcurrentQueue<msg_event> QMSG = new System.Collections.Concurrent.ConcurrentQueue<msg_event>();
        public Provider()
        {
            QMSG = new System.Collections.Concurrent.ConcurrentQueue<msg_event>();
            var tsk = new Task(this.exe);
            tsk.Start();
        }
        void exe()
        {
            while (CMG > 0) {
                if (QMSG.Any()) {
                    CMG = 1; SPN = INT_SPNBUSY;

                    if (QMSG.TryDequeue(out var msg))
                        this.Occur(msg.Sender, msg.Args);
                }
                else if (++CMG > int.MaxValue - 10) CMG = INT_SPNLAZY;
                else if (CMG > INT_COUTLAZ) SPN = INT_SPNLAZY;
                else if (CMG > INT_COUTNRM) SPN = INT_SPNNORM;

                System.Threading.Thread.Sleep(SPN);
            }
        }

        public void Write(object sender, EventArgsMessage e)
        {
            QMSG.Enqueue(new msg_event(sender, e));
        }
        public void Write(string text, TMessage type, string position = null)
        {
            QMSG.Enqueue(new msg_event(this, new EventArgsMessage { Text = text, Type = type, Position = position }));
        }
        public void Write(Exception ex, TMessage type = TMessage.ExceptionWorkflow, string position = null)
        {
            if (this.Occur == null) { }
            else if ((type & TMessage.MaskException) == 0) {
                throw new ArgumentException("type should mask with TMessage.MaskException");
            }
            else {
                string txt = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
                QMSG.Enqueue(new msg_event(this, new EventArgsMessage { Text = txt, Type = type, Position = position }));
            } }

        void IDisposable.Dispose()
        {
            CMG = 0;
        }

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

        class msg_event
        {
            public msg_event(object sender, EventArgsMessage e) { this.Sender = sender; this.Args = e; }
            public readonly object Sender;
            public readonly EventArgsMessage Args;
        }
    }
}
