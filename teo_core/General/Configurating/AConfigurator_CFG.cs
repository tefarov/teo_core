using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using io = System.IO;

namespace TEO.General.Configurating
{
    /// <summary>
    /// This abstract configurator is a base for developing Configurators that use *.cfg textfiles to configurate objects
    /// </summary>
    public abstract class AConfigurator_CFG : IConfigurator
    {
        string PTH;

        List<input> LVAL = new List<input>();
        string SSN = null;

        public AConfigurator_CFG(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentException("Filepath unspecified");
            if (!io.File.Exists(filepath)) throw new ArgumentException("Configuration file '" + filepath + "' doesn't exist");

            PTH = filepath;
        }

        public virtual void Configure()
        {
            try {
                using (var MSM = new io.MemoryStream()) {

                    using (var stm = new io.FileStream(PTH, io.FileMode.Open)) {
                        stm.CopyTo(MSM);
                    }

                    MSM.Position = 0;

                    using (var rdr = new io.StreamReader(MSM)) {
                        LVAL.Clear();
                        while (!rdr.EndOfStream) {

                            if (this.tryParse(rdr.ReadLine(), out input val))
                                LVAL.Add(val);
                        }
                    }
                }
            }
            catch (Exception ex) { throw new io.FileLoadException("Unable to read configuration file '" + PTH + "'", PTH, ex); }

            var enm = LVAL.GetEnumerator();
            while (enm.MoveNext()) {
                var itm = enm.Current;
                this.setParameter(itm.Section, itm.Key, itm.Value);
            }

        }

        /// <summary>
        /// Textfile-Configurators are unable to save any changes
        /// </summary>
        public virtual void Serialize() { }

        /// <summary>
        /// This will parse a read textline, extracting it's key and value data and storing it to the dictionary if needed
        /// </summary>
        /// <param name="txt">A line to parse</param>
        private bool tryParse(string txt, out input value)
        {
            value = INP_NOVALUE;

            if (string.IsNullOrEmpty(txt))
                return false;

            Match mch;

            // We found some section identifier
            if ((mch = REX_SECTION.Match(txt)).Success) {
                SSN = mch.Value.ToLowerInvariant();
                //SSN = SSN.Substring(1, SSN.Length - 2);
                return false;
            }

            // strip comments
            if (txt.Contains("#")) {

                txt = txt.Replace("&", "&amp;");
                txt = txt.Replace("##", "&shp;");
                txt = REX_COMMENT.Replace(txt, "");
                txt = txt.Replace("&shp;", "#");
                txt = txt.Replace("&amp;", "&");
            }

            if ((mch = REX_LNVALUE.Match(txt)).Success) {
                value = new input() { Section = SSN };

                value.Key = mch.Groups["key"].Value.Trim();
                value.Value = mch.Groups["val"].Value.TrimStart();
                return true;
            }

            return false;
        }

        protected abstract void setParameter(string section, string key, string value);

        private struct input
        {
            public string Section, Key, Value;
        }

        static input INP_NOVALUE = new input();

        static Regex
            REX_SECTION = new Regex(@"(?<=^\s*\[)([a-z0-9\-_.]+)(?=\]\s*$)", RegexOptions.IgnoreCase)
            , REX_COMMENT = new Regex(@"#.*", RegexOptions.IgnoreCase)
            , REX_LNVALUE = new Regex(@"(?<key>[\w._-]+)\s+?(?<val>.+)(?=\s*$)", RegexOptions.IgnoreCase)
            ;
    }
}
