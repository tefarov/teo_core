using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding.IO
{
    public class CmdReadText : CmdTransfer<string>
    {
        public CmdReadText(IGetter<string> source, ISetter<string> destination) : base(source, destination) { }

        /// <param name="destination">To output filecontents to</param>
        public static IFactory<Input, IBatchable> GetFactory(ISetter<string> destination)
        {
            return new factory(destination);
        }  
        /// <param name="destination">To output filecontents to</param>
        /// <param name="ffiles">
        /// Filepath factory that creates filenames out of filestrings.
        /// Usualy is a part of Program.Core. Otherwise pass new TEO.General.FactoryFiles()
        /// </param>
        public static IFactory<Input, IBatchable> Factory(ISetter<string> destination, IFactory<string> ffiles)
        {
            return new factory(destination) { FFiles = ffiles };
        }

        class factory : IFactory<Input, IBatchable>
        {
            /// <summary>
            /// Filepath factory that creates filenames out of filestrings
            /// </summary>
            public IFactory<string> FFiles;
            public ISetter<string> Destination;

            public factory(ISetter<string> destination)
            {
                this.Destination = destination;
            }

            public IBatchable Create(Input item)
            {
                string src = null, typ = "exception";

                if (!item.TrySet(ref src, 1, "file")) throw new ArgumentException("Не установлено свойство: file(1)");
                if (!item.TrySet(ref typ, 2, "modify")) { }

                var mdf = Helpers.ParseBehaviour(typ);
                IGetter<string> gfil = null;

                if (FFiles != null)
                    gfil = new GetterTextFile(FFiles, src, mdf);
                else
                    gfil = new GetterTextFile(src, mdf);

                return new CmdReadText(gfil, Destination);
            }
        }
    }
}
