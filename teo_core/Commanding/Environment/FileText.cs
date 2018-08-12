using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using TEO.General;

namespace TEO.Commanding.Environment
{
    public class FileText : IFile, IGetter<string>, ISetter<string>, IDisposable, IValuable 
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;
        public Encoding Encoding = FileText.DefaultEncoding;

        public readonly bool IsNullable;
        public string Filepath { get; private set; }
        object IValuable.Value { get => this.Get(); set => this.Set(value.ToString()); }

        StreamWriter WRT;

        public FileText(string filepath, bool append, bool isnullabble = false)
        {
            this.IsNullable = isnullabble;
            this.Filepath = filepath.NotNull();

            var fil = new FileInfo(filepath);

            if (!append && fil.Exists)
                fil.Delete();
        }

        public string Get()
        {
            if (!File.Exists(this.Filepath))
                return this.IsNullable ? string.Empty : throw new FileNotFoundException("Файл '" + this.Filepath + "' не найден");

            if (WRT != null)
                this.Dispose();

            return File.ReadAllText(this.Filepath, this.Encoding);
        }
        public void Set(string value)
        {
            if (WRT == null)
                WRT = new StreamWriter(this.Filepath, true, this.Encoding);

            WRT.WriteLine(value);
        }
        public void Dispose()
        {
            WRT.Close();
            WRT.Dispose();
            WRT = null;
        }
    }
}
