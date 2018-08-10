using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding.Language
{
    class Run : Keyword
    {
        Parser PRS;

        public Run(Parser parent, Environment.Environment environment) : base(environment) { PRS = parent.NotNull(); }

        public override IBatchable Create(Input item)
        {
            string fil = null;
            bool onc = false, ask = false;

            if (!item.TrySet( ref fil, 1, "file")) throw new ArgumentException("Codesource file not specified");
            if (string.IsNullOrEmpty(fil)) return null;

            item.TrySet(ref onc, 2, "once");

            ask = fil == Language.KwAsk;    // we will ask the user for a command if he has input word ASK instead of a filename
            onc = onc && !ask;              // we do not use once-mode when asking

            IGetter<string> gcmd = null;
            if (ask) {
                gcmd = new GetterAskConsole("Какой файл запустить:");
                gcmd = new GetterTextFile(gcmd);
            }
            else
                gcmd = new GetterTextFile(fil);

            var itm = new bch_file(this, gcmd);
            if (onc) itm.Filepath = fil; // this will make once-mode work

            return itm;
        }

        /// <summary>
        /// This is a dynamic batch getter. It will return IBatchables, generated from some code-file
        /// on runtime .. i.e. it will read the codefile not when parsed, but at runtime
        /// </summary>
        class bch_file : IBatch
        {
            /// <summary>
            /// Parent parser
            /// </summary>
            Run PRT;
            /// <summary>
            /// Code-source file
            /// </summary>
            IGetter<string> SRC;
            /// <summary>
            /// If we want for a run of specified file in once-mode, store it's filename here.
            /// </summary>
            public string Filepath;

            public bch_file(Run parent, IGetter<string> source)
            {
                PRT = parent.NotNull();
                SRC = source.NotNull();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
            public IEnumerator<IBatchable> GetEnumerator()
            {
                
                // this heppens if the filename is specified and the parser already processed that file
                // and then this instruction should be omitted
                if (!string.IsNullOrEmpty(Filepath) && PRT.PRS.FilesUsed.Contains(Filepath))
                    return Enumerable.Empty<IBatchable>().GetEnumerator();

                return PRT.PRS.Parse(SRC).GetEnumerator();
            }
        }
    }
}
