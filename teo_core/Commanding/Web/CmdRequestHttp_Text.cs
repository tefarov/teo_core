using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

using rx = System.Text.RegularExpressions;

using TEO.General;
using TEO.General.Messaging;

namespace TEO.Commanding.Web
{
    public partial class CmdRequestHttp
    {
        /// <summary>
        /// This processes a response as a text withe the help of some RegEx
        /// </summary>
        public class ProcessorText : AResponseProcessor
        {
            public rx.Regex ReX;
            /// <summary>
            /// If specified will tell the RegEx to return the values only from the group with this id
            /// </summary>
            public string GroupName;
            /// <summary>
            /// This setter will be called each time a needed data is found in a response
            /// </summary>
            public ISetter<string> Output;

            public ProcessorText(CmdRequestHttp item) : base(item) { }
            public override void Process(HttpWebResponse rsp)
            {
                string txt = null;

                using (var rdr = new StreamReader(rsp.GetResponseStream())) {
                    txt = rdr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(txt)) return;
                else if (this.ReX != null) {
                    var emch = this.ReX.Matches(txt);
                    foreach (rx.Match mch in emch) {
                        if (this.Output != null)
                            this.Output.Set(getMatch(mch));
                        else
                            ITM.Message(ITM, new EventArgsMessage(getMatch(mch), TMessage.CommandResult));
                    }
                }
                else {
                    if (this.Output != null)
                        this.Output.Set(txt);
                    else
                        ITM.Message(ITM, new EventArgsMessage(txt));
                }

                string getMatch(rx.Match item)
                {
                    rx.Group grp;

                    if (string.IsNullOrEmpty(this.GroupName)) { }
                    else if ((grp = item.Groups[this.GroupName]) != null) {
                        return grp.Success ? grp.Value : string.Empty;
                    }

                    return item.Value;
                }
            }
            /// <summary>
            /// Implemented from AResponseProcessor.
            /// Is needed for tuning a processor
            /// </summary>
            /// <param name="key">Left side of the CFG-instruction</param>
            /// <param name="value">Right side of the CFG-instruction</param>
            public override void Set(string key, string value)
            {
                if (key == "pattern" && this.ReX == null)
                    this.ReX = new rx.Regex(value);
                else if (key == "group")
                    this.GroupName = value;
            }
        }
    }
}
