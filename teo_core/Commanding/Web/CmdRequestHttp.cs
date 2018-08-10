using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using rx = System.Text.RegularExpressions;

using TEO.General;
using TEO.General.Messaging;

namespace TEO.Commanding.Web
{
    public class CmdRequestHttp : ICommand, ICommandTunnelable, IMessageable
    {
        /// <summary>
        /// This array simbolizes no Content-Data
        /// </summary>
        static byte[] ContentEmpty = new byte[] { 0xFE };

        /// <summary>
        /// Default timeout period for a Command
        /// </summary>
        public static int TimeoutDefault = 15_000;
        /// <summary>
        /// Default timeout attempts per one request
        /// </summary>
        public static int TimeoutAttemptsDefault = 10;

        /// <summary>
        /// Default HTTP-Header Accept
        /// </summary>
        public static string AcceptDefault = "text/html";
        /// <summary>
        /// Default HTTP-Header ContentType
        /// </summary>
        public static string ContentTDefault = "text/plain";
        /// <summary>
        /// Default request method
        /// </summary>
        public static string MethodDefault = "GET";
        /// <summary>
        /// Default HTTP-Header User-Agent
        /// </summary>
        public static string UserAgentDefault = "teo_core";

        public static Encoding
            EncodingRequestDefault = Encoding.UTF8
            , EncodingResponseDefault = Encoding.UTF8
            ;

        public bool
            IsOutputStatus = false
            , IsOutputResponse = false
            ;

        public int
            Timeout = CmdRequestHttp.TimeoutDefault,
            TimeoutAttempts = CmdRequestHttp.TimeoutAttemptsDefault
            ;

        /// <summary>
        /// Fully qulified page-address to send request to
        /// </summary>
        public string Page;
        /// <summary>
        /// Request-Method: GET / POST 
        /// </summary>
        public string Method = CmdRequestHttp.MethodDefault;
        /// <summary>
        /// HTTP-Header : Accept
        /// </summary>
        public string Accept = CmdRequestHttp.AcceptDefault;
        /// <summary>
        /// HTTP-Header : ContentType
        /// </summary>
        public string ContentT = CmdRequestHttp.ContentTDefault;
        /// <summary>
        /// HTTP-Header : User-Agent the one that describes a browser
        /// </summary>
        public string UserAgent = CmdRequestHttp.UserAgentDefault;

        /// <summary>
        /// Content-string to be sent to the server if not GET-method
        /// </summary>
        public IGetter<byte[]> Content;

        public Encoding
            EncodingRequest = CmdRequestHttp.EncodingRequestDefault
            , EncodingResponse = CmdRequestHttp.EncodingResponseDefault
            ;

        public ICommand Next { get; set; }
        public TCommand Type => TCommand.Sequential;

        public AResponseProcessor RepsonseProcessor;

        public event EventHandler<EventArgsMessage> Message;

        public ExecuteResult Execute(Context context)
        {
            int att = this.TimeoutAttempts;
            bool has = false;
            byte[] cnt = null;

            retry:
            var req = (HttpWebRequest)WebRequest.Create(this.Page);//*/

            req.Accept = this.Accept;
            req.ContentType = this.ContentT;
            req.Method = this.Method;
            req.UserAgent = this.UserAgent;

            // ... This creates content bytes if any content specified ...
            // We tried to acquire content before, and no need to do it again
            if (has) { }
            // GET-method doesn't support sending content to server
            if (this.Method == "GET") { }
            // Content getter might be unspecified
            else if (this.Content == null) { }
            // some content has been acquired before retry 
            // or there's some significant data got now
            else if (
                cnt != null
                || (cnt = this.Content.Get()) != null) {

                req.ContentLength = cnt.LongLength;
                using (var stm = req.GetRequestStream())
                    stm.Write(cnt, 0, cnt.Length);

            }

            has = true; // signalize no need to acquire content once again

            try {
                using (var rsp = (HttpWebResponse)req.GetResponse()) {

                    if (this.IsOutputStatus) this.Message(this, new EventArgsMessage(rsp.StatusDescription, TMessage.QueryData));

                    if (this.RepsonseProcessor != null) {
                        this.RepsonseProcessor.Process(rsp);
                        return new ExecuteResult(true);
                    }

                    // Try processing a response as a text
                    // Just display it
                    using (var rdr = new StreamReader(rsp.GetResponseStream(), this.EncodingResponse)) {
                        string txt = rdr.ReadToEnd();

                        //if (this.IsOutputResponse)
                        this.Message(this, new EventArgsMessage(txt, TMessage.QueryData));

                        return new ExecuteResult(true);
                    }
                }
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.Timeout && --att > 0)
                    goto retry;

                throw;
            }
        }

