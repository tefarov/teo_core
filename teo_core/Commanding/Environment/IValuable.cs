using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding.Environment
{
    /// <summary>
    /// Implement this interface to some Variable's value-type to allow special logics for 
    /// getting and setting Variable's values
    /// </summary>
    public interface IValuable
    {
        object Value { get; set; }
    }
}
