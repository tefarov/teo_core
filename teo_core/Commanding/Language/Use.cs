using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding.Language
{
    class Use : Keyword
    {
        Parser PRT;

        /// <param name="filesused">A set, describing filenames that have been used while parsing the current batch, to prevent them from being used repeatedly</param>
        public Use(Parser parent, Environment.Environment environment) : base(environment)
        {
            PRT = parent ?? new Parser(environment);
        }

        public override IBatchable Create(Input item)
        {
            string fil = null;
            bool onc = false;

            if (!item.TrySet(ref fil, 1, "file")) throw new ArgumentException("Codesource file not specified");

            if (string.IsNullOrEmpty(fil)) return null;

            if (item.TrySet(ref onc, 2, "once") && PRT.FilesUsed.Contains(fil))
                return null;

            PRT.FilesUsed.Add(fil);

            var gfil = new GetterTextFile(fil);
            return PRT.Parse(gfil);
        }
    }
}
