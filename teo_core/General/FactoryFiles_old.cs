using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using rx = System.Text.RegularExpressions;

namespace TEO.General
{
    /// <summary>
    /// This factory creates and validates filenames.
    /// It may take a usual path as an input, or some shortcut-path and validate paths.
    /// </summary>
    public class FactoryFiles_old : IFactory<string>
    {
        /// <summary>
        /// The factory deals with files to read and files to write a bit differently.
        /// 
        /// </summary>
        public enum Mode : byte
        {
            Read = 0x01,
            Write = 0x02
        }

        public enum Behaviour : byte
        {
            MaskRead = 0b01_000000,
            MaskWrite = 0b10_000000,

            // <summary>This will not check for validity</summary>
            //NoCheck = 0b00_000000 | MaskWrite | MaskRead,
            /// <summary>This will return an empty string if conflict</summary>
            Empty = 0b00_000010 | MaskWrite | MaskRead,
            /// <summary>This will return the filepath even in case of conflict</summary>
            Ignore = 0b00_000001 | MaskWrite | MaskRead,
            /// <summary>Throw error if conflict</summary>
            Exception = 0b00_100000 | MaskWrite | MaskRead,

            /// <summary>Rename a current file if conflict</summary>
            Rename = 0b00_000001 | MaskWrite,
            /// <summary>Delete the other file if conflict</summary>
            Overwrite = 0b00_000010 | MaskWrite,

            /// <summary>Create a file if not exists</summary>
            Create = 0b00_000001 | MaskRead
        }

        protected static Dictionary<string, string> __DMAP = new Dictionary<string, string>();

        protected validation VLD;
        protected modifcation MDF;

        public FactoryFiles_old(Mode mode, Behaviour modifying)
        {
            VLD = get_validation(mode);
            MDF = get_modification(mode, modifying);
        }
        static FactoryFiles_old()
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

            if (VLD(file)) return file.FullName;
            if (MDF != null) return MDF(file);

            return file.FullName;
        }

        public IGetter<string> IGetter(string path)
        {
            return new GetterFactored<string>(this, path);
        }

        #region Modification-behaviour
        protected delegate string modifcation(FileInfo file);
        protected static string modify_ignore(FileInfo file) { return file.FullName; }
        protected static string modify_exception(FileInfo file) { throw new IOException("File '" + file.FullName + "' invalid"); }
        protected static string modify_empty(FileInfo file) { return string.Empty; }
        protected static string modify_overwrite(FileInfo file)
        {
            file.Delete();
            return file.FullName;
        }
        static string modify_rename(FileInfo file)
        {
            // maximum number of times to rename a file
            const int INT_MAXITER = 999;

            string dir, nam, ext, pth;
            int i = 0;

            dir = file.DirectoryName;
            ext = file.Extension;
            nam = file.Name;

            if (nam.EndsWith(ext)) nam = nam.Substring(0, nam.Length - ext.Length);

            do {
                if (++i > INT_MAXITER) throw new ArgumentException("Больше переименовывать файл '" + file.FullName + "' не имеет смысла");
                pth = Path.Combine(dir, nam + "_" + i.ToString("000") + ext);
            }
            while (File.Exists(pth)); // I know VLD(pth) should be here, but we are static and may not use instance fields :-((
            return pth;
        }

        static modifcation get_modification(Mode mode, Behaviour behaviour)
        {
            if (mode == Mode.Read && (behaviour & Behaviour.MaskRead) == 0) throw new ArgumentException(string.Format("Validation behavior '{1}' doesn't comply with workmode '{0}'", mode, behaviour));
            if (mode == Mode.Write && (behaviour & Behaviour.MaskWrite) == 0) throw new ArgumentException(string.Format("Validation behavior '{1}' doesn't comply with workmode '{0}'", mode, behaviour));

            if (behaviour == Behaviour.Ignore) return modify_ignore;
            else if (behaviour == Behaviour.Exception) return modify_exception;
            else if (behaviour == Behaviour.Overwrite) return modify_overwrite;
            else if (behaviour == Behaviour.Rename) return modify_rename;
            else if (behaviour == Behaviour.Create) throw new NotImplementedException("Creating of non-existing files not yet implemented");
            else if (behaviour == Behaviour.Empty) return modify_empty;

            throw new ArgumentException("Unknown validation-mode specified: " + behaviour.ToString());
        }
        #endregion

        #region Validation 
        protected delegate bool validation(FileInfo file);
        protected static validation get_validation(Mode mode)
        {
            if (mode == Mode.Read) return x => x.Exists;
            if (mode == Mode.Write) return x => !x.Exists;

            throw new ArgumentException("Unknow mode: " + mode.ToString());
        }
        #endregion

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

        public static Behaviour Parse(string name)
        {
            if (name == "overwrite") return Behaviour.Overwrite;
            else if (name == "rename") return Behaviour.Rename;
            else if (name == "ignore") return Behaviour.Ignore;
            else if (name == "exception") return Behaviour.Exception;
            else if (name == "create") return Behaviour.Create;

            return Behaviour.Exception;
        }

        public static string ToFilepath(string path, Mode mode = Mode.Read, Behaviour behaviour = Behaviour.Exception)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            // MAPPING: 
            // some paths (that change at runtime) are hardcoded ( i.g. last filename or whatever ) 
            if (trymap_preloads(ref path)) { }
            // special folders
            else if (trymap_specials(ref path)) { }

            // STRIP RELATIVITY
            // convert realative paths to direct
            if (string.IsNullOrEmpty(path)) return string.Empty;
            else try_relativity(ref path);

            var isvalid = get_validation(mode);
            var modify = get_modification(mode, behaviour);
            var file = new FileInfo(path);

            if (isvalid(file)) return file.FullName;
            if (modify != null) return modify(file);

            return file.FullName;
        }
    }
}
