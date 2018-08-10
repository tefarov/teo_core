using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TEO.General.Messaging
{
    public class BusIFTT : ABus
    {
        const string
            STR_URLHOST = "https://maker.ifttt.com/trigger/{0}/with/key/{1}"
            ;

        public string Header;

        public BusIFTT(Provider sender, bool isasync = true) : base(sender, isasync) { }

        protected override void send_sync(object sender, EventArgsMessage e)
        {
            this.send(e);
        }
        protected override void send_async(object sender, EventArgsMessage e)
        {
            var trd = new Task(this.send, e);
            trd.Start();
        }
        
        void send(object data)
        {
            TMessage typ = TMessage.None;
            string text = string.Empty;
            string[] aval = { this.Header, "", "" };


            if (data is string) {
                text = (string)data;
            }
            else if (data is EventArgsMessage) {
                var dat = (EventArgsMessage)data;

                typ = dat.Type;
                text = dat.Text;
                aval[2] = dat.Position;
            }
            else {
                text = data.ToString();
            }

            if (typ == TMessage.None) return;
            else if (typ == TMessage.ExceptionCritical) {
                aval[1] = text;
            }
            else if (typ == TMessage.ExceptionBox)
                aval[1] = text;
            else if ((typ & TMessage.MaskMessageBox) > 0)
                aval[2] = text;
            else
                return;

            string url = string.Format(STR_URLHOST, BusIFTT.IfttName, BusIFTT.IfttKey);

            var req = (HttpWebRequest)WebRequest.Create(url);//*/

            req.Accept = "json";
            req.ContentType = "MCorelication/json";
            req.Method = "POST";

            string jsn = "{" +
                " \"value1\" : \"" + aval[0] + "\"" +
                ",\"value2\" : \"" + aval[1] + "\"" +
                ",\"value3\" : \"" + aval[2] + "\"" +
                "}";

            if (req.Method != "GET") {
                var cnt = Encoding.UTF8.GetBytes(jsn);//*/
                req.ContentLength = cnt.LongLength;

                using (var stm = req.GetRequestStream()) {
                    stm.Write(cnt, 0, cnt.Length);
                }
            }

            using (var rsp = (HttpWebResponse)req.GetResponse()) {
                using (var rdr = new System.IO.StreamReader(rsp.GetResponseStream())) {

                }
            }
        }
        public static string IfttKey = null, IfttName = null;
        //public static string IfttKey = "dejqDswNvFDFjPzzzQpg9L", IfttName = "MCore_info";
    }
}
