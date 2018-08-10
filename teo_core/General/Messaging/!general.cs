using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General.Messaging
{
    public enum TMessage : byte
    {
        None = 0,

        UserInput = 0b0000_0001,

        MaskOperation = 0b0001_0000,
        MaskInline = 0b0010_0000,
        MaskException = 0b0100_0000,
        MaskSystem = 0b1000_0000,
        MaskMessageBox = 0b0000_1000,

        InlineText = 0b0000_0001 | MaskInline,
        Progress = 0b0000_0010 | MaskInline,

        Info = 0b0000_0001 | MaskOperation,
        CommandHeader = 0b0000_0100 | MaskOperation,
        CommandResult = 0b0000_0110 | MaskOperation,
        /// <summary>
        /// This one occurs when the entire command-batch has been processed and we need to output the Report
        /// </summary>
        CommandReport = 0b0000_0111 | MaskOperation,
        MessageBox = MaskMessageBox | MaskOperation,

        QueryData = 0b0000_0001 | MaskSystem,

        Warning = 0b0000_0001 | MaskException,
        ExceptionWorkflow = 0b0000_0010 | MaskException,
        ExceptionCritical = 0b0000_0011 | MaskException,
        ExceptionBox = MaskMessageBox | MaskException
    }

    public class EventArgsMessage : EventArgs
    {
        public TMessage Type;
        public string Text, Position;

        public EventArgsMessage() { }
        public EventArgsMessage(string text, TMessage type = TMessage.Info)
        {
            this.Type = type;
            this.Text = text;
        }
        public EventArgsMessage(Exception ex, string position = null, TMessage type = TMessage.ExceptionWorkflow)
        {
            if ((type & TMessage.MaskException) == 0) throw new ArgumentException("type should mask with TMessage.MaskException");
            else {
                this.Text = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
                this.Type = type;
                this.Position = position;
            }
        }
    }
}
