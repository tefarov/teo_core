using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.Commanding.Environment;
using mg = TEO.General.Messaging;

namespace APP
{
    public partial class Core
    {
        public string DbHost, DbName;

        public mg.Provider Display = new mg.Provider();
        public mg.ABus BusConsole, BusMsg;

        public Core(string configpath)
        {
            this.Environment.Set("console", this.Display.GetSetter(mg.TMessage.Info));
            this.Environment.Set("msg", this.Display.GetSetter(mg.TMessage.MessageBox));

            iniConfig(configpath);

            this.Environment.Set("log", this.FFiles.Create("log"));
        }

        /// <summary>
        /// List of macro-files specified in a config
        /// </summary>
        List<KeyValuePair<string, string>> LMAC = new List<KeyValuePair<string, string>>();
        public IEnumerable<string> GetMacros(string key)
        {
            key = key.ToLowerInvariant();
            return LMAC.Where(x => x.Key == key).Select(x => x.Value);
        }

        public readonly TEO.Commanding.Environment.Environment Environment = new TEO.Commanding.Environment.Environment();
    }
}
