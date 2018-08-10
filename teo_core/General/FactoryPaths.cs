using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace TEO.General
{/*
    public class FactoryPaths : IFactory<string>
    {
        protected Dictionary<string, string> DMAP = new Dictionary<string, string>();

        public virtual string Create(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            
            // static shortcut mappings
            if (trymap_preloads(ref path)) { }
            // special folders
            else if (trymap_specials(ref path)) { }

            // STRIP RELATIVITY
            // convert realative paths to direct
            if (string.IsNullOrEmpty(path)) return string.Empty;
            else try_relativity(ref path);

            return path;
        }

        /// <summary>
        /// A filepath may start with some special-folder. modify the input path in that case and return true.
        /// otherwise false.
        /// </summary>
        /// <param name="input">some path to examine and modify if needed</param>
        static bool trymap_specials(ref string input)
        {
            string t = null, p;

            if (input.StartsWith(t = "desktop\\")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }
            else if (input.StartsWith(t = "mydocs\\")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }
            else if (input.StartsWith(t = "recent\\")) {
                p = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                t = input.Substring(t.Length);
                input = Path.Combine(p, t);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Some paths may be shortcuts to something preloaded in a map dictionary.
        /// This tries to map that data.
        /// </summary>
        /// <param name="input">a input string, that will be examined and if that's a shortcut, will be changed</param>
        bool trymap_preloads(ref string input)
        {
            if (!DMAP.ContainsKey(input)) return false;

            input = DMAP[input];
            return true;
        }
        /// <summary>
        /// This converts relative paths to direct ones
        /// </summary>
        static bool try_relativity(ref string input)
        {
            if (input[0] == '\\' && !input.StartsWith(@"\\")) {
                var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                input = Path.Combine(dir, input.Substring(1));
                return true;
            }

            return false;
        }
    } */
}
