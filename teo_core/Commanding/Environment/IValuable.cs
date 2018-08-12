using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Environment
{
    /// <summary>
    /// This interface tells that we may set ang get the values of this Variable in a specific way
    /// </summary>
    public interface IValuable
    {
        object Value { get; set; }
    }
}
