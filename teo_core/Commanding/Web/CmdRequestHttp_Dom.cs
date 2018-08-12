using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

using AngleSharp.Dom;
using AngleSharp.Parser.Html;

using TEO.General;
using TEO.General.Messaging;

namespace TEO.Commanding.Web
{
    public partial class CmdRequestHttp
    {
        /// <summary>
        /// This processor uses AngleSharp library for processing HTML-response as Dom 
        /// </summary>
        public class ProcessorDom : AResponseProcessor
        {
            /// <summary>
            /// IsActive : if true, tells that we should read a response, otherwise just process whatever saved in RSP
            /// </summary>
            bool ISA = true;
            /// <summary>
            /// Response string
            /// </summary>
            string RSP = null;
            /// <summary>
            /// Selector-strings that will be used to create selectors
            /// </summary>
            string SEL = null;

            Func<IElement, string> Extractor = x => x.TextContent;

            public ISetter<string> Output;

            public ProcessorDom(CmdRequestHttp owner) : base(owner) { }

            public override void Process(HttpWebResponse rsp)
            {
                // acquire data if it hasn't been done yet
                if (ISA) {
                    using (var rdr = new StreamReader(rsp.GetResponseStream()))
                        RSP = rdr.ReadToEnd();
                    ISA = false;
                }
                if (SEL == null) SEL = "";

                var prs = new HtmlParser();
                var doc = prs.Parse(RSP);

                var enod = doc.QuerySelectorAll(SEL);
                var etxt = enod.Select(this.Extractor);

                foreach (var mch in etxt) {
                    if (this.Output != null)
                        this.Output.Set(mch);
                    else
                        ITM.Message(ITM, new EventArgsMessage(mch, TMessage.CommandResult));
                }
            }
            public override void Set(string key, string value)
            {
                if (key == "pattern")
                    SEL = value;
                else if (key == "attribute")
                    this.Extractor = x => x.Attributes[value].Value;
            }
        }
    }
}
