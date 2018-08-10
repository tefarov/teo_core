using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public interface ICommand : IBatchable
    {
        TCommand Type { get; }
        ExecuteResult Execute(Context context);
    }
}
