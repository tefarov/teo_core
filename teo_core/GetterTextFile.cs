using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace TEO
{
    public class GetterTextFile : IGetter<string>
    {
        public static Encoding EncodingDefault = Encoding.UTF8;

        IGetter<string> GPTH;
        IFactory<string> FPTH;
        modifcation modify;

        public Encoding Encoding = GetterTextFile.EncodingDefault;
        public string Filepath;

        public GetterTextFile(string filepath, Behaviour modifier = Behaviour.Exception)
        {
            Filepath = filepath.NotNull("Filepath is empty");
            this.modify = this.get_modification(modifier);
        }
        public GetterTextFile(IGetter<string> filepath, Behaviour modifier = Behaviour.Exception)
        {
            GPTH = filepath.NotNull();
            this.modify = this.get_modification(modifier);
        }
        public GetterTextFile(IFactory<string> filepath, string file, Behaviour modifier = Behaviour.Exception)
        {
            FPTH = filepath.NotNull();
            Filepath = file.NotNull("Filepath unset");
            this.modify = this.get_modification(modifier);
        }

        public string Get()
        {
            string pth = Filepath;

            if (GPTH != null)
                pth = GPTH.Get();
            else if (FPTH != null)
                pth = FPTH.Create(pth);

            var fil = new FileInfo(pth);

            if (!fil.Exists) pth = modify(fil);

            using (var stm = new StreamReader(pth, this.Encoding)) {
                return stm.ReadToEnd();
            }
        }

        #region Modification-behaviour
        protected delegate string modifcation(FileInfo file);
        protected static string modify_ignore(FileInfo file) { return file.FullName; }
        protected static string modify_exception(FileInfo file) { throw new IOException("File '" + file.FullName + "' invalid"); }
        protected static string modify_empty(FileInfo file) { return string.Empty; }
        protected string modify_create(FileInfo file)
        {
            using (var wrt = File.Create(file.FullName))
                return file.FullName;
        }
        protected modifcation get_modification(Behaviour modifier)
        {
            if ((modifier & Behaviour.MaskRead) == 0) throw new ArgumentException("Modifier '" + modifier.ToString() + "' should intersect Behaviour.MaskRead");

            if (modifier == Behaviour.Exception) return GetterTextFile.modify_exception;
            else if (modifier == Behaviour.Empty) return GetterTextFile.modify_empty;
            else if (modifier == Behaviour.Create) return modify_create;
            else if (modifier == Behaviour.Ignore) return GetterTextFile.modify_ignore;

            throw new ArgumentException("Unsupported argument " + modifier.ToString());
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}({1})", this.GetType().Name, Filepath);
        }
    }
}