        public abstract class AResponseProcessor
        {
            protected CmdRequestHttp ITM;
            public AResponseProcessor(CmdRequestHttp item) { ITM = item.NotNull(); }

            public abstract void Process(HttpWebResponse response);
            public abstract void Set(string key, string value);
        }
        public class ProcessorText : AResponseProcessor
        {
            public rx.Regex ReX;
            public string GroupName;
            
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
                        ITM.Message(ITM, new EventArgsMessage(getMatch(mch), TMessage.CommandResult));
                    }
                }
                else
                    ITM.Message(ITM, new EventArgsMessage(txt));


                string getMatch ( rx.Match item )
                {
                    rx.Group grp;

                    if (string.IsNullOrEmpty(this.GroupName)) { }
                    else if ((grp = item.Groups[this.GroupName]) != null) {
                        return grp.Success ? grp.Value : string.Empty;
                    }

                    return item.Value;
                }
            }
            public override void Set(string key, string value)
            {
                if (key == "pattern" && this.ReX == null)
                    this.ReX = new rx.Regex(value);
                else if (key == "group")
                    this.GroupName = value;
            }
        }

        public static IBatchable Create(Input command , Environment.Environment env )
        {
            var itm = new CmdRequestHttp();
            string enc = null, cfg = null;

            if (command.TrySet(ref cfg, 3, "config")
                || command.TrySet(ref cfg, 3, "cfg")) {
                (new configurator(itm, cfg)).Configure();
            }

            if (!command.TrySet(ref itm.Page, name: "page"))
                throw new ArgumentException("Webpage not specified");

            command.TrySet(ref itm.Method, name: "method");
            command.TrySet(ref itm.Accept, name: "accept");
            command.TrySet(ref itm.ContentT, name: "contenttype");
            command.TrySet(ref itm.UserAgent, name: "useragent");

            decimal tme = 0;
            if (command.TrySet(ref tme, name: "timeout")) itm.Timeout = (int)(tme * 1000);

            // TODO: Parse encoding
            command.TrySet(ref enc, name: "encoding");

            return itm;
        }
        class configurator : TEO.General.Configurating.AConfigurator_CFG
        {
            CmdRequestHttp ITM;

            public configurator(CmdRequestHttp item, string filepath) : base(filepath) { ITM = item.NotNull(); }
            protected override void setParameter(string section, string key, string value)
            {
                if (section == "http") {
                    if (key == "page") ITM.Page = value;
                    else if (key == "accept") ITM.Accept = value;
                    else if (key == "contenttype") ITM.ContentT = value;
                    else if (key == "method") ITM.Method = value;
                    else if (key == "useragent") ITM.UserAgent = value;

                    else if (key == "timeout" && int.TryParse(value, out int tme)) ITM.Timeout = tme;
                    else if (key == "encoding") {
                        this.TrySet(value, ref ITM.EncodingRequest);
                        this.TrySet(value, ref ITM.EncodingResponse);
                    }
                    else if (key == "encoding.req")
                        this.TrySet(value, ref ITM.EncodingRequest);
                    else if (key == "encoding.rsp")
                        this.TrySet(value, ref ITM.EncodingResponse);
                    else if (key == "content")
                        ITM.Content = new encoder(value);
                }
                else if (section == "postprocessor.regex") {
                    if (ITM.RepsonseProcessor == null
                        || !(ITM.RepsonseProcessor is ProcessorText))
                        ITM.RepsonseProcessor = new ProcessorText(ITM);

                    ITM.RepsonseProcessor.Set(key, value);
                }
            }

            bool TrySet(string name, ref Encoding item) {
                try {
                    item = Encoding.GetEncoding(name);
                    return true;
                }
                catch { return false; }
            }
        }
        class encoder : IGetter<byte[]> {
            public string Text;
            public Encoding Encoding = CmdRequestHttp.EncodingRequestDefault;

            public encoder(string text, Encoding encoding = null)
            {
                this.Text = text;
                if (encoding != null) this.Encoding = encoding;
            }

            public byte[] Get()
            {
                return this.Encoding.GetBytes(this.Text);
            }
        }
    }
}
