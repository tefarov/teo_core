using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TEO.General;
using TEO.General.Messaging;

namespace TEO.Commanding
{
    public class CmdExecute : ACommand
    {
        IGetter<string> GPTH;

        public CmdExecute(IGetter<string> filepath)
        {
            GPTH = filepath.NotNull();
        }

        public override ExecuteResult Execute(Context context)
        {
            string cmd, args = null;
            string pth = GPTH.Get();

            if (string.IsNullOrEmpty(pth)) throw new ExceptionRuntime("Ничего не запущено");

            if (File.Exists(pth)) { cmd = pth; }
            else if (Directory.Exists(pth)) { cmd = pth; }
            else {
                //throw new FileNotFoundException("Путь не найден: " + filepath);
                pth = pth.Trim();
                int pos = pth.IndexOf(' ');
                if (pos < 1)
                    cmd = pth;
                else {
                    cmd = pth.Substring(0, pos);
                    args = pth.Substring(pos);
                }
            }

            System.Diagnostics.Process.Start(cmd, args);
            return new ExecuteResult(true);
        }
    }
}
