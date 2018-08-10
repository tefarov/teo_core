using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding
{
    /// <summary>
    /// Simply launches some parameterless action
    /// </summary>
    public class CmdSimple : ACommand
    {
        Action ITM;

        public CmdSimple(Action item)
        {
            ITM = item.NotNull();
        }

        public override ExecuteResult Execute(Context context)
        {
            ITM.Invoke();
            return new ExecuteResult(true);
        }
    }
    /// <summary>
    /// Simply launches some parametrized action
    /// </summary>
    public class CmdSimple<T> : ACommand
    {
        Action<T> ITM;
        IGetter<T> ARG;

        public CmdSimple(Action<T> item, IGetter<T> argument)
        {
            ITM = item.NotNull();
            ARG = argument.NotNull();
        }

        public override ExecuteResult Execute(Context context)
        {
            var dat = ARG.Get();
            ITM.Invoke(dat);

            return new ExecuteResult(true);
        }
    }
}
