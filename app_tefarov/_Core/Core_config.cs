using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO;
using TEO.General.Configurating;

namespace APP
{
    public partial class Core
    {
        void iniConfig(string fileid)
        {
            var fil = this.FFiles.Create(fileid);
            var cfg = new cfg_file(this, fil);

            cfg.Configure();
        }
        
        public IConfigurator GetConfigurator(IEnumerable<string> arguments) { return new cfg_arguments(arguments); }
        class cfg_file : AConfigurator_CFG
        {
            readonly Core PRT;

            public cfg_file(Core parent, string filepath) : base(filepath) { PRT = parent; }
            protected override void setParameter(string section, string key, string value)
            {
                if (!string.IsNullOrEmpty(key)) { key = key.ToLowerInvariant(); } else return;

                if (string.IsNullOrEmpty(section) || section == "main") {

                }

                else if (section == "db") {
                    if (key == "host") PRT.DbHost = value;
                    else if (key == "name") PRT.DbName = value;
                }

                else if (section == "macro") {
                    if (false) { }
#if !DEBUG
                    else if (key == "debug") { }

#endif
                    else if (!System.IO.File.Exists(value))
                        Console.WriteLine("File '{0}' not found", value);
                    else
                        PRT.LMAC.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }
        class cfg_arguments : IConfigurator
        {
            IEnumerable<string> EARG;
            public cfg_arguments(IEnumerable<string> arguments) { EARG = arguments.NotNull(); }

            public void Serialize() { throw new NotSupportedException("Argument do not support serialization"); }
            public void Configure()
            {

            }
        }
    }
}
