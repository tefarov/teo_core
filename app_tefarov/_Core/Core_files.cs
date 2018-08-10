using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO;
using TEO.General;

namespace APP
{
    public partial class Core
    {
        fct_files FFIL = new fct_files();
        
        /// <summary>
        /// File-factory that creates absolute filenames out of file-strings
        /// </summary>
        public IFactory<string> FFiles { get { return FFIL; } }
        /// <summary>
        /// Creates a getter, that will return contents of a text-file
        /// </summary>
        /// <param name="file">file string out of which to create a path</param>
        /// <param name="behavior">What to do in case of conflict</param>
        public IGetter<string> GFileContent_TXT(string file, Behaviour behavior = Behaviour.Exception)
        {
            return new GetterTextFile(FFIL, file, behavior);
        }

        class fct_files : FactoryFiles
        {
            static fct_files()
            {
                __DMAP["teo.ini"] = @"Macros\ini.teo";
                __DMAP["teo.debug"] = @"Macros\debug.teo";
                __DMAP["log"] = @"main.log";
            }
        }
    }
}
