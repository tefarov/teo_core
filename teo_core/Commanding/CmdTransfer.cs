using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding
{
    /// <summary>
    /// This command transfers some object or data from some source to some destination.
    /// </summary>
    /// <typeparam name="T">Type of item to transfer</typeparam>
    public class CmdTransfer<T> : ACommand
    {
        IGetter<T> SRC;
        ISetter<T> DST;

        public CmdTransfer(IGetter<T> source, ISetter<T> destination)
        {
            SRC = source.NotNull();
            DST = destination.NotNull();
        }

        public override ExecuteResult Execute(Context context)
        {
            var dat = SRC.Get();
            DST.Set(dat);

            return new ExecuteResult(true);
        }
    }
}
