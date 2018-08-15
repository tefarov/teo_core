using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using rx = System.Text.RegularExpressions;

namespace TEO
{
    /// <summary>
    /// This factory creates and validates filenames.
    /// It may take a usual path as an input, or some shortcut-path and validate paths.
    /// </summary>
    public class FactoryFiles : IFactory<string>
    {

        protected static Dictionary<string, string> __DMAP = new Dictionary<string, string>();

        static FactoryFiles()
        {
            __DMAP["log.main"] = @"main.log";
            __DMAP["cfg.main"] = @"Config\main.cfg";
        }


        public string Create(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // MMCoreING: 
            // some paths (that change at runtime) are hardcoded ( i.g. last filename or whatever ) 
            if (trymap_hardcoded(ref path)) { }
            // static shortcut mappings
            else if (trymap_preloads(ref path)) { }
            // special folders
            else if (trymap_specials(ref path)) { }

            // STRIP RELATIVITY
            // convert realative paths to direct
            if (string.IsNullOrEmpty(path)) return string.Empty;
            else try_relativity(ref path);

            var file = new FileInfo(path);

            return file.FullName;
        }

        public IGetter<string> IGetter(string path)
        {
            return new GetterFactored<string>(this, path);
        }


        /// <summary>
        /// Some paths might be hardcoded.
        /// This tries to map an input path and if it succedes returns true.
        /// </summary>
        /// <param name="input">The input path. It will be modified if a method succedes</param>
        protected virtual bool trymap_hardcoded(ref string input)
        {


            return false;
        }
        /// <summary>
        /// Some paths may be shortcuts to something preloaded in a static map dictionary.
        /// This tries to map that data.
        /// </summary>
        /// <param name="input">a input string, that will be examined and if that's a shortcut, will be changed</param>
        static bool trymap_preloads(ref string input)
        {
            if (!__DMAP.ContainsKey(input)) return false;

            input = __DMAP[input];
            return true;
        }
        /// <summary>
        /// a filepath may start with some special-folder. modify the input path in that case and return true.
        /// otherwise false.
        /// </summary>
        /// <param name="input">some path to examine and modify if needed</param>
        static bool trymap_specials(ref string input)
        {
            string t = null, p;

            if (input == "temp") {
                input = Path.GetTempFileName();
                return true;
            }
            else if (input.StartsWith(t = "desktop")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }
            else if (input.StartsWith(t = "mydocs")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }
            else if (input.StartsWith(t = "recent")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }

            return false;
        }
        /// <summary>
        /// This converts relative paths to direct ones
        /// </summary>
        static bool try_relativity(ref string input)
        {
            if (System.IO.Path.IsPathRooted(input)) return false;

            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            input = Path.Combine(dir, input);
            return true;
        }
    }
}
