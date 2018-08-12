using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;

using TEO.General;
using TEO.General.Messaging;

namespace TEO.Commanding.Web
{
    public partial class CmdRequestHttp : ICommand, ICommandTunnelable, IMessageable
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
        public IGetter<string> Page;
        /// <summary>
        /// Request-Method: GET / POST 
        /// </summary>
        public IGetter<string> Method = new GetterValue<string>() { Value = CmdRequestHttp.MethodDefault };
        /// <summary>
        /// HTTP-Header : Accept
        /// </summary>
        public IGetter<string> Accept = new GetterValue<string>() { Value = CmdRequestHttp.AcceptDefault };
        /// <summary>
        /// HTTP-Header : ContentType
        /// </summary>
        public IGetter<string> ContentT = new GetterValue<string>() { Value = CmdRequestHttp.ContentTDefault };
        /// <summary>
        /// HTTP-Header : User-Agent the one that describes a browser
        /// </summary>
        public IGetter<string> UserAgent = new GetterValue<string>() { Value = CmdRequestHttp.UserAgentDefault };

        /// <summary>
        /// Content-string to be sent to the server if not GET-method
        /// </summary>
        public IGetter<byte[]> Content;

        public Encoding
            EncodingRequest = CmdRequestHttp.EncodingRequestDefault
            , EncodingResponse = CmdRequestHttp.EncodingResponseDefault
            ;

        Environment.Environment ENV;

        public ICommand Next { get; set; }
        public TCommand Type => TCommand.Sequential;

        public AResponseProcessor RepsonseProcessor;

        public event EventHandler<EventArgsMessage> Message;

        public CmdRequestHttp(Environment.Environment environment) { ENV = environment.NotNull(); }

        public ExecuteResult Execute(Context context)
        {
            int att = this.TimeoutAttempts;
            bool has = false;
            byte[] cnt = null;

            retry:
            var req = (HttpWebRequest)WebRequest.Create(this.Page.Get());//*/
            var mtd = this.Method?.Get() ?? "GET";
            var ctt = this.Content?.Get() ?? null;
            
            req.Accept = this.Accept.Get () ;
            req.ContentType = this.ContentT.Get();
            req.Method = this.Method.Get();
            req.UserAgent = this.UserAgent.Get();

            // ... This creates content bytes if any content specified ...
            // We tried to acquire content before, and no need to do it again
            if (has) { }
            // GET-method doesn't support sending content to server
            if (mtd == "GET") { }
            // Content getter might be unspecified
            else if (ctt == null) { }
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
                using (var rsp = (HttpWebResponse)req.GetResponse ()) {

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
        /// <summary>
        /// Some abstract class for processing http-response. 
        /// </summary>
        public abstract class AResponseProcessor
        {
            protected CmdRequestHttp ITM;
            public AResponseProcessor(CmdRequestHttp item) { ITM = item.NotNull(); }

            public abstract void Process(HttpWebResponse response);
            public abstract void Set(string key, string value);
        }
        
        /// <summary>
        /// This is the factory-function for creating http-requests
        /// </summary>
        public static IBatchable Create(Input command , Environment.Environment env )
        {
            var itm = new CmdRequestHttp(env);
            Arg apag, acfg;

            if (command.TrySet(out apag, 1, "page")) 
                itm.Page = apag.GetGetter(env);
            else
                throw new ArgumentException("На певром месте необходимо указать страницу в кавычках");

            if (command.TrySet(out acfg, 3, "config")
                || command.TrySet(out acfg, name: "cfg")) {
                var cfg = acfg.GetGetter(env).Get();
                (new configurator(itm, cfg)).Configure();
            }

            if (command.TrySet(out var avl1, 2, name: "method")) itm.Method = avl1.GetGetter(env);
            if (command.TrySet(out var avl2, name: "accept")) itm.Accept = avl2.GetGetter(env);
            if (command.TrySet(out var avl3, name: "useragent")) itm.UserAgent = avl3.GetGetter(env);
            if (command.TrySet(out var avl4, name: "contenttype")) itm.ContentT = avl4.GetGetter(env);
            // TODO: Parse encoding
            //if (command.TrySet(out var avl5, name: "encoding")) itm.ContentT = avl5.GetGetter(env);

            if (command.TrySet(out var avli1, name: "timeout")) itm.Timeout = avli1.GetGetter(env, Arg.ConverterInt).Get();
            if (command.TrySet(out var avli2, name: "retries")) itm.TimeoutAttempts = avli2.GetGetter(env, Arg.ConverterInt).Get();

            decimal tme = 0;
            if (command.TrySet(ref tme, name: "timeout")) itm.Timeout = (int)(tme * 1000);

            return itm;
        }
        /// <summary>
        /// This configurator implements AConfigurator_CFG that reads .CFG files and applies some configuration to the particular HTTP-request        
        /// </summary>
        class configurator : TEO.General.Configurating.AConfigurator_CFG
        {
            CmdRequestHttp ITM;

            public configurator(CmdRequestHttp item, string filepath) : base(filepath) { ITM = item.NotNull(); }
            protected override void setParameter(string section, string key, string value)
            {
                if (section == "http") {
                    if (key == "page") ITM.Page = new GetterValue<string>() { Value = value };
                    else if (key == "accept") ITM.Accept = new GetterValue<string>() { Value = value };
                    else if (key == "method") ITM.Method = new GetterValue<string>() { Value = value };
                    else if (key == "useragent") ITM.UserAgent = new GetterValue<string>() { Value = value };
                    else if (key == "contenttype") ITM.ContentT = new GetterValue<string>() { Value = value };
                    
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
                        || !(ITM.RepsonseProcessor is ProcessorText)) {

                        ITM.RepsonseProcessor = new ProcessorText(ITM) { Output = ITM.ENV["console"].Convert<ISetter<string>>() };
                    }

                    ITM.RepsonseProcessor.Set(key, value);
                }
                else if (section == "postprocessor.dom") {
                    if (ITM.RepsonseProcessor == null
                        || !(ITM.RepsonseProcessor is ProcessorDom)) {

                        ITM.RepsonseProcessor = new ProcessorDom(ITM) { Output = ITM.ENV["console"].Convert<ISetter<string>>() };
                    }
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
        /// <summary>
        /// This class is responsible for encoding some text to binary for the needs of sending that data to the server
        /// </summary>
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
